using Atys.PowerMES.Foundation;
using System;
using System.Linq;
using System.Reflection;

namespace TeamSystem.Customizations.SetupThreadActivity
{
    public class AttributeCheck
    {
        IMesManager _mesManager = null;

        public AttributeCheck(IMesManager mesManager)
        {
            _mesManager = mesManager;
        }

        /// <summary>
        /// Recupera il valore dell'attibuto stringa, se non presente restituisce "NOT VALID"
        /// </summary>
        /// <param name="mesResource">Risorsa PowerMes</param>
        /// <param name="attributeName">Nome attributo</param>
        /// <returns></returns>
        public string GetStringValueAttribute(IMesResource mesResource, string attributeName)
        {
            //Recupero ID_MACC (MES) attraverso un attributo stringa della risorsa
            var mustProcessAttribute = mesResource.Settings.ResourceAttributes.FirstOrDefault(a => a.Name == attributeName);

            var mustProcess = mustProcessAttribute != null
                        && mustProcessAttribute.IsValid
                        && !mustProcessAttribute.Disabled
                        && mustProcessAttribute.Type == ValueContainerType.String;

            return mustProcess ? mustProcessAttribute.Value : "NOT VALID";
        }

        /// <summary>
        /// Recupera il valore dell'attibuto bool, se non presente o non valido restituisce false
        /// </summary>
        /// <param name="mesResource">Risorsa PowerMes</param>
        /// <param name="attributeName">Nome attributo</param>
        /// <returns></returns>
        public bool GetBoolValueAttribute(IMesResource mesResource, string attributeName)
        {
            var mustProcessAttribute = mesResource.Settings.ResourceAttributes.FirstOrDefault(a => a.Name == attributeName);

            var mustProcess = mustProcessAttribute != null
                                && mustProcessAttribute.IsValid
                                && !mustProcessAttribute.Disabled
                                && mustProcessAttribute.Type == ValueContainerType.Boolean
                                && mustProcessAttribute.GetConvertedValueToType<bool>();

            return mustProcess;
        }

        /// <summary>
        /// Recupera il valore dell'attibuto di tipo int, se non presente restituisce 9999
        /// </summary>
        /// <param name="mesResource">Risorsa PowerMes</param>
        /// <param name="attributeName">Nome attributo</param>
        /// <returns></returns>
        public int GetIntValueAttribute(IMesResource mesResource, string attributeName)
        {
            //Recupero ID_MACC (MES) attraverso un attributo stringa della risorsa
            var mustProcessAttribute = mesResource.Settings.ResourceAttributes.FirstOrDefault(a => a.Name == attributeName);

            var mustProcess = mustProcessAttribute != null
                        && mustProcessAttribute.IsValid
                        && !mustProcessAttribute.Disabled
                        && mustProcessAttribute.Type == ValueContainerType.Integer;

            return mustProcess ? Convert.ToInt32(mustProcessAttribute.Value) : 9999;
        }

        /// <summary>
        /// Attributo stringa a livello servizio
        /// </summary>
        /// <param name="attributeName">Nome attributo da valutare</param>
        /// <returns></returns>
        public string GetServiceStringAttibute(string attributeName)
        {
            var serviceStringAttribute = _mesManager.GeneralSettings.ServiceAttributes.FirstOrDefault(a => a.Name == attributeName);

            var serviceStringValid = serviceStringAttribute != null
                                     && serviceStringAttribute.IsValid
                                     && !serviceStringAttribute.Disabled
                                     && serviceStringAttribute.Type == ValueContainerType.String;

            return serviceStringValid ? serviceStringAttribute.Value : "NOT VALID";
        }

        /// <summary>
        /// Attributo di tipo bool a livello servizio
        /// </summary>
        /// <param name="attributeName">Nome attributo da valutare</param>
        /// <returns></returns>
        public bool GetServiceBoolAttibute(string attributeName)
        {
            var serviceBoolAttribute = _mesManager.GeneralSettings.ServiceAttributes.FirstOrDefault(a => a.Name == attributeName);

            var serviceBoolValid = serviceBoolAttribute != null
                                     && serviceBoolAttribute.IsValid
                                     && !serviceBoolAttribute.Disabled
                                     && serviceBoolAttribute.Type == ValueContainerType.Boolean
                                     && serviceBoolAttribute.GetConvertedValueToType<bool>();

            return serviceBoolValid;
        }
    }
}
