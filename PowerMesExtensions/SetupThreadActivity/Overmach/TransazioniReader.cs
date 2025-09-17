using Atys.PowerMES.Foundation;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamSystem.Customizations.SetupThreadActivity
{
    internal class TransazioniReader
    {
        private readonly string _dbOverOneNewConn;
        private readonly string _atysPowerMesConn;
        private readonly IMesManager _MesManager;

        public TransazioniReader(string dbOverOneNewConn, string atysPowerMesConn, IMesManager mesManager)
        {
            _dbOverOneNewConn = dbOverOneNewConn;
            _atysPowerMesConn = atysPowerMesConn;
            _MesManager = mesManager;
        }

        public List<TransazioniData> ReadNewTransazioni(int machineId)
        {
            const string methodName = nameof(ReadNewTransazioni);
            SqlConnection dbOverOneNewConn = null;
            SqlConnection atysPowerMesConn = null;
            try
            {
                int lastId = GetLastIDTransazione(machineId, ref atysPowerMesConn);
                List<TransazioniData> newTransazioni = GetTransazioniFromDb(machineId, lastId, ref dbOverOneNewConn);

                if (newTransazioni.Count > 0)
                {
                    UpdateLastIDTransazione(machineId, newTransazioni[newTransazioni.Count - 1].IDTransazione, ref atysPowerMesConn);

                    return newTransazioni;
                }

                return null;
            }
            catch (Exception ex)
            {
                _MesManager.AppendMessageToLog(MessageLevel.Error, methodName, ex.ToString());
                throw;
            }
            finally
            {
                if (dbOverOneNewConn != null && dbOverOneNewConn.State == ConnectionState.Open)
                    dbOverOneNewConn.Close();
                if (atysPowerMesConn != null && atysPowerMesConn.State == ConnectionState.Open)
                    atysPowerMesConn.Close();
            }
        }

        private int GetLastIDTransazione(int machineId, ref SqlConnection connection)
        {
            const string query = "SELECT LastIDTransazione FROM TransazioniReference WHERE MachineId = @MachineId";
            connection = new SqlConnection(_atysPowerMesConn);
            SqlCommand command = null;
            try
            {
                connection.Open();
                command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MachineId", machineId);

                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                _MesManager.AppendMessageToLog(MessageLevel.Error, nameof(GetLastIDTransazione), ex.ToString());
                throw;
            }
            finally
            {
                if (command != null) command.Dispose();
            }
        }

        private List<TransazioniData> GetTransazioniFromDb(int machineId, int lastId, ref SqlConnection connection)
        {
            const string query = @"
            SELECT * 
            FROM Transazioni
            WHERE MachineId = @MachineId AND IDTransazione > @LastId
            ORDER BY IDTransazione";

            connection = new SqlConnection(_dbOverOneNewConn);
            SqlCommand command = null;
            SqlDataReader reader = null;
            var transazioniList = new List<TransazioniData>();

            try
            {
                connection.Open();
                command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MachineId", machineId);
                command.Parameters.AddWithValue("@LastId", lastId);

                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var transazione = new TransazioniData
                    {
                        IDTransazione = reader.GetInt32(reader.GetOrdinal("IDTransazione")),
                        OrderPhaseId = reader.IsDBNull(reader.GetOrdinal("OrderPhaseId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("OrderPhaseId")),
                        OrderPhasesGroupId = reader.IsDBNull(reader.GetOrdinal("OrderPhasesGroupId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("OrderPhasesGroupId")),
                        OperatorId = reader.IsDBNull(reader.GetOrdinal("OperatorId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("OperatorId")),
                        MachineId = reader.GetInt32(reader.GetOrdinal("MachineId")),
                        ActivityId = reader.GetInt32(reader.GetOrdinal("ActivityId")),
                        CauseId = reader.IsDBNull(reader.GetOrdinal("CauseId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CauseId")),
                        Inizio = reader.IsDBNull(reader.GetOrdinal("Inizio")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("Inizio")),
                        Fine = reader.IsDBNull(reader.GetOrdinal("Fine")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("Fine")),
                        PezziBuoni = reader.IsDBNull(reader.GetOrdinal("PezziBuoni")) ? 0 : reader.GetInt32(reader.GetOrdinal("PezziBuoni")),
                        PezziScarto = reader.IsDBNull(reader.GetOrdinal("PezziScarto")) ? 0 : reader.GetInt32(reader.GetOrdinal("PezziScarto")),
                        PezziNonConformi = reader.IsDBNull(reader.GetOrdinal("PezziNonConformi")) ? 0 : reader.GetInt32(reader.GetOrdinal("PezziNonConformi")),
                        PalletNumber = reader.IsDBNull(reader.GetOrdinal("PalletNumber")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("PalletNumber")),
                        Durata = reader.IsDBNull(reader.GetOrdinal("Durata")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("Durata")),
                        StartingPieces = reader.IsDBNull(reader.GetOrdinal("StartingPieces")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("StartingPieces")),
                        PalletPieces = reader.IsDBNull(reader.GetOrdinal("PalletPieces")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("PalletPieces")),
                        PingFailed = reader.GetBoolean(reader.GetOrdinal("PingFailed")),
                        PieceNumber = reader.IsDBNull(reader.GetOrdinal("PieceNumber")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("PieceNumber")),
                        IdMateriale = reader.IsDBNull(reader.GetOrdinal("IdMateriale")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("IdMateriale")),
                        QuantitaMateriale = reader.IsDBNull(reader.GetOrdinal("QuantitaMateriale")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("QuantitaMateriale")),
                        ToolsChanged = reader.IsDBNull(reader.GetOrdinal("ToolsChanged")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ToolsChanged")),
                        SpindleData = reader.IsDBNull(reader.GetOrdinal("SpindleData")) ? null : reader.GetString(reader.GetOrdinal("SpindleData")),
                        PartProgramNumber = reader.IsDBNull(reader.GetOrdinal("PartProgramNumber")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("PartProgramNumber")),
                        PartProgramName = reader.IsDBNull(reader.GetOrdinal("PartProgramName")) ? null : reader.GetString(reader.GetOrdinal("PartProgramName")),
                        CreationDate = reader.IsDBNull(reader.GetOrdinal("CreationDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CreationDate")),
                        Lotto = reader.IsDBNull(reader.GetOrdinal("Lotto")) ? null : reader.GetString(reader.GetOrdinal("Lotto")),
                        PalletFace = reader.IsDBNull(reader.GetOrdinal("PalletFace")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("PalletFace")),
                        IsRevision = reader.IsDBNull(reader.GetOrdinal("IsRevision")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("IsRevision")),
                        DetectionType = reader.IsDBNull(reader.GetOrdinal("DetectionType")) ? 0 : reader.GetInt32(reader.GetOrdinal("DetectionType")),
                        AlarmId = reader.IsDBNull(reader.GetOrdinal("AlarmId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("AlarmId")),
                        IsPalletEndCycle = reader.IsDBNull(reader.GetOrdinal("IsPalletEndCycle")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("IsPalletEndCycle")),
                        CauseSuspensionId = reader.IsDBNull(reader.GetOrdinal("CauseSuspensionId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CauseSuspensionId")),
                        WorkType = reader.IsDBNull(reader.GetOrdinal("WorkType")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("WorkType")),
                        FeedRate = reader.IsDBNull(reader.GetOrdinal("FeedRate")) ? null : reader.GetString(reader.GetOrdinal("FeedRate")),
                        ToolNumber = reader.IsDBNull(reader.GetOrdinal("ToolNumber")) ? null : reader.GetString(reader.GetOrdinal("ToolNumber")),
                        ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
                        AutomaticSwitchOperator = reader.IsDBNull(reader.GetOrdinal("AutomaticSwitchOperator")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("AutomaticSwitchOperator")),
                        IsEmergency = reader.IsDBNull(reader.GetOrdinal("IsEmergency")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("IsEmergency")),
                    };

                    transazioniList.Add(transazione);
                }

                return transazioniList;
            }
            catch (Exception ex)
            {
                _MesManager.AppendMessageToLog(MessageLevel.Error, nameof(GetTransazioniFromDb), ex.ToString());
                throw;
            }
            finally
            {
                if (reader != null && !reader.IsClosed) reader.Close();
                if (command != null) command.Dispose();
            }
        }

        private void UpdateLastIDTransazione(int machineId, int lastId, ref SqlConnection connection)
        {
            const string query = @"
            IF EXISTS (SELECT 1 FROM TransazioniReference WHERE MachineId = @MachineId)
                UPDATE TransazioniReference
                SET LastIDTransazione = @LastId
                WHERE MachineId = @MachineId
            ELSE
                INSERT INTO TransazioniReference (MachineId, LastIDTransazione)
                VALUES (@MachineId, @LastId)";

            connection = new SqlConnection(_atysPowerMesConn);
            SqlCommand command = null;

            try
            {
                connection.Open();
                command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MachineId", machineId);
                command.Parameters.AddWithValue("@LastId", lastId);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _MesManager.AppendMessageToLog(MessageLevel.Error, nameof(UpdateLastIDTransazione), ex.ToString());
                throw;
            }
            finally
            {
                if (command != null) command.Dispose();
            }
        }
    }
}
