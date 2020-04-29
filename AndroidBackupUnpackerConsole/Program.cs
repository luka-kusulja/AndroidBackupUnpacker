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

            var exitCodesCommand = new Command("--exitcodes", "Print exit code list");
            rootCommand.AddCommand(exitCodesCommand);

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

            exitCodesCommand.Handler = CommandHandler.Create(
                                                       () =>
                                                       {
                                                           PrintExitCodes();
                                                       });

            rootCommand.Handler = CommandHandler.Create(
                                                       () =>
                                                       {
                                                           Console.Error.WriteLine("Missing command");
                                                           Environment.Exit((int)ExitCode.MissingCommand);
                                                       });

            rootCommand.Invoke(args);
        }

        static void PrintExitCodes()
        {
            foreach (var currentEnumItem in Enum.GetValues(typeof(ExitCode)))
            {
                Console.WriteLine($"{(int)currentEnumItem} => {currentEnumItem}");
            }
        }

        static void UnpackBackup(string backupFilename, string path, bool extractTar = false, string password = "")
        {
            if (File.Exists(backupFilename) == false)
            {
                Console.Error.WriteLine($"File \"{backupFilename}\" does not exist or you don't have permission to access it.");
                Environment.Exit((int)ExitCode.BackupNotFound);
            }

            if (extractTar == false && File.Exists(path) == true)
            {
                Console.Error.WriteLine($"File \"{path}\" already exists.");
                Environment.Exit((int)ExitCode.FileExists);
            }

            if (extractTar == true && Directory.Exists(path) && (Directory.GetFiles(path).Length + Directory.GetDirectories(path).Length) > 0)
            {
                Console.Error.WriteLine($"Directory \"{path}\" is not empty.");
                Environment.Exit((int)ExitCode.DirectoryNotEmpty);
            }

            try
            {
                var file = File.ReadAllBytes(backupFilename);
                var inputBackupFileStream = new MemoryStream(file);
                var androidBackup = new AndroidBackup(inputBackupFileStream);

                var tarStream = androidBackup.GetTarStream(password);

                if (extractTar == false)
                {
                    var outputFileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
                    tarStream.CopyTo(outputFileStream);

                    tarStream.Close();
                    outputFileStream.Close();
                }
                else
                {
                    ExtractTar.ToFolder(path, tarStream);
                }
            }
            catch (NoPasswordProvidedException ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit((int)ExitCode.NoPasswordProvided);
            }
            catch (WrongPasswordException ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit((int)ExitCode.WrongPassword);
            }
            catch (ChecksumFailedException ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit((int)ExitCode.ChecksumFailed);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Console.WriteLine("Done.");
        }
    }
}
