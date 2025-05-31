using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Data.DbModel
{
    public class Doctor
    {
        [Key]
        public string DOCT_ID { get; set; }
        public string EMPLOYEE_ID { get; set; }
        public string DTYP_ID { get; set; }
        public string SPEC_ID { get; set; }
        public string DOCT_TELE_EXT_NO { get; set; }
        public string DOCT_REG_NO { get; set; }
        public DateTime? DOCT_REG_DATE { get; set; }
        public decimal? DOCT_INDEMNITY_NO { get; set; }
        public char? DOCT_RNEW_TYPE { get; set; }
        public char? DOCT_RNEW_PERIOD_TYPE { get; set; }
        public int? DOCT_PERIOD_NUMBER { get; set; }
        public int? DOCT_TOTAL_VISITS { get; set; }
        public char? DOCT_CONSULT_FEE_TYPE { get; set; }
        public decimal? DOCT_AMOUNT_FIRST { get; set; }
        public decimal? DOCT_AMOUNT_SECOND { get; set; }
        public decimal? DOCT_AMOUNT_THIRD { get; set; }
        public char? DOCT_ACTIVE_STATUS { get; set; }
        public string CREATE_USER { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public DateTime? UPDATE_DATE { get; set; }
        public int? ACTIVE_YEAR { get; set; }
        public string BRANCH_CODE { get; set; }
        public decimal? DOCT_LOAN_MAX { get; set; }
        public decimal? DOCT_CASH_COMMI { get; set; }
        public DateTime? DOCT_SETTLE_CASHDATE { get; set; }
        public decimal? DOCT_CREDIT_COMMI { get; set; }
        public DateTime? DOCT_SETTLE_CREDITDATE { get; set; }
        public decimal? DOCT_OP_DAILY_INCOME { get; set; }
        public decimal? DOCT_PHARMACY_DAILY_INCOME { get; set; }
        public decimal? DOCT_NITE_FEE { get; set; }
        public decimal? DOCT_EMGNCY_FEE { get; set; }
        public string DO_CODE { get; set; }
        public string DOCT_SHORT_NAME { get; set; }
        public string DOCT_DISPLAYNAME { get; set; }
        public string DOCT_ROOM_NO { get; set; }
        public string DOCT_QUALIFICATION { get; set; }
        public string DOCT_PARTICULARES { get; set; }
        public int? MED_DEPT_ID { get; set; }
        public char? DOCT_DISC_EDIT { get; set; }
        public int? CHAMBER_ID { get; set; }
        public int? VISIT_DURATION { get; set; }
        public decimal? DOCT_CONSULT_RENW__FEE { get; set; }
        public int? COST_CENTER_ID { get; set; }
        public string CC_UPDATE_USER { get; set; }
        public DateTime? CC_UPDATE_DATE { get; set; }
        public decimal? CONSULT_SHARE_PERC { get; set; }
        public string PRC_REMARKS { get; set; }
        public string CONSULT_REMARKS { get; set; }
        public int? BENCH_ID { get; set; }
        public char? FEE_EDITABLE { get; set; }
        public char PKG_CONSULTATION { get; set; }
        public string ROOM_NO { get; set; }
        public char? ONLINE_APPOINTMENT { get; set; }
        public char TREAT_NEW_PATIENTS { get; set; }
        public char? VAT_NEEDED { get; set; }
        public char? EMERGENCY_REG { get; set; }
        public char? AVP_AVAILABLE { get; set; }
        public char? AVAILABLE_ECHS { get; set; }
        public string DISPLAY_NAME_ID { get; set; }
        public int? OD_CAT_ID { get; set; }
        public char? APPOINTMENT_NEED { get; set; }
        public int? MAX_WAITING_LIST_COUNT { get; set; }
        public char? REFERRAL_DOCTOR { get; set; }
        public char? OTHER_DOCTOR { get; set; }
        public char? CPP_NEEDED { get; set; }
        public char? IS_RMO { get; set; }
        public char? DOCT_INSURANCE { get; set; }
        public char? SEPERATE_LEDGER_NEEDED { get; set; }
        public int? ACC_MASTER_ID { get; set; }
        public int? REG_ACC_MASTER_ID { get; set; }
        public int? INS_REG_ACC_MASTER_ID { get; set; }
        public int? INS_CONS_ACC_MASTER_ID { get; set; }
        public int? IP_ACC_MASTER_ID { get; set; }
        public int? INS_IP_ACC_MASTER_ID { get; set; }
        public char? SIMPLE_PACKAGE { get; set; }
        public string DOCT_TOKEN_PREFIX { get; set; }
        public char? COVID_BILLING_DOCTOR { get; set; }
        public int? BRANCH_ID { get; set; }
        public char? DOCT_SHARE_NEEDED { get; set; }
        public char? DOCTOR_SHARE_TAX { get; set; }
        public int DEFAULT_BRANCH_ID { get; set; }
        public int? PREVI_VISITS_TO_PRESCRIPTION { get; set; }
        public char? NEED_APPOINTMENT_ADVANCE { get; set; }
        public int? LEVEL_ID { get; set; }
        public char? TIME_WISE_CONSULT_FEE { get; set; }
        public string FOLLOWUP_DOCT_ID { get; set; }
        public char? NEED_VALIDITY_DAYS { get; set; }
        public int? WAITING_TIME_MINUTUES { get; set; }
        public int? TAT_TIME_MINUTES { get; set; }
        public int? IP_PREVI_VISITS_TO_PRESCRI { get; set; }
        public string LAST_VITALS_DATA { get; set; }
        public char DIALYSIS { get; set; }
        public int? DR_ORDER_IN_DEPT_APP { get; set; }
        public char? BLOCK_NEW_PATIENT_APPOINTMENT { get; set; }

        public string? DEF_PAGE {  get; set; }
    }
}
