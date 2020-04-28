namespace AndroidBackupUnpackerConsole
{
    internal enum ExitCode
    {
        Success = 0,
        UnknownError = 1,
        BackupNotFound = 2,
        FileExists = 3,
        DirectoryNotEmpty = 4,
        NoPasswordProvided = 5,
        WrongPassword = 6,
        ChecksumFailed = 7,
        MissingCommand = 8
    }
}
