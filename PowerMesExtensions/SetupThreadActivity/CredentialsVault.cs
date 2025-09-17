using Atys.PowerMES.Foundation;
using Atys.PowerMES.Foundation.Services;
using Atys.PowerMES.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamSystem.Customizations.SetupThreadActivity
{
    public class CredentialsVault
    {
        /// <summary>
        /// Imposta e restituisce il nome dell'istanza
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// Imposta e restituisce il nome del database
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Imposta e restituisce il nome utente
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Imposta e restituisce la password
        /// </summary>
        public string Password { get; set; }

        public CredentialsVault(IMesManager _MesManager, IMesAppLogger _MesLogger)
        {
            //codice univoco che deve essere generato nel vault dell'authentication manager
            var vaultUniqueCode = Guid.Parse(Constants.VaultUniqueCode);

            var authService = _MesManager.ServiceManager.GetService<IAuthenticationService>();

            var credentials = authService.GetVaultItemByCode(vaultUniqueCode);

            if (credentials == null || !credentials.Enabled)
            {
                _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                        $@"{nameof(CredentialsVault)}: Error in authentication manager vault");

                return;
            }

            this.InstanceName = Constants.InstanceName;
            this.DatabaseName = Constants.DatabaseName;
            this.UserName = credentials.UserName;
            this.Password = credentials.Password;
        }
    }
}
