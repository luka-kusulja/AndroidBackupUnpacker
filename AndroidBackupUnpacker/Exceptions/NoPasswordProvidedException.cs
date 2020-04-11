using System;

namespace AndroidBackupUnpacker.Exceptions
{
    public class NoPasswordProvidedException : Exception
    {
        public NoPasswordProvidedException() : base("The backup is encrypted but no password was provided")
        {
        }
    }
}
