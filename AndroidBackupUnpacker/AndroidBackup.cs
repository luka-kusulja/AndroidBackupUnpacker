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
        private MemoryStream backupFileStream;

        public BackupAttributes Attributes { get; }

        public AndroidBackup(MemoryStream _backupFileStream)
        {
            this.backupFileStream = _backupFileStream;

            this.Attributes = ExtractAttributesFromBackup();
        }

        public void Dispose()
        {
            this.backupFileStream.Close();
        }

        private BackupAttributes ExtractAttributesFromBackup()
        {
            this.backupFileStream.Position = 0;

            var backupAttributes = new BackupAttributes();
            
            backupAttributes.Header = this.backupFileStream.ReadOneLine(); // 1. Line
            backupAttributes.BackupVersion = this.backupFileStream.ReadOneLine(); // 2. Line

            var isCompressed = this.backupFileStream.ReadOneLine(); // 3. Line
            backupAttributes.IsCompressed = isCompressed == "1" ? true : false;

            var encryptionType = this.backupFileStream.ReadOneLine(); // 4. Line
            switch (encryptionType)
            {
                case "none":
                    backupAttributes.EncryptionType = EncryptionType.None;
                    break;
                case "AES-256":
                    backupAttributes.EncryptionType = EncryptionType.AES256;
                    break;
                default:
                    backupAttributes.EncryptionType = EncryptionType.Unknown;
                    break;
            }

            if(backupAttributes.EncryptionType == EncryptionType.AES256)
            {
                backupAttributes.UserPasswordSalt = this.backupFileStream.ReadOneLine(); // 5. Line
                backupAttributes.MasterKeySalt = this.backupFileStream.ReadOneLine(); // 6. Line
                backupAttributes.Iterations = int.Parse(this.backupFileStream.ReadOneLine()); // 7. Line
                backupAttributes.UserIVKey = this.backupFileStream.ReadOneLine(); // 8. Line
                backupAttributes.MasterKeyEncrypted = this.backupFileStream.ReadOneLine(); // 9. Line
            }

            return backupAttributes;
        }

        public MemoryStream GetTarStream(string password = "")
        {
            if(this.Attributes.EncryptionType == EncryptionType.AES256 && string.IsNullOrWhiteSpace(password))
            {
                throw new NoPasswordProvidedException();
            }

            this.backupFileStream.Position = 0;

            // Skip android backup headers (not encrypted 4, encrypted 9 lines)
            int skip = this.Attributes.EncryptionType == EncryptionType.None ? 4 : 9;
            for (int i = 0; i < skip; i++)
            {
                this.backupFileStream.ReadOneLine();
            }

            MemoryStream inputStream;
            if(this.Attributes.EncryptionType == EncryptionType.AES256)
            {
                inputStream = DecryptedStream(password);
            }
            else
            {
                inputStream = this.backupFileStream;
            }

            using (var inflaterInputStream = new InflaterInputStream(inputStream))
            {
                var outputMemoryStream = new MemoryStream();
                
                inflaterInputStream.CopyTo(outputMemoryStream);
                
                outputMemoryStream.Position = 0;
                return outputMemoryStream;
            }
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

            using (var decryptedStream = Encryption.AESDecryptStream(this.backupFileStream, masterKey, masterKeyIV))
            {
                var outputMemoryStream = new MemoryStream();

                decryptedStream.CopyTo(outputMemoryStream);

                outputMemoryStream.Position = 0;
                return outputMemoryStream;
            }
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
