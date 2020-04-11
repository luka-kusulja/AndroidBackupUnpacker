using System;
using System.IO;
using System.Linq;
using System.Text;

namespace AndroidBackupUnpacker
{
    internal static class Helpers
    {
        internal static byte[] HexToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        // C# and Java return UTF-8 byte values off by 1, its a mess google it
        internal static byte[] ToJavaUTF8ByteArray(byte[] data)
        {
            var input = new MemoryStream(data);
            
            var output = new StringBuilder();
            while(true)
            {
                int read = input.ReadByte();

                if(read == -1)
                {
                    break;
                }

                if (read >= 128)
                {
                    output.Append(char.ConvertFromUtf32(65536 + (read - 256)));
                }
                else
                {
                    output.Append(char.ConvertFromUtf32(read));
                }
            }

            return Encoding.UTF8.GetBytes(output.ToString());
        }
    }
}
