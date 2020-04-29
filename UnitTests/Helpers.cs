using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace UnitTests
{
    public static class Helpers
    {
        private const int ProcessTimeoutMilliseconds = 5 * 1000;

        internal static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        internal static string RunABU(string arguments)
        {
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = "abu",
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var process = new Process()
            {
                StartInfo = processStartInfo
            };

            process.Start();

            if (!process.WaitForExit(ProcessTimeoutMilliseconds))
            {
                process.Kill();
                throw new Exception("Process timed out");
            }

            var errorOutput = process.StandardError.ReadToEnd();

            if (string.IsNullOrEmpty(errorOutput) == false)
            {
                throw new Exception(errorOutput);
            }

            return process.StandardOutput.ReadToEnd();
        }

        internal static string GetFileCheckSum(string path)
        {
            byte[] checksum;
            using (var stream = File.OpenRead(path))
            {
                var sha = new SHA1Managed();
                checksum = sha.ComputeHash(stream);
            }

            return BitConverter.ToString(checksum).Replace("-", string.Empty);
        }

        private static bool CheckFileChecksum(string filePath, string checksum)
        {
            var outputFileChecksum = Helpers.GetFileCheckSum(filePath);
            return outputFileChecksum == checksum;
        }

        internal static bool RunAbuAndCheckFile(string abuCommand, TestFileType type)
        {
            var abuOutput = Helpers.RunABU(abuCommand);

            if (abuOutput.Contains("Done.") == false)
            {
                return false;
            }

            if (type == TestFileType.Tar)
            {
                return CheckFileChecksum(Constants.TarPath, Constants.TarChecksum);
            }
            else
            {
                return CheckFileChecksum(Constants.ApkPath, Constants.ApkChecksum);
            }
        }
    }
}
