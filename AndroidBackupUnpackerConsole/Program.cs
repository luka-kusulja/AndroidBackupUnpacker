using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using AndroidBackupUnpacker;
using AndroidBackupUnpacker.Exceptions;

namespace AndroidBackupUnpackerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootCommand = new RootCommand()
            {
                Name = "AndroidBackupUnpacker",
                Description = "Tool for converting and unpacking android backups - Luka Kusulja",
            };

            var backupFileNameArgument = new Argument<string>("backup");
            rootCommand.AddArgument(backupFileNameArgument);

            var convertCommand = new Command("--convert", "Convert backtup to TAR archive")
            {
                new Argument<string>("tar")
            };
            rootCommand.AddCommand(convertCommand);

            var unpackCommand = new Command("--unpack", "Extract content to folder")
            {
                new Argument<string>("folder")
            };
            rootCommand.AddCommand(unpackCommand);

            rootCommand.AddGlobalOption(new Option("--password", "Password if the backup is encrypted")
            {
                Argument = new Argument<string>("password")
            });

            convertCommand.Handler = CommandHandler.Create<string, string, string>(
                                                       (backup, tar, password) =>
                                                       {
                                                           UnpackBackup(backup, tar, false, password);
                                                       });

            unpackCommand.Handler = CommandHandler.Create<string, string, string>(
                                                       (backup, folder, password) =>
                                                       {
                                                           UnpackBackup(backup, folder, true, password);
                                                       });

            rootCommand.Invoke(args);
        }

        static void UnpackBackup(string backupFilename, string path, bool extractTar = false, string password = "")
        {
            if (File.Exists(backupFilename) == false)
            {
                Console.WriteLine($"File \"{backupFilename}\" does not exits or you don't have permission to read it.");
                return;
            }

            if (extractTar == false && File.Exists(path) == true)
            {
                Console.WriteLine($"File \"{path}\" already exists.");
                return;
            }

            if (extractTar == true && Directory.Exists(path) && (Directory.GetFiles(path).Length + Directory.GetDirectories(path).Length) > 0)
            {
                Console.WriteLine($"Directory \"{path}\" is not empty.");
                return;
            }

            try
            {
                var file = File.ReadAllBytes(backupFilename);
                var inputBackupFileStream = new MemoryStream(file);
                var androidBackup = new AndroidBackup(inputBackupFileStream);

                var tarStream = androidBackup.GetTarStream(password);

                if (extractTar == false)
                {
                    using (var outputFileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                    {
                        tarStream.CopyTo(outputFileStream);
                    }
                }
                else
                {
                    ExtractTar.ToFolder(path, tarStream);
                }
            }
            catch (Exception ex)
            {
                if (ex is NoPasswordProvidedException || ex is WrongPasswordException || ex is ChecksumFailedException)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }

                throw ex;
            }
        }
    }
}
