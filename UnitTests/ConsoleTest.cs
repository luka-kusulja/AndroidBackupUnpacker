using System;
using System.IO;
using NUnit.Framework;

namespace UnitTests
{
    public class ConsoleTest
    {
        [SetUp]
        public void Setup()
        {
            Helpers.DeleteDirectory(Constants.TempFolderPath);

            Directory.CreateDirectory(Constants.TempFolderPath);
        }

        [TearDown]
        public void TearDown()
        {
            Helpers.DeleteDirectory(Constants.TempFolderPath);
        }

        [Test]
        public void PrintHelp()
        {
            var abuOutput = Helpers.RunABU("--help");

            Assert.IsTrue(abuOutput.Contains("Tool for converting and unpacking android backups - Luka Kusulja"));
        }

        [Test]
        public void PrintExitCodes()
        {
            var abuOutput = Helpers.RunABU("--exitcodes");

            Assert.IsTrue(abuOutput.Contains("Success"));
        }

        [Test]
        public void MissingCommand()
        {
            try
            {
                var abuOutput = Helpers.RunABU("nobackup.ab");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Missing command"));
                return;
            }

            Assert.Fail();
        }

        [Test]
        public void UnrecognizedCommand()
        {
            try
            {
                var abuOutput = Helpers.RunABU("nobackup.ab --doesntexist");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Unrecognized command or argument"));
                return;
            }

            Assert.Fail();
        }

        [Test]
        public void NotEncryptedConvert()
        {
            Assert.IsTrue(Helpers.RunAbuAndCheckFile($"{Constants.BackupFolderPath}/no-encryption.ab --convert {Constants.TarPath}", TestFileType.Tar));
        }

        [Test]
        public void NotEncryptedConvertFileExists()
        {
            File.WriteAllText(Constants.TarPath, "test");

            try
            {
                Assert.IsTrue(Helpers.RunAbuAndCheckFile($"{Constants.BackupFolderPath}/no-encryption.ab --convert {Constants.TarPath}", TestFileType.Tar));
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("already exists"));
            }
        }

        [Test]
        public void NotEncryptedUnpack()
        {
            Assert.IsTrue(Helpers.RunAbuAndCheckFile($"{Constants.BackupFolderPath}/no-encryption.ab --unpack {Constants.UnpackPath}", TestFileType.Apk));
        }

        [Test]
        public void NotEncryptedUnpackDirectoryNotEmpty()
        {
            Directory.CreateDirectory(Constants.UnpackPath);
            File.WriteAllText($"{Constants.UnpackPath}/file.txt", "test");

            try
            {
                Assert.IsTrue(Helpers.RunAbuAndCheckFile($"{Constants.BackupFolderPath}/no-encryption.ab --unpack {Constants.UnpackPath}", TestFileType.Apk));
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("is not empty"));
            }
        }
    }
}
