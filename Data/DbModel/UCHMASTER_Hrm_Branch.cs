using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Data.DbModel
{
    public class UCHMASTER_Hrm_Branch
    {
        [Key] 
        public int BRANCH_ID { get; set; }
        public string? BRCH_NAME { get; set; }
        public string? SHORT_NAME { get; set; }
        public int? BRCH_CAT_ID { get; set; }
        public int? CREATE_USER {get;set;}
        public DateTime? CREATE_DATE {get;set;}
        public int? UPDATE_USER { get; set; }
        public DateTime? UPDATE_DATE {get;set; }
        public string? BRCH_ADDRESS1 { get; set; }
        public string? BRCH_ADDRESS2 { get; set; }
        public int? FIN_YEAR_ID { get; set; }
        public string? ACTIVE_STATUS { get; set; }
        public string? BRCH_ADDRESS3 { get; set; }
        public int? COMP_ID { get; set; }
        public string? BRCH_PINCODE { get; set; }
        public string? BRCH_PHONE { get; set; }
        public DateTime? BRCH_START_DATE { get; set; }
        public string? BRCH_EMAIL { get; set; }
        public string? BRCH_FAX { get; set; }
        public int? PT_LOC_ID { get; set; }
        public string? HO_BRANCH { get; set; }   //SINGLE CHAR
        public string? CO_BRANCH { get; set; }   //SINGLE CHAR
        public int? ISR_RGRP_ID { get; set; }
        public string? OPNO_PREFIX { get; set; }
        public string? EMAIL_ID { get; set; }
        public string? EMAIL_PASSWORD  { get; set; }
        public string? MSEND_MAIL_TO { get; set; }


    }
}
