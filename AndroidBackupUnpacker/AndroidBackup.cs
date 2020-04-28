using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AndroidBackupUnpacker.Exceptions;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace AndroidBackupUnpacker
{
    public class AndroidBackup : IDisposable
    {
        private readonly MemoryStream _backupFileStream;

        public BackupAttributes Attributes { get; }

        public AndroidBackup(MemoryStream backupFileStream)
        {
            this._backupFileStream = backupFileStream;

            this.Attributes = ExtractAttributesFromBackup();
        }

        public void Dispose()
        {
            this._backupFileStream.Close();
        }

        private BackupAttributes ExtractAttributesFromBackup()
        {
            this._backupFileStream.Position = 0;

            var backupAttributes = new BackupAttributes
            {
                Header = this._backupFileStream.ReadOneLine(), // 1. Line
                BackupVersion = this._backupFileStream.ReadOneLine(), // 2. Line
                IsCompressed = this._backupFileStream.ReadOneLine() == "1" ? true : false // 3. Line
            };

            var encryptionType = this._backupFileStream.ReadOneLine(); // 4. Line
            backupAttributes.EncryptionType = encryptionType switch
            {
                "none" => EncryptionType.None,
                "AES-256" => EncryptionType.AES256,
                _ => EncryptionType.Unknown,
            };

            if (backupAttributes.EncryptionType == EncryptionType.AES256)
            {
                backupAttributes.UserPasswordSalt = this._backupFileStream.ReadOneLine(); // 5. Line
                backupAttributes.MasterKeySalt = this._backupFileStream.ReadOneLine(); // 6. Line
                backupAttributes.Iterations = int.Parse(this._backupFileStream.ReadOneLine()); // 7. Line
                backupAttributes.UserIVKey = this._backupFileStream.ReadOneLine(); // 8. Line
                backupAttributes.MasterKeyEncrypted = this._backupFileStream.ReadOneLine(); // 9. Line
            }

            return backupAttributes;
        }

        public MemoryStream GetTarStream(string password = "")
        {
            if (this.Attributes.EncryptionType == EncryptionType.AES256 && string.IsNullOrWhiteSpace(password))
            {
                throw new NoPasswordProvidedException();
            }

            this._backupFileStream.Position = 0;

            // Skip android backup headers (not encrypted 4, encrypted 9 lines)
            var skip = this.Attributes.EncryptionType == EncryptionType.None ? 4 : 9;
            for (var i = 0; i < skip; i++)
            {
                this._backupFileStream.ReadOneLine();
            }

            MemoryStream inputStream;
            if (this.Attributes.EncryptionType == EncryptionType.AES256)
            {
                inputStream = DecryptedStream(password);
            }
            else
            {
                inputStream = this._backupFileStream;
            }

            var inflaterInputStream = new InflaterInputStream(inputStream);
            var outputMemoryStream = new MemoryStream();

            inflaterInputStream.CopyTo(outputMemoryStream);

            outputMemoryStream.Position = 0;
            return outputMemoryStream;
        }

        // backup is encrypted with master key (utf8)
        // master key is encrypted with user password (ascii)
        private MemoryStream DecryptedStream(string password)
        {
            var userSalt = Helpers.HexToByteArray(this.Attributes.UserPasswordSalt);
            var masterKeySalt = Helpers.HexToByteArray(this.Attributes.MasterKeySalt);
            var userIV = Helpers.HexToByteArray(this.Attributes.UserIVKey);
            var encryptedMasterKey = Helpers.HexToByteArray(this.Attributes.MasterKeyEncrypted);

            var userKey = GetUserPasswordBytes(password, userSalt, this.Attributes.Iterations);

            byte[] masterKeyBlob;
            try
            {
                masterKeyBlob = Encryption.AESDecrypt(encryptedMasterKey, userKey, userIV);
            }
            catch (CryptographicException ex)
            {
                throw new WrongPasswordException(ex);
            }

            var masterKeyBlobStream = new MemoryStream(masterKeyBlob);
            var masterKeyIV = masterKeyBlobStream.ReadBlobPart();
            var masterKey = masterKeyBlobStream.ReadBlobPart();
            var masterKeyChecksum = masterKeyBlobStream.ReadBlobPart();
            masterKeyBlobStream.Close();

            var calculatedCk = GenerateKeyChecksum(masterKey, masterKeySalt, this.Attributes.Iterations);

            if (calculatedCk.SequenceEqual(masterKeyChecksum) == false)
            {
                throw new ChecksumFailedException();
            }

            var decryptedStream = Encryption.AESDecryptStream(this._backupFileStream, masterKey, masterKeyIV);
            var outputMemoryStream = new MemoryStream();

            decryptedStream.CopyTo(outputMemoryStream);

            outputMemoryStream.Position = 0;
            return outputMemoryStream;
        }

        private byte[] GetUserPasswordBytes(string password, byte[] salt, int iterations)
        {
            return Encryption.PBKDF2(ASCIIEncoding.ASCII.GetBytes(password), salt, iterations);
        }

        private byte[] GenerateKeyChecksum(byte[] data, byte[] salt, int iterations)
        {
            return Encryption.PBKDF2(Helpers.ToJavaUTF8ByteArray(data), salt, iterations);
        }
    }
}
