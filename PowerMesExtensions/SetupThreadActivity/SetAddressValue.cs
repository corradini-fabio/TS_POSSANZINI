using Atys.PowerMES.Device;
using Atys.PowerMES.Foundation;
using Atys.PowerMES.Services;
using Atys.PowerMES.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamSystem.Customizations.SetupThreadActivity
{
    public class SetAddressValue
    {
        public bool CanProceed { get; set; }

        public SetAddressValue(IMesResource resource, ResourceAttribute resourceAttribute, MesValue mesValue, IMesManager _MesManager, IMesAppLogger _MesLogger)
        {
            /*
             * In "PowerMES Quick Start Assistant" devi andare in "System Components", "PowerDevice Integration".
             * Poi, devi abilitare "PowerDEVICE integration enabled" e "PowerDEVICE servers" attraverso gli appositi checkbox
             * 
             * In "PowerDVC Client" devi andare in "Vista", "Parametri del server", "Connected Commands".
             * Poi, devi abilitare "Abilita servizio Connected Commands" attraverso l'apposito checkbox
             * 
             * In "PowerDVC Client" devi andare in "Vista", "Parametri del server", "MES Client".
             * Poi, devi abilitare "Abilita Mes Client" attraverso l'apposito checkbox
             */
            var dvcService = _MesManager.ServiceManager.GetService<IDvcIntegrationService>();

            if (dvcService == null || !dvcService.Enabled)
            {
                _MesLogger.WriteMessage(MessageLevel.Diagnostics, true, Constants.LOGGERSOURCE,
                                          "PowerDVC integration not available");

                return;
            }

            const string dvcInstance = "localhost";

            //scrivo sul PLC il valore estrapolato dal MES e faccio un controllo per assicurarmi di averlo scritto correttamente
            var setWorkOrderAddressValueResult = (dvcService.SetAddressValue(resourceAttribute.WorkOrder, dvcInstance, mesValue.WorkOrder) as DvcWriteOperationResult);
            if (!setWorkOrderAddressValueResult.Success)
            {
                _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                        $@"{resource.Name}: il valore NON è stato scritto correttamente sull'address del PLC => il valore di setWorkOrderAddressValueResult.Success è '{setWorkOrderAddressValueResult.Success}'");
                return;
            }

            //scrivo sul PLC il valore estrapolato dal MES e faccio un controllo per assicurarmi di averlo scritto correttamente
            var setArticleNameAddressValueResult = (dvcService.SetAddressValue(resourceAttribute.ArticleName, dvcInstance, mesValue.ArticleName) as DvcWriteOperationResult);
            if (!setArticleNameAddressValueResult.Success)
            {
                _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                        $@"{resource.Name}: il valore NON è stato scritto correttamente sull'address del PLC => il valore di setArticleNameAddressValueResult.Success è '{setArticleNameAddressValueResult.Success}'");
                return;
            }

            //scrivo sul PLC il valore estrapolato dal MES e faccio un controllo per assicurarmi di averlo scritto correttamente
            var setArticlePhaseAddressValueResult = (dvcService.SetAddressValue(resourceAttribute.ArticlePhase, dvcInstance, mesValue.ArticlePhase) as DvcWriteOperationResult);
            if (!setArticlePhaseAddressValueResult.Success)
            {
                _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                        $@"{resource.Name}: il valore NON è stato scritto correttamente sull'address del PLC => il valore di setArticlePhaseAddressValueResult.Success è '{setArticlePhaseAddressValueResult.Success}'");
                return;
            }

            //scrivo sul PLC il valore estrapolato dal MES e faccio un controllo per assicurarmi di averlo scritto correttamente
            var setTotalQtyAddressValueResult = (dvcService.SetAddressValue(resourceAttribute.TotalQty, dvcInstance, mesValue.TotalQty) as DvcWriteOperationResult);
            if (!setTotalQtyAddressValueResult.Success)
            {
                _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                        $@"{resource.Name}: il valore NON è stato scritto correttamente sull'address del PLC => il valore di setTotalQtyAddressValueResult.Success è '{setTotalQtyAddressValueResult.Success}'");
                return;
            }

            this.CanProceed = true;
        }
    }
}
