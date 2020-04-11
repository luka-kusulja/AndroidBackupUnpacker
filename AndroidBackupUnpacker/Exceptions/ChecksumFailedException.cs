using System;

namespace AndroidBackupUnpacker.Exceptions
{
    public class ChecksumFailedException : Exception
    {
        public ChecksumFailedException() : base("Checksum verification failed, wrong password or corrupted file")
        {
        }
    }
}
