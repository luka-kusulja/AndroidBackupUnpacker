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
        public void BackupFileDoesntExist()
        {
            try
            {
                Assert.IsTrue(Helpers.RunAbuAndCheckFile($"{Constants.BackupFolderPath}/doesnt-exist.ab --convert {Constants.TarPath}", TestFileType.Tar));
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("does not exist"));
            }
        }

        [Test]
        public void ConvertFileExists()
        {
            File.WriteAllText(Constants.TarPath, "test");

            try
            {
                NotEncryptedConvert();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("already exists"));
            }
        }

        [Test]
        public void UnpackDirectoryNotEmpty()
        {
            Directory.CreateDirectory(Constants.UnpackPath);
            File.WriteAllText($"{Constants.UnpackPath}/file.txt", "test");

            try
            {
                NotEncryptedUnpack();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("is not empty"));
            }
        }

        [Test]
        public void NoPasswordProvided()
        {
            try
            {
                Assert.IsTrue(Helpers.RunAbuAndCheckFile($"{Constants.BackupFolderPath}/encryption-1234.ab --convert {Constants.TarPath}", TestFileType.Tar));
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("The backup is encrypted but no password was provided"));
            }
        }

        [Test]
        public void WrongPasswordProvided()
        {
            try
            {
                Assert.IsTrue(Helpers.RunAbuAndCheckFile($"{Constants.BackupFolderPath}/encryption-1234.ab --convert {Constants.TarPath} --password wrong-password", TestFileType.Tar));
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Wrong password"));
            }
        }

        [Test]
        public void NotEncryptedConvert()
        {
            Assert.IsTrue(Helpers.RunAbuAndCheckFile($"{Constants.BackupFolderPath}/no-encryption.ab --convert {Constants.TarPath}", TestFileType.Tar));
        }

        [Test]
        public void NotEncryptedUnpack()
        {
            Assert.IsTrue(Helpers.RunAbuAndCheckFile($"{Constants.BackupFolderPath}/no-encryption.ab --unpack {Constants.UnpackPath}", TestFileType.Apk));
        }

        [Test]
        public void EncryptedConvertFirst()
        {
            Assert.IsTrue(Helpers.RunAbuAndCheckFile($"{Constants.BackupFolderPath}/encryption-1234.ab --convert {Constants.TarPath} --password 1234", TestFileType.Tar));
        }

        [Test]
        public void EncryptedConvertSecond()
        {
            Assert.IsTrue(Helpers.RunAbuAndCheckFile($"{Constants.BackupFolderPath}/encryption-q1w2e3r4t5y6u7.ab --convert {Constants.TarPath} --password q1w2e3r4t5y6u7", TestFileType.Tar));
        }

        [Test]
        public void EncryptedUnpackFirst()
        {
            Assert.IsTrue(Helpers.RunAbuAndCheckFile($"{Constants.BackupFolderPath}/encryption-1234.ab --unpack {Constants.UnpackPath} --password 1234", TestFileType.Apk));
        }

        [Test]
        public void EncryptedUnpackSecond()
        {
            Assert.IsTrue(Helpers.RunAbuAndCheckFile($"{Constants.BackupFolderPath}/encryption-q1w2e3r4t5y6u7.ab --unpack {Constants.UnpackPath} --password q1w2e3r4t5y6u7", TestFileType.Apk));
        }
    }
}
