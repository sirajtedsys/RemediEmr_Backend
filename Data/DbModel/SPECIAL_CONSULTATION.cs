using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Data.DbModel
{
    public class SPECIAL_CONSULTATION
    {
        [Key]
        public string EMR_DOC_ID { get; set; } // Maps to EMR_DOC_ID
        public string PATI_ID { get; set; } // Maps to PATI_ID
        public string SPECIAL_CONSULT { get; set; } // Maps to SPECIAL_CONSULT
        public DateTime? CREATE_DATE { get; set; } // Maps to CREATE_DATE
        public string CREATE_USER { get; set; } // Maps to CREATE_USER
    }
}
