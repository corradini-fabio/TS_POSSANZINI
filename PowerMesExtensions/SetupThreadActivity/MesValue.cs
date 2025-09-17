using Atys.PowerMES.Foundation;
using Atys.PowerMES.Support;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamSystem.Customizations.SetupThreadActivity
{
    public class MesValue
    {
        public string WorkOrder { get; set; }
        public string ArticleName { get; set; }
        public string ArticlePhase { get; set; }
        public string TotalQty { get; set; }

        public bool CanProceed { get; set; }

        public MesValue(CredentialsVault _CredentialsVault, IMesResource resource, IMesManager _MesManager, IMesAppLogger _MesLogger)
        {
            try
            {
                string connectionString = string.Format(Constants.ConnectionString, _CredentialsVault.InstanceName, _CredentialsVault.DatabaseName, _CredentialsVault.UserName, _CredentialsVault.Password);

                //_MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                //                        $@"{nameof(MesValues)}: the connection string is '{connectionString}'");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string commandString = string.Format(Constants.CommandString, resource.Description);

                    if (resource.Description == "Heidenhain")
                        commandString = string.Format(Constants.CommandString, "FRESA-DOOSAN");

                    _MesLogger.WriteMessage(MessageLevel.Diagnostics, false, Constants.LOGGERSOURCE,
                                            $@"{resource.Name}: il valore di commandString è '{commandString}'");

                    using (SqlCommand command = new SqlCommand(commandString, connection))
                    {
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    this.WorkOrder = Convert.ToString(reader["BOLLA"], CultureInfo.InvariantCulture).Trim().ToUpper();
                                    this.ArticleName = Convert.ToString(reader["COD_ART"], CultureInfo.InvariantCulture).Trim().ToUpper();
                                    this.ArticlePhase = Convert.ToString(reader["COD_FASE"], CultureInfo.InvariantCulture).Trim().ToUpper();
                                    this.TotalQty = Convert.ToString(Convert.ToInt64(Convert.ToDouble(reader["QTA"], CultureInfo.InvariantCulture), CultureInfo.InvariantCulture), CultureInfo.InvariantCulture).Trim().ToUpper();
                                }

                                this.CanProceed = true;
                            }

                            //if (!reader.HasRows)
                            //{
                            //    this.WorkOrder = "0";
                            //    this.ArticleName = string.Empty;
                            //    this.ArticlePhase = string.Empty;
                            //    this.TotalQty = "0";
                            //}
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                _MesLogger.WriteException(e, Constants.LOGGERSOURCE,
                                          $@"{nameof(MesValue)}: Exception during processing." +
                                          $" INFO -> Source: '{e.Source}'; Message: '{e.Message}'");
            }
        }
    }
}
