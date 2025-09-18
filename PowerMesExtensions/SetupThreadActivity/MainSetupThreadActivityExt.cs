using Atys.PowerMES.Extensibility;
using Atys.PowerMES.Foundation;
using Atys.PowerMES.Repeaters;
using Atys.PowerMES.Services;
using Atys.PowerMES.Settings.GammaMes;
using Atys.PowerMES.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
//using TeamSystem.MesExtension;

namespace TeamSystem.Customizations.SetupThreadActivity
{
    [ExtensionData(name: "SetupThreadActivity",
                   description: "Personalizzazione PowerMES per il cliente " + Constants.CUSTOMER,
                   version: "1.0",
                   editorCompany: "TeamSystem",
                   author: "TeamSystem")]

    public class MainSetupThreadActivityExt : IMesExtension
    {
        const string extensionLog = @"C:\ProgramData\Atys\PowerMES\v1\Logs\ExtensionLog.txt";

        #region fields

        private IMesManager _MesManager = null; //riferimento all'oggetto principale PowerMES
        private IMesAppLogger _MesLogger = null; //riferimento al sistema di log principale dell'applicazione (MainLog)
                                                 
        private CredentialsVault _CredentialsVault = null; //oggetto contenente le credenziali di connessione

        /*
         * ManualResetEventSlim è una tipologia di lock qui usato per evitare che due
         * esecuzioni del task ricorrente vadano in sovrapposizione
         */
        private readonly ManualResetEventSlim _Thread = new System.Threading.ManualResetEventSlim(false); //per sincronizzazione attività ricorrente
        private readonly Guid _ComponentId = Guid.NewGuid(); //id specifico del componente (per scheduler)
        private Guid _ThreadActivityId = Guid.Empty; //id task per job schedulatore costruzione piani di lavoro
        AttributeCheck _Attribute = null;

        private string _pmesSqlConnStr = null;
        private string _overOneSqlConnStr = null;

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

            _Attribute = new AttributeCheck(_MesManager);

            #region service attributes

            //ConnectionString al DB AtysPowerMes
            _pmesSqlConnStr = _Attribute.GetServiceStringAttibute("POWERMES_SQLCONN_STRING");

            if (string.IsNullOrWhiteSpace(_pmesSqlConnStr))
            {
                this._MesLogger.WriteMessage(MessageLevel.Error, true, nameof(Initialize),
                                             $"Errore attributo di servizio POWERMES_SQLCONN_STRING");
            }

            _overOneSqlConnStr = _Attribute.GetServiceStringAttibute("OVERONE_SQLCONN_STRING");

            if (string.IsNullOrWhiteSpace(_overOneSqlConnStr))
            {
                this._MesLogger.WriteMessage(MessageLevel.Error, true, nameof(Initialize),
                                              $"Errore attributo di servizio OVERONE_SQLCONN_STRING");
            }

            #endregion

        }

        /// <summary>
        /// Esegue/avvia la personalizzazione
        /// </summary>
        public void Run()
        {
            this._CredentialsVault = new CredentialsVault(_MesManager, _MesLogger);

            this.SetupThreadActivity();

            //da qui mi aggancio all'evento d'interesse
            //dopo "+=" è bene creare un metodo con nome affine all'evento richiamato
            //prende dell'intero ambiente PowerMES tutti i comandi esterni.
            //Presenti in: PowerMES Clinet --> Home --> Server --> Send Command
            this._MesManager.ProcessExternalCommand += ProcessExternalCommand;

            this.PublishExternalCommands();
        }

        /// <summary>
        /// Deve contenere il codice di cleanup da eseguire prima della disattivazione
        /// della personalizzazione o comunque alla chiusura di PowerMES
        /// </summary>
        public void Shutdown()
        {
            this._MesManager.ProcessExternalCommand -= ProcessExternalCommand;

            //revoca un comando esterno su "Send Command" di PowerMES
            this.RevokeExternalCommands();

            //pulisco/distruggo l'attività ricorrente
            this.ClearThreadActivity();
        }

        #endregion

        #region thread activity

        private void SetupThreadActivity()
        {
            this._MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                         $@"{nameof(SetupThreadActivity)}: Begin.");

            //scarta un'eventuale evento successivo, se il thread precedente non ha ancora terminato
            this._Thread.Reset();

            //prendo un riferimento al servizio di schedulazione
            IJobSchedulerService scheduler = this._MesManager.ServiceManager.GetService<IJobSchedulerService>();

#if DEBUG
            DateTimeOffset firstStart = DateTimeOffset.Now.AddSeconds(10);
            TimeSpan interval = new TimeSpan(0, 0, 10);
#else
            DateTimeOffset firstStart = DateTimeOffset.Now.AddSeconds(60);
            TimeSpan interval = new TimeSpan(0, 0, 60);
#endif

            //creo l'oggetto per lo schedulatore (da qui ho l'azione dello scheduler)
            RecurringActivity recurringActivity = new RecurringActivity(Guid.NewGuid(),
                                                                        this._ComponentId,
                                                                        firstStart,
                                                                        this.ThreadTaskTriggerAction,
                                                                        this.ThreadTaskErrorAction,
                                                                        interval);

            //creo il job nello schedulatore
            if (scheduler.SubmitRecurringActivity(recurringActivity))
            {
                this._ThreadActivityId = recurringActivity.ActivityId;

                this._MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                             $"{nameof(SetupThreadActivity)}: activity submitted -> "
                                             + "'{0}' starting from: '{1}'; interval: '{2}'",
                                             recurringActivity.ActivityId.ToString(),
                                             firstStart.ToString(), interval.ToString());
            }
            else
            {
                this._MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                             $"{nameof(SetupThreadActivity)}: failed to submit activity.");
            }

            this._MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                         $@"{nameof(SetupThreadActivity)}: End.");
        }

        /// <summary>
        /// Trigger che avvia il thread della task richiesta
        /// </summary>
        /// <param name="activityId"></param>
        private void ThreadTaskTriggerAction(Guid activityId)
        {
            this._MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                         $@"{nameof(ThreadTaskTriggerAction)}: Begin.");

            Debug.Assert(activityId != Guid.Empty);

            if (this._Thread.IsSet)
            {
                this._MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                             $@"{nameof(ThreadTaskTriggerAction)}: overlapping operation!");

                return;
            }

            this._Thread.Set();

            List<IMesResource> resources = this._MesManager.ResourcesHandler.GetResources().ToList();

            try
            {
                foreach (IMesResource resource in resources)
                {
                    //controllo ed estrapolo i valori degli attributi impostati a livello di risorsa
                    ResourceAttribute resourceAttribute = new ResourceAttribute(resource, _MesManager, _MesLogger);

                    //File.AppendAllText(@"C:\ProgramData\Atys\PowerMES\v1\Logs\extensionLog.txt", $"{resource.Name}: resourceAttribute.CanProceed = {resourceAttribute.CanProceed}" + Environment.NewLine);

                    if (!resourceAttribute.CanProceed)
                    {
                        _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                                $@"{resource.Name}: i valori degli attributi impostati a livello di risorsa NON sono validi => il valore di resourceAttribute.CanProceed è '{resourceAttribute.CanProceed}'");
                        continue;
                    }

                    //recupero i valori memorizzati nella tabella [AtysPowerMes].[dbo].[SH97_RepositoryValues]
                    var resourceMemento = RepositoryValues.LoadResourceMemento(resource.Name, _MesManager, _MesLogger);

                    //controllo ed estrapolo i valori della lavorazione presente nella vista MES
                    MesValue mesValue = new MesValue(_CredentialsVault, resource, _MesManager, _MesLogger);
                    if (!mesValue.CanProceed)
                    {
                        _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                                $@"{resource.Name}: NON è possibile procedere => il valore di mesValue.CanProceed è '{mesValue.CanProceed}'");
                        continue;
                    }

                    //File.AppendAllText(@"C:\ProgramData\Atys\PowerMES\v1\Logs\extensionLog.txt", $"{resource.Name}: mesValue.CanProceed = {mesValue.CanProceed}" + Environment.NewLine);

                    //eseguo istruzioni differenti in base al nome risorsa
                    switch (resource.Name)
                    {
                        case "SKFNG060":
                            //scrivo sul PLC i valori estrapolati dal MES e faccio un controllo per assicurarmi di averli scritti correttamente
                            SetAddressValue setAddressValue = new SetAddressValue(resource, resourceAttribute, mesValue, _MesManager, _MesLogger);
                            if (!setAddressValue.CanProceed)
                            {
                                _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                                        $@"{resource.Name}: i valori NON sono stati scritti correttamente sugli address del PLC => il valore di setAddressValue.CanProceed è '{setAddressValue.CanProceed}'");
                                continue;
                            }
                            break;
                    }

                    resourceMemento.LastReadWorkOrder = mesValue.WorkOrder;
                    resourceMemento.LastReadArticleName = mesValue.ArticleName;
                    resourceMemento.LastReadArticlePhase = mesValue.ArticlePhase;
                    resourceMemento.LastReadTotalQty = mesValue.TotalQty;

                    RepositoryValues.SaveResourceMemento(resourceMemento, resource.Name, _MesManager, _MesLogger);
                }

                #region Overmach

                //File.AppendAllText(ExtensionLog, $"[{DateTime.Now}]: Overmach Begin" + Environment.NewLine);

                var overmachResource = this._MesManager.ResourcesHandler.GetResource("CORREA");
                //File.AppendAllText(extensionLog, $"[{DateTime.Now}]: resource = {overmachResource.Name}" + Environment.NewLine);

                var machineId = _Attribute.GetStringValueAttribute(overmachResource, "MACHINE_ID");
                //File.AppendAllText(extensionLog, $"[{DateTime.Now}]: MACHINE_ID = {machineId}" + Environment.NewLine);

                if (!string.IsNullOrEmpty(machineId) && machineId != "NOT VALID")
                {
                    bool isNumber = int.TryParse(machineId, out _);

                    if (isNumber)
                    {
                        TransazioniReader transazioniReader = new TransazioniReader(_overOneSqlConnStr, _pmesSqlConnStr, _MesManager);
                        List<TransazioniData> newTransactions = transazioniReader.ReadNewTransazioni(Convert.ToInt32(machineId));

                        //File.AppendAllText(extensionLog, $"[{DateTime.Now}]: newTransactions = {newTransactions.Count}" + Environment.NewLine);

                        if (newTransactions != null && newTransactions.Count > 0)
                        {
                            foreach (var transaction in newTransactions)
                            {
                                EventsTools powerMesEvents = new EventsTools(overmachResource.Name,
                                                                        "OVERMACH",
                                                                        "10",
                                                                        "WO",
                                                                        transaction.Inizio.ToString("MM/dd/yyyy HH:mm:ss"),
                                                                        transaction.Inizio.ToString("MM/dd/yyyy HH:mm:ss"),
                                                                        transaction.PezziBuoni,
                                                                        _MesManager,
                                                                        _MesLogger
                                                                        );

                                switch (transaction.ActivityId)
                                {
                                    case 9:
                                        powerMesEvents.SendDone();
                                        break;
                                    case 10:
                                        powerMesEvents.SendGenericSuspension();
                                        break;
                                    case 11:
                                        powerMesEvents.SendStartWithWo();
                                        break;
                                }
                            }
                        }

                        //File.AppendAllText(ExtensionLog, $"[{DateTime.Now}]: Overmach End" + Environment.NewLine);
                    }
                }

                #endregion


            }
            catch (Exception ex)
            {
                this._MesLogger.WriteException(ex, Constants.LOGGERSOURCE,
                                               $@"{nameof(ThreadTaskTriggerAction)}: Exception during processing." +
                                               $" INFO -> Source: '{ex.Source}'; Message: '{ex.Message}'");
            }
            finally
            {
                this._Thread.Reset();
            }

            this._MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                         $@"{nameof(ThreadTaskTriggerAction)}: End.");
        }

        /// <summary>
        /// Errore in caso di eccezione sul thread della task richiesta
        /// </summary>
        /// <param name="activityId"></param>
        /// <param name="ex"></param>
        private void ThreadTaskErrorAction(Guid activityId, Exception ex)
        {
            this._MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                         $@"{nameof(ThreadTaskErrorAction)}: Begin.");

            Debug.Assert(activityId != Guid.Empty);
            Debug.Assert(ex != null);

            this._MesLogger.WriteException(ex, Constants.LOGGERSOURCE, "Error building resource work plan.");
        }

        /// <summary>
        /// Reset e pulizia del thread
        /// </summary>
        private void ClearThreadActivity()
        {
            this._MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                         $@"{nameof(ClearThreadActivity)}: Begin.");

            this._Thread.Reset();

            if (this._ThreadActivityId == Guid.Empty)
                return;

            //prendo un riferimento al servizio di schedulazione
            IJobSchedulerService scheduler = this._MesManager.ServiceManager.GetService<IJobSchedulerService>();
            Debug.Assert(scheduler != null);

            if (scheduler.HasActivityById(this._ThreadActivityId))
                scheduler.CancelActivity(this._ThreadActivityId);

            //tolgo il riferimento a task
            this._ThreadActivityId = Guid.Empty;

            this._MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                         $@"{nameof(ClearThreadActivity)}: End.");
        }

        #endregion

        #region external command

        /// <summary>
        /// Evento che processa il comando esterno specificato
        /// in: PowerMES, Principale, Invia Comando
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProcessExternalCommand(object sender, ExternalCommandsExecutionEventArgs e)
        {
            this._MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                         $@"{nameof(ProcessExternalCommand)}: Begin.");

            List<ExternalCommandValue> paramList = e.Parameters != null
                                                 ? e.Parameters.ToList()
                                                 : new List<ExternalCommandValue>();

            //controllo del comando da processare: da modificare in base a quello che si vuole creare
            if (e.CommandCode.Trim().ToUpper() == Constants.EXTCMD_1 && paramList.Count == 1 && paramList[0].Type == ExternalCommandValueType.String)
            {
                string resourceName = paramList[0].GetConvertedValueToType<string>().Trim();

                IMesResource resource = this._MesManager.ResourcesHandler.GetResource(resourceName);
                if (resource == null)
                {
                    this._MesLogger.WriteMessage(MessageLevel.Diagnostics, true, Constants.LOGGERSOURCE,
                                                 $@"{nameof(ProcessExternalCommand)}: No resource found for external command. " +
                                                 $@"INFO -> command: '{Constants.EXTCMD_1}'; resourceName: '{resourceName}'");
                    return;
                }

                this._MesManager.AppendMessageToLog(MessageLevel.Diagnostics, Constants.LOGGERSOURCE,
                                                    "EXTERNAL COMMAND (" + Constants.EXTCMD_1 + ")");

                //TODO
            }

            //controllo del comando da processare: da modificare in base a quello che si vuole creare
            if (e.CommandCode.Trim().ToUpper() == Constants.EXTCMD_2 && paramList.Count == 2 && paramList[0].Type == ExternalCommandValueType.String && paramList[1].Type == ExternalCommandValueType.Boolean)
            {
                string resourceName = paramList[0].GetConvertedValueToType<string>().Trim();
                bool proceed = paramList[1].GetConvertedValueToType<bool>();

                IMesResource resource = this._MesManager.ResourcesHandler.GetResource(resourceName);
                if (resource == null)
                {
                    this._MesLogger.WriteMessage(MessageLevel.Diagnostics, true, Constants.LOGGERSOURCE,
                                                 $@"{nameof(ProcessExternalCommand)}: No resource found for external command. " +
                                                 $@"INFO -> command: '{Constants.EXTCMD_2}'; resourceName: '{resourceName}'");
                    return;
                }

                if (proceed == false)
                {
                    this._MesLogger.WriteMessage(MessageLevel.Diagnostics, true, Constants.LOGGERSOURCE,
                                                 $@"{nameof(ProcessExternalCommand)}: It is not possible to proceed for external command. " +
                                                 $@"INFO -> command: '{Constants.EXTCMD_2}'; proceed: '{proceed}'");
                    return;
                }

                this._MesManager.AppendMessageToLog(MessageLevel.Diagnostics, Constants.LOGGERSOURCE,
                                                    "EXTERNAL COMMAND (" + Constants.EXTCMD_2 + ")");

                //TODO
            }

            this._MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                         $"{nameof(ProcessExternalCommand)}: End.");
        }

        /// <summary>
        /// Pubblica i template dei comandi esterni creati
        /// in: PowerMES, Principale, Invia Comando
        /// </summary>
        private void PublishExternalCommands()
        {
            //prelevo il nome dell'assembly
            string publisher = this.GetType().Name.ToUpper();

            List<ExternalCommandDescriptor> templates = new List<ExternalCommandDescriptor>();

            //creazione del template del comando: da modificare in base a quello che si vuole creare
            templates.Add(new ExternalCommandDescriptor(publisher,
                                                        Constants.EXTCMD_1,
                                                        new List<ExternalCommandValue>()
                                                        {
                                                            new ExternalCommandValue("RESOURCE", string.Empty, ExternalCommandValueType.String)
                                                        }));

            //creazione del template del comando: da modificare in base a quello che si vuole creare
            templates.Add(new ExternalCommandDescriptor(publisher,
                                                        Constants.EXTCMD_2,
                                                        new List<ExternalCommandValue>()
                                                        {
                                                            new ExternalCommandValue("RESOURCE", string.Empty, ExternalCommandValueType.String),
                                                            new ExternalCommandValue("PROCEED", "1", ExternalCommandValueType.Boolean),
                                                        }));

            this._MesManager.PublishExternalCommandsTemplates(templates);
        }

        /// <summary>
        /// Annulla la pubblicazione dei template dei comandi esterni creati
        /// in: PowerMES, Principale, Invia Comando
        /// </summary>
        private void RevokeExternalCommands()
        {
            string publisher = this.GetType().Name.ToUpper();

            this._MesManager.RevokeExternalCommandsTemplates(publisher);
        }

        #endregion
    }
}
