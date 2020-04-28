using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Tar;

namespace AndroidBackupUnpackerConsole
{
    internal static class ExtractTar
    {
        internal static void ToFolder(string targetDir, MemoryStream tarStream)
        {
            var inputTarStream = new TarInputStream(tarStream);
            TarEntry tarEntry;
            while ((tarEntry = inputTarStream.GetNextEntry()) != null)
            {
                if (tarEntry.IsDirectory)
                {
                    continue;
                }

                var name = SanitizeName(tarEntry.Name);

                if (Path.IsPathRooted(name))
                {
                    name = name.Substring(Path.GetPathRoot(name).Length);
                }

                var outName = Path.Combine(targetDir, name);

                var directoryName = Path.GetDirectoryName(outName);

                Directory.CreateDirectory(directoryName);

                var outStr = new FileStream(outName, FileMode.Create);

                inputTarStream.CopyEntryContents(outStr);

                outStr.Close();
            }

            inputTarStream.Close();
        }

        private static string SanitizeName(string name)
        {
            var stringBuilder = new StringBuilder();

            var illegalChars = Path.GetInvalidFileNameChars();

            foreach (var currentChar in name)
            {
                if (currentChar != '/' && illegalChars.Contains(currentChar))
                {
                    stringBuilder.Append('_');
                }
                else
                {
                    stringBuilder.Append(currentChar);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
