using System.IO;
using NUnit.Framework;

namespace UnitTests
{
    public class ConsoleTest
    {
        private const string BackupFolder = "Backups";
        private const string FolderPath = "temp";

        [SetUp]
        public void Setup()
        {
            Helpers.DeleteDirectory(FolderPath);

            Directory.CreateDirectory(FolderPath);
        }

        [TearDown]
        public void TearDown()
        {
            Helpers.DeleteDirectory(FolderPath);
        }

        [Test]
        public void Test1()
        {
            var burek = Helpers.RunABU("test --convert dsa.ab");

            Assert.Pass();
        }
    }
}
