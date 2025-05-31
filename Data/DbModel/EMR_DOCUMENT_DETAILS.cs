using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Data.DbModel
{
    public class EMR_DOCUMENT_DETAILS
    {
        [Key]
        public string?EMR_DOC_ID { get; set; } // Maps to EMR_DOC_ID
        public string?PATI_ID { get; set; } // Maps to PATI_ID
        public string?DOCT_ID { get; set; } // Maps to DOCT_ID
        public DateTime? CREATE_DATE { get; set; } // Maps to CREATE_DATE
        public string?DOCT_REMARKS { get; set; } // Maps to DOCT_REMARKS
        public string?DOCT_IMMUN { get; set; } // Maps to DOCT_IMMUN
        public string?PRSNT_ILLNESS { get; set; } // Maps to PRSNT_ILLNESS
        public string?GEN_REMARKS { get; set; } // Maps to GEN_REMARKS
        public string?COMMENTS { get; set; } // Maps to COMMENTS
        public string?TREATMENT_REMARKS { get; set; } // Maps to TREATMENT_REMARKS
        public string?TREATMENT_REMARKS_NEW { get; set; } // Maps to TREATMENT_REMARKS_NEW
        public string?DIAGNOSIS { get; set; } // Maps to DIAGNOSIS
        public string?IMAGES { get; set; } // Maps to IMAGES
        public string?NURSE_ID { get; set; } // Maps to NURSE_ID
        public string?NOTES { get; set; } // Maps to NOTES
        public string?PSYCO_REPORT { get; set; } // Maps to PSYCO_REPORT
        public string?MENTAL_STATUS { get; set; } // Maps to MENTAL_STATUS
        public string?IMPRESSION { get; set; } // Maps to IMPRESSION
        public string?DISCHARGE_NOTE { get; set; } // Maps to DISCHARGE_NOTE
        public string?PHYSICIAN_NOTE { get; set; } // Maps to PHYSICIAN_NOTE
        public string?FOLLOWUP_REMARKS { get; set; } // Maps to FOLLOWUP_REMARKS
        public string?PREVACCINATION { get; set; } // Maps to PREVACCINATION
        public DateTime? FOLLOWUPDATE { get; set; } // Maps to FOLLOWUPDATE
        public string?DOCT_NOTES { get; set; } // Maps to DOCT_NOTES
    }
}
