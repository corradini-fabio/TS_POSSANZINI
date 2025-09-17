using Atys.PowerMES.Foundation;
using Atys.PowerMES.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamSystem.Customizations.DataManipulator
{
    public class ResourceAttribute
    {
        public string WorkOrder { get; set; }
        public string ArticleName { get; set; }
        public string ArticlePhase { get; set; }
        public string TotalQty { get; set; }

        public bool CanProceed { get; set; }

        public ResourceAttribute(IMesResource resource, IMesManager _MesManager, IMesAppLogger _MesLogger)
        {
            //estrapolo il valore dell'attributo ENABLE_DATA_MANIPULATOR impostato a livello di risorsa (posso procedere solo se la personalizzazione è abilitata per la risorsa)
            bool enableDataManipulator = EnableSetupThreadActivity(resource, Constants.ResourceAttributeName.ENABLE_DATA_MANIPULATOR, _MesManager, _MesLogger);
            if (!enableDataManipulator)
                return;

            this.CanProceed = true;
        }

        //controllo se il valore dell'attributo impostato a livello di risorsa è valido
        public static bool EnableSetupThreadActivity(IMesResource resource, string attributeName, IMesManager _MesManager, IMesAppLogger _MesLogger)
        {
            /*
            * Nome      =>      ENABLE_DATA_MANIPULATOR
            * Tipo      =>      Boolean
            * Valore    =>      1
            */
            var requestAttributeName = resource.Settings.ResourceAttributes.FirstOrDefault(a => a.Name == attributeName);

            var result = requestAttributeName != null &&
                         requestAttributeName.IsValid &&
                         !requestAttributeName.Disabled &&
                         requestAttributeName.Type == ValueContainerType.Boolean &&
                         requestAttributeName.GetConvertedValueToType<bool>();

            if (!result)
                _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                        $@"{resource.Name}: il valore dell'attributo impostato a livello di risorsa NON è valido => il valore di '{attributeName}' è '{result}'");
            //per le risorse che non hanno questo attributo, il codice si spacca con "requestAttributeName.Value"
            //_MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
            //                        $@"{resource.Name}: il valore dell'attributo impostato a livello di risorsa NON è valido => il valore di '{attributeName}' è '{requestAttributeName.Value}'");

            return result;
        }
    }
}
