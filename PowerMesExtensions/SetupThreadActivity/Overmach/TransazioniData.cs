using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamSystem.Customizations.SetupThreadActivity
{
    internal class TransazioniData
    {
        public int IDTransazione { get; set; }
        public int? OrderPhaseId { get; set; }
        public int? OrderPhasesGroupId { get; set; }
        public int? OperatorId { get; set; }
        public int MachineId { get; set; }
        public int ActivityId { get; set; }
        public int? CauseId { get; set; }
        public DateTime Inizio { get; set; }
        public DateTime Fine { get; set; }
        public int PezziBuoni { get; set; }
        public int PezziScarto { get; set; }
        public int PezziNonConformi { get; set; }
        public int? PalletNumber { get; set; }
        public long? Durata { get; set; }
        public long? DurataTotale { get; set; }
        public int? StartingPieces { get; set; }
        public int? PalletPieces { get; set; }
        public bool PingFailed { get; set; }
        public int? PieceNumber { get; set; }
        public int? IdMateriale { get; set; }
        public decimal? QuantitaMateriale { get; set; }
        public int? ToolsChanged { get; set; }
        public string SpindleData { get; set; }
        public int? PartProgramNumber { get; set; }
        public string PartProgramName { get; set; }
        public DateTime? CreationDate { get; set; }
        public string Lotto { get; set; }
        public int? PalletFace { get; set; }
        public bool? IsRevision { get; set; }
        public int DetectionType { get; set; }
        public int? AlarmId { get; set; }
        public bool? IsPalletEndCycle { get; set; }
        public int? CauseSuspensionId { get; set; }
        public int? WorkType { get; set; }
        public string FeedRate { get; set; }
        public string ToolNumber { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? AutomaticSwitchOperator { get; set; }
        public bool? IsEmergency { get; set; }
    }
}
