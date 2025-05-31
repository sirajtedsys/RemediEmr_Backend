using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Data.Class
{
    public class Progressnote
    {
        [Key]
        public string? P_PHI_EVA_DTLS_ID { get; set; }
        public string? P_EMR_DOC_ID { get; set; }
        public string? P_PATI_ID { get; set; }
        public string? P_PHYSIOTHERAPIST { get; set; }
        public int? P_SESSION_NO { get; set; }
        public string? P_START_DATE { get; set; }
        //public DateTime? P_START_DATE { get; set; }
        public int? P_ADVISED_SESSION { get; set; }
        public int? P_CURRENT_SESSION { get; set; }
        public string? P_VISIT_DATE { get; set; }
        public string? P_START_TIME { get; set; }
        public string? P_END_TIME { get; set; }
        //public DateTime? P_VISIT_DATE { get; set; }
        //public DateTime? P_START_TIME { get; set; }
        //public DateTime? P_END_TIME { get; set; }
        public string? P_TREATMENT_CODE { get; set; }
        public string? P_PATIENT_CONDITION { get; set; }
        public string? P_PATIENT_MANAGEMENT { get; set; }
        public string? P_ADVERSE_EFFECT { get; set; }
        public string? P_ADVERSE_EFFECT_COMMENTS { get; set; }
        public string? P_RESPONSE_TREATMENT { get; set; }
        public string? P_RESPONSE_TREATMENT_OTHER { get; set; }
        public string? P_INCIDENT { get; set; }
        public string? P_INCIDENT_OTHER { get; set; }
        public string? P_NOTES { get; set; }
        public string? P_TYPE { get; set; }
        public string? P_PATI_SAFETY_ENSURE { get; set; }
        //RETVAL OUT VARCHAR2

        //public string? PATI_ID { get; set; }
        //public string? EMR_DOC_ID { get; set; }
        //public string? CREATED_USER { get; set; }
        //public string? CREATED_DATE { get; set; }
        //public string? EntryDate { get; set; }
        public string? Subjective { get; set; }
        public string? Objective { get; set; }
        public string? Planning { get; set; }
        public string? Assessment { get; set; }
        //public int? PhysioId { get; set; }

    }
}
