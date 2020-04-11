using System.IO;
using System.Security.Cryptography;

namespace AndroidBackupUnpacker
{
    internal static class Encryption
    {
        internal static byte[] PBKDF2(byte[] data, byte[] salt, int iterations)
        {
            var PBKDF2 = new Rfc2898DeriveBytes(data, salt, iterations);
            return PBKDF2.GetBytes(32);
        }

        internal static byte[] AESDecrypt(byte[] encryptedData, byte[] key, byte[] iv)
        {
            var inputMemoryStream = new MemoryStream(encryptedData);
            return AESDecryptStream(inputMemoryStream, key, iv).ReadUntilEnd();
        }

        // AES/CBC/PKCS5Padding
        internal static CryptoStream AESDecryptStream(MemoryStream encryptedDataStream, byte[] key, byte[] iv)
        {
            var rijndaelManaged = new RijndaelManaged()
            {
                Key = key,
                IV = iv
            };

            var decryptor = rijndaelManaged.CreateDecryptor(rijndaelManaged.Key, rijndaelManaged.IV);

            return new CryptoStream(encryptedDataStream, decryptor, CryptoStreamMode.Read);
        }
    }
}
