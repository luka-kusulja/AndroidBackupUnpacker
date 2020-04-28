namespace AndroidBackupUnpacker
{
    public class BackupAttributes
    {
        public string Header { get; internal set; }

        public string BackupVersion { get; internal set; }

        public bool IsCompressed { get; internal set; }

        public EncryptionType EncryptionType { get; internal set; }

        public string UserPasswordSalt { get; set; }

        public string MasterKeySalt { get; set; }

        public int Iterations { get; set; }

        public string UserIVKey { get; set; }

        public string MasterKeyEncrypted { get; set; }
    }

    public enum EncryptionType
    {
        Unknown = 0,
        None = 1,
        AES256 = 2
    }
}
