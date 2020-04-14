using System.IO;
using System.Text;

namespace AndroidBackupUnpacker
{
    internal static class Extensions
    {
        internal static string ReadOneLine(this Stream stream)
        {
            var stringBuilder = new StringBuilder();

            var currentChar = -1;
            while (true)
            {
                currentChar = stream.ReadByte();
                if (currentChar == '\n' || currentChar == -1)
                {
                    break;
                }
                else
                {
                    stringBuilder.Append((char)currentChar);
                }
            }

            return stringBuilder.ToString();
        }

        internal static byte[] ReadUntilEnd(this Stream inputStream)
        {
            var outputStream = new MemoryStream();

            var buffer = new byte[1024];
            while(true)
            {
                int readLength = inputStream.Read(buffer, 0, buffer.Length);

                outputStream.Write(buffer, 0, readLength);

                if (readLength != buffer.Length)
                {
                    break;
                }
            };

            return outputStream.ToArray();
        }

        internal static byte[] ReadBlobPart(this Stream inputStream)
        {
            int blobPartLength = inputStream.ReadByte();
            byte[] blobPartData = new byte[blobPartLength];
            inputStream.Read(blobPartData, 0, blobPartLength);

            return blobPartData;
        }
    }
}
