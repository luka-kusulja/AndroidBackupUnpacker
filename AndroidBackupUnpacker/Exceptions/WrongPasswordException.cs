using System;

namespace AndroidBackupUnpacker.Exceptions
{
    public class WrongPasswordException : Exception
    {
        public WrongPasswordException() : base("Wrong password")
        {
        }

        public WrongPasswordException(Exception ex) : base("Wrong password", ex)
        {
        }
    }
}
