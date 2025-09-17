using Atys.PowerMES.Device;
using Atys.PowerMES.Events;
using Atys.PowerMES.Extensibility;
using Atys.PowerMES.Foundation;
using Atys.PowerMES.Services;
using Atys.PowerMES.Support;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TeamSystem.Customizations.DataManipulator
{
    [ExtensionData(name: "DataManipulator",
                   description: "Personalizzazione PowerMES per il cliente " + Constants.CUSTOMER,
                   version: "1.0",
                   editorCompany: "TeamSystem",
                   author: "TeamSystem")]

    public sealed class MainDataManipulatorExt : IMesExtension
    {
        #region fields

        private IMesManager _MesManager = null; //riferimento all'oggetto principale PowerMES
        private IMesAppLogger _MesLogger = null; //riferimento al sistema di log principale dell'applicazione (MainLog)

        private CredentialsVault _CredentialsVault = null; //oggetto contenente le credenziali di connessione

        #endregion

        #region IMesExtension members

        /// <summary>
        /// Inizializzazione della personalizzazione e collegamento all'oggetto principale PowerMES
        /// (eseguito al caricamento in memoria della personalizzazione)
        /// </summary>
        /// <param name="mesManager">Riferimento all'oggetto principale PowerMES</param>
        public void Initialize(IMesManager mesManager)
        {
#if DEBUG
            Debugger.Launch();
#endif
            this._MesManager = mesManager;
            this._MesLogger = this._MesManager.ApplicationMainLogger;

            this._MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                         $@"{nameof(Initialize)}: Customization created!");
        }

        /// <summary>
        /// Esegue/avvia la personalizzazione
        /// </summary>
        public void Run()
        {
            this._CredentialsVault = new CredentialsVault(_MesManager, _MesLogger);

            this._MesManager.Controller.ManipulatingDataUnitOnProcessorQueueUp += Controller_ManipulatingDataUnitOnProcessorQueueUp;
        }

        /// <summary>
        /// Deve contenere il codice di cleanup da eseguire prima della disattivazione
        /// della personalizzazione o comunque alla chiusura di PowerMES
        /// </summary>
        public void Shutdown()
        {
            this._MesManager.Controller.ManipulatingDataUnitOnProcessorQueueUp -= Controller_ManipulatingDataUnitOnProcessorQueueUp;
        }

        #endregion

        #region Controller_ManipulatingDataUnitOnProcessorQueueUp

        /// <summary>
        /// Evento per la manipolazione dei dati di produzione (articolo-fase-workorder)
        /// prima dell'accodamento in processor risorsa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Controller_ManipulatingDataUnitOnProcessorQueueUp(object sender, ResourceProcessingDataManipulationEventArgs e)
        {
            /*
             * In "PowerMES Client" devi andare in "Visualizza", "Impostazioni Server", "Generale.
             * Poi, devi abilitare "Abilita manipolazione avanzata eventi via API" attraverso l'apposito checkbox
             */

            var resource = e.Resource;

            //controllo ed estrapolo i valori degli attributi impostati a livello di risorsa
            ResourceAttribute resourceAttribute = new ResourceAttribute(resource, _MesManager, _MesLogger);
            if (!resourceAttribute.CanProceed)
            {
                _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                        $@"{resource.Name}: i valori degli attributi impostati a livello di risorsa NON sono validi => il valore di resourceAttribute.CanProceed è '{resourceAttribute.CanProceed}'");
                return;
            }

            //recupero i valori memorizzati nella tabella [AtysPowerMes].[dbo].[SH97_RepositoryValues]
            var resourceMemento = RepositoryValues.LoadResourceMemento(resource.Name, _MesManager, _MesLogger);

            e.HideSource = true;
            e.ManipulationMode = ProcessingDataManipulationMode.SwapArticle;
            e.ManipulationData = new List<ProcessingData>()
            {
                new ProcessingData(new ArticleItem(resourceMemento.LastReadArticleName ?? "ATYS", resourceMemento.LastReadArticlePhase ?? "10"), resourceMemento.LastReadWorkOrder ?? string.Empty),
            };

            #endregion

        }
    }
}
