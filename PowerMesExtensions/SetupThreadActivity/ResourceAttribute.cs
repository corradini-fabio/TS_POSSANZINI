using Atys.PowerMES.Foundation;
using Atys.PowerMES.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamSystem.Customizations.SetupThreadActivity
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
            //estrapolo il valore dell'attributo ENABLE_SETUP_THREAD_ACTIVITY impostato a livello di risorsa (posso procedere solo se la personalizzazione è abilitata per la risorsa)
            bool enableSetupThreadActivity = EnableSetupThreadActivity(resource, Constants.ResourceAttributeName.ENABLE_SETUP_THREAD_ACTIVITY, _MesManager, _MesLogger);
            if (!enableSetupThreadActivity)
                return;

            if(resource.Name == "SKFNG060")
            {
                //estrapolo il valore dell'attributo NUMERO_BOLL impostato a livello di risorsa
                var workOrder = GetAttributeStringValue(resource, Constants.ResourceAttributeName.WORK_ORDER, _MesManager, _MesLogger);
                if (string.IsNullOrWhiteSpace(workOrder) || string.IsNullOrEmpty(workOrder))
                    return;


                //estrapolo il valore dell'attributo NUMERO_BOLL impostato a livello di risorsa
                var articleName = GetAttributeStringValue(resource, Constants.ResourceAttributeName.ARTICLE_NAME, _MesManager, _MesLogger);
                if (string.IsNullOrWhiteSpace(articleName) || string.IsNullOrEmpty(articleName))
                    return;

                //estrapolo il valore dell'attributo NUMERO_BOLL impostato a livello di risorsa
                var articlePhase = GetAttributeStringValue(resource, Constants.ResourceAttributeName.ARTICLE_PHASE, _MesManager, _MesLogger);
                if (string.IsNullOrWhiteSpace(articlePhase) || string.IsNullOrEmpty(articlePhase))
                    return;

                //estrapolo il valore dell'attributo NUMERO_BOLL impostato a livello di risorsa
                var totalQty = GetAttributeStringValue(resource, Constants.ResourceAttributeName.TOTAL_QTY, _MesManager, _MesLogger);
                if (string.IsNullOrWhiteSpace(totalQty) || string.IsNullOrEmpty(totalQty))
                    return;


                this.WorkOrder = workOrder;
                this.ArticleName = articleName;
                this.ArticlePhase = articlePhase;
                this.TotalQty = totalQty;
            }

            this.CanProceed = true;
        }

        //controllo se il valore dell'attributo impostato a livello di risorsa è valido
        public static bool EnableSetupThreadActivity(IMesResource resource, string attributeName, IMesManager _MesManager, IMesAppLogger _MesLogger)
        {
            /*
            * Nome      =>      ENABLE_SETUP_THREAD_ACTIVITY
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

        //controllo se il valore dell'attributo impostato a livello di risorsa è valido
        private string GetAttributeStringValue(IMesResource resource, string attributeName, IMesManager _MesManager, IMesAppLogger _MesLogger)
        {
            var requestAttributeName = resource.Settings.ResourceAttributes.FirstOrDefault(a => a.Name == attributeName);

            var result = requestAttributeName != null &&
                         requestAttributeName.IsValid &&
                         !requestAttributeName.Disabled &&
                         requestAttributeName.Type == ValueContainerType.String;

            if (!result)
                _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                        $@"{resource.Name}: il valore dell'attributo impostato a livello di risorsa NON è valido => il valore di '{attributeName}' è '{result}'");
            //per le risorse che non hanno questo attributo, il codice si spacca con "requestAttributeName.Value"
            //_MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
            //                        $@"{resource.Name}: il valore dell'attributo impostato a livello di risorsa NON è valido => il valore di '{attributeName}' è '{requestAttributeName.Value}'");

            return result ? requestAttributeName.Value : string.Empty;
        }
    }
}
