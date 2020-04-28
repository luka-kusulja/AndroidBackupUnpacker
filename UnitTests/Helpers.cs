using System;
using System.Diagnostics;
using System.IO;

namespace UnitTests
{
    public static class Helpers
    {
        private const int ProcessTimeoutMilliseconds = 10 * 1000;

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
    }
}
