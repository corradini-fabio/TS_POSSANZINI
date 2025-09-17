using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamSystem.Customizations.SetupThreadActivity
{
    public static class Constants
    {
        //indicare il cliente per la quale è stata creata la personalizzazione
        public const string CUSTOMER = "POSSANZINI SRL";

        //indicare la personalizzazione da cui proviene il log
        public const string LOGGERSOURCE = "SetupThreadActivity";

        //indicare il nome dei template dei comandi esterni creati
        public const string EXTCMD_1 = "#";
        public const string EXTCMD_2 = "##";

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
            public const string ENABLE_SETUP_THREAD_ACTIVITY = "ENABLE_SETUP_THREAD_ACTIVITY";
            public const string WORK_ORDER = "WORK_ORDER";
            public const string ARTICLE_NAME = "ARTICLE_NAME";
            public const string ARTICLE_PHASE = "ARTICLE_PHASE";
            public const string TOTAL_QTY = "TOTAL_QTY";
        }

        //stringa di connessione
        public const string ConnectionString = @"Data Source={0};" + //InstanceName
                                                "Initial Catalog={1};" + //DatabaseName
                                                "Persist Security Info=True;" + //
                                                "User ID={2};" + //UserName
                                                "Password={3};"; //Password

        //stringa di comando
#if DEBUG
        public const string CommandString = @"SELECT TOP 1 * FROM [POSSANZINI_MES].[dbo].[_DATI_POWERMES] WHERE [MACCHINA] = '{0}';";
#else
        public const string CommandString = @"SELECT TOP 1 * FROM [POSSANZINI_MES].[dbo].[_DATI_POWERMES] WHERE [MACCHINA] = '{0}';";
#endif
    }
}
