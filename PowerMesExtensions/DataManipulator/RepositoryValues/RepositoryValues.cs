using Atys.PowerMES;
using Atys.PowerMES.Foundation;
using Atys.PowerMES.Services;
using Atys.PowerMES.Support;
using Atys.PowerMES.Support.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamSystem.Customizations.DataManipulator
{
    public class RepositoryValues
    {
        /// <summary>
        /// Carica i dati accessori di elaborazione dalla tabella SH97_RepositoryValues o crea un default
        /// </summary>
        /// <param name="mesManager">Riferimento all'oggetto principale</param>
        /// <param name="resourceName">Nome della risorsa di cui si vogliono i dati</param>
        /// <param name="mesLogger">Riferimento a sistema di log applicazione</param>
        public static MesProdMemento LoadResourceMemento(string resourceName, IMesManager mesManager, IMesAppLogger mesLogger)
        {
            IMesManager _MesManager = mesManager ?? throw new ArgumentNullException(nameof(mesManager));
            IMesAppLogger _MesLogger = mesLogger ?? throw new ArgumentNullException(nameof(mesLogger));

            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException("Value cannot be null or whitespace. ", nameof(resourceName));

            _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                    $"{nameof(LoadResourceMemento)}: Begin." +
                                    $" INFO -> {nameof(resourceName)}: {resourceName}");

            IStorageValuesService storageValueService = _MesManager.ServiceManager.GetService<IStorageValuesService>();

            RepositoryValueContainer processingMementoContainer = storageValueService
                                                                  .GetOrCreateValue(MesGlobalConstants.PowerMesApplicationName,
                                                                                    "EXTENSION",
                                                                                    resourceName);

            MesProdMemento processingMemento = string.IsNullOrWhiteSpace(processingMementoContainer.Value)
                                               ? new MesProdMemento()
                                               : JsonSerializer.DeserializeObject<MesProdMemento>(processingMementoContainer.Value);

            _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                    $"{nameof(LoadResourceMemento)}: End." +
                                    $" INFO -> Loaded: {processingMemento}");

            return processingMemento;
        }

        /// <summary>
        /// Salva i dati accessori di elaborazione nella tabella SH97_RepositoryValues
        /// </summary>
        /// <param name="mesManager">Riferimento all'oggetto principale</param>
        /// <param name="resourceName">Nome della risorsa a cui si riferiscono i dati</param>
        /// <param name="mesLogger">Riferimento a sistema di log applicazione</param>
        public static void SaveResourceMemento(MesProdMemento resourceMemento, string resourceName, IMesManager mesManager, IMesAppLogger mesLogger)
        {
            IMesManager _MesManager = mesManager ?? throw new ArgumentNullException(nameof(mesManager));
            IMesAppLogger _MesLogger = mesLogger ?? throw new ArgumentNullException(nameof(mesLogger));

            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException("Value cannot be null or whitespace .", nameof(resourceName));

            if (resourceMemento == null)
            {
                _MesLogger.WriteMessage(MessageLevel.Warning, false, Constants.LOGGERSOURCE,
                                        $"{nameof(SaveResourceMemento)}: saving data NOT valid.");
                return;
            }

            _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                    $"{nameof(SaveResourceMemento)}: Begin." +
                                    $" INFO -> {nameof(resourceName)}: '{resourceName}'; {nameof(resourceMemento)}: '{resourceMemento}'");

            IStorageValuesService storageValueService = _MesManager.ServiceManager.GetService<IStorageValuesService>();

            //creo o aggiorno l'oggetto (riga della tabella SH97) secondo
            //una chiave di accesso a tre valori di cui la discriminante è il nome risorsa
            RepositoryValueContainer processingMementoContainer = storageValueService
                                                                  .GetOrCreateValue(application: MesGlobalConstants.PowerMesApplicationName,
                                                                                    component: "EXTENSION",
                                                                                    itemName: resourceName);

            //aggiornamento valori memorizzati su db locale PowerMES con parametri vari
            /*
             * NB: l'utilizzo di JsonSerializer implica una referenza a Atys.PowerMES.Support.dll
             */
            string mementoJson = JsonSerializer.SerializeObject(resourceMemento);
            processingMementoContainer.ApplyValue(mementoJson, typeof(MesProdMemento), true);
            bool saveResult = storageValueService.SaveValue(processingMementoContainer);

            _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                    $"{nameof(SaveResourceMemento)}: End. Saving data completed" +
                                    $" INFO -> saveResult: '{saveResult}'");
        }
    }
}
