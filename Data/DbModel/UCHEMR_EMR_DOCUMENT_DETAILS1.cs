namespace RemediEmr.Data.DbModel
{
    public class UCHEMR_EMR_DOCUMENT_DETAILS1
    {
        public string? EMR_DOC_ID { get; set; }
        public string? PATI_ID { get; set; }
        public string? DOCT_ID { get; set; }
        public DateTime CREATE_DATE { get; set; }
        public string? DOCT_REMARKS { get; set; }
        public string? DOCT_IMMUN { get; set; }
        public string? PRSNT_ILLNESS { get; set; }

        public string? GEN_REMARKS { get; set; }
        public string? COMMENTS { get; set; }
        public string? TREATMENT_REMARKS { get; set; }
        public string? DIAGNOSIS { get; set; }
        public string? IMAGES { get; set; }
        public string? NURSE_ID { get; set; }
        public string? NOTES { get; set; }

        public string? PSYCO_REPORT { get; set; }
        public string? MENTAL_STATUS { get; set; }
        public string? IMPRESSION { get; set; }
        public string? DISCHARGE_NOTE { get; set; }
        public string? PHYSICIAN_NOTE { get; set; }
        public string? FOLLOWUP_REMARKS { get; set; }
        public string? PREVACCINATION { get; set; }

        public DateTime FOLLOWUPDATE { get; set; }
        public string? DOCT_NOTES { get; set; }
    }
}
