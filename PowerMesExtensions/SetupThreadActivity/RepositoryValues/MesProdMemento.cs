using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamSystem.Customizations.SetupThreadActivity
{
    //dati memorizzati nella tabella [AtysPowerMes].[dbo].[SH97_RepositoryValues]
    public class MesProdMemento
    {
        /// <summary>
        /// Imposta e restituisce l'ultimo "LastReadWorkOrder" elaborato
        /// </summary>
        public string LastReadWorkOrder { get; set; }

        /// <summary>
        /// Imposta e restituisce l'ultimo "LastReadArticleName" elaborato
        /// </summary>
        public string LastReadArticleName { get; set; }

        /// <summary>
        /// Imposta e restituisce l'ultimo "LastReadArticlePhase" elaborato
        /// </summary>
        public string LastReadArticlePhase { get; set; }

        /// <summary>
        /// Imposta e restituisce l'ultimo "LastReadTotalQty" elaborato
        /// </summary>
        public string LastReadTotalQty { get; set; }
    }
}
