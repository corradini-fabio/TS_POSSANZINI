namespace TeamSystem.Customizations.DataManipulator
{
    public static class Constants
    {
        //indicare il cliente per la quale è stata creata la personalizzazione
        public const string CUSTOMER = "POSSANZINI SRL";

        //indicare la personalizzazione da cui proviene il log
        public const string LOGGERSOURCE = "DataManipulator";

#if DEBUG
        //indicare il nome dell'istanza
        public const string InstanceName = @"NB001354\DEV17";

        //indicare il nome del database
        public const string DatabaseName = @"POSSANZINI_MES";

        //indicare il codice univoco generato nel vault dell'authentication manager
        public const string VaultUniqueCode = @"1bd53549-1705-4ff3-93a0-e6feaccffa22";
#else
        //indicare il nome dell'istanza 
        public const string InstanceName = @"SRV02";

        //indicare il nome del database
        public const string DatabaseName = @"POSSANZINI_MES";

        //indicare il codice univoco generato nel vault dell'authentication manager
        //public const string VaultUniqueCode = @"842b6495-4b2f-42be-9c16-8a1d3a92e08a";
        public const string VaultUniqueCode = @"da1beeab-03b3-42ba-a378-69dbfd4dc72e";
#endif

        public struct ResourceAttributeName
        {
            public const string ENABLE_DATA_MANIPULATOR = "ENABLE_DATA_MANIPULATOR";
        }
    }
}
