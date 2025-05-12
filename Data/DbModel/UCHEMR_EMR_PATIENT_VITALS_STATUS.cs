using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Data.DbModel
{
    public class UCHEMR_EMR_PATIENT_VITALS_STATUS
    {
        [Key]
        public string? PATI_ID { get; set; }
        public string? VISIT_ID { get; set; }
        public string? DOCT_ID { get; set; }
        public string? HEIGHT { get; set; }
        public string? WEIGHT { get; set; }
        public int?TEMP { get; set; }
        public int?PULSE { get; set; }
        public char? UNIT { get; set; }
        public DateTime? EVENT_DT { get; set; }
        public char? STATUS { get; set; }
        public int?MAX_BP { get; set; }
        public int?MIN_BP { get; set; }
        public int?RR { get; set; }
        public string? EMR_DOC_ID { get; set; }
        public string? EMP_ID { get; set; }

        public int?HCIRCUMFERENCE { get; set; }
        public int?BP_LEFTMAX { get; set; }
        public int?BP_LEFTMIN { get; set; }

        public char?TEMP_UNIT { get; set; }
        public string? VITAL_EMP_ID { get; set; }
        public string? NURSE_NOTE { get; set; }

        public int?SPO2 { get; set; }
        public string? LOCATION { get; set; }
        public string? PAINREMARKS { get; set; }
        public string? PATIENTASSESSMENTDISTRESSLEVEL { get; set; }
        public string? NURSE_REMARKS { get; set; }
        public string? GRBS { get; set; }

        public int?BIRTH_WEIGHT { get; set; }
        public int?WCIRCUMFERENCE { get; set; }
        public int?BP_UP { get; set; }
        public string? BG { get; set; }
        public string? NURSE { get; set; }

        public char? SUPPLEMENTAL { get; set; }
        public char? LEVEL_OF_CONSCIOUSNESS { get; set; }
        public string? DURATION { get; set; }
        public string? PAIN_RADIATING { get; set; }
        public string? ONSET_PAIN { get; set; }

        public int?CLINIC_TRIAGE { get; set; }
        public int?PRIORITY { get; set; }
        public char? HEALTH_EDUCATION { get; set; }
        public int?URINE_ACR { get; set; }
        public int?HBA1C { get; set; }

        public int?VITAMIN_D { get; set; }
        public string? NOTES { get; set; }
        public string? BLD_GRP_ID { get; set; }
        public int?FSH { get; set; }
    }
}
