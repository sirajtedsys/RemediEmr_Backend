using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Data.DbModel
{
    public class UCHMASTER_Hrm_Employee
    {
        [Key]
        public string EMP_ID { get; set; }
        public string? EMP_CODE { get; set; }
        public string? EMPT_ID { get; set; }
        public string? EMPG_ID { get; set; }
        public string? EMP_CUR_COMP_ID { get; set; }
        public string? EMP_CUR_DPT_ID { get; set; }
        public string? EMP_CUR_DSG_ID { get; set; }
        public string? EMP_CUR_SAL_COMP_ID { get; set; }
        public string? EMP_CUR_HR_COMP_ID { get; set; }
        public string? EMP_F_NAME { get; set; }
        public string? EMP_L_NAME { get; set; }
        public string? EMP_OFFL_NAME { get; set; }
        public string? EMP_PHOTO { get; set; }
        public string? EMP_EMAIL { get; set; }
        public char? EMP_MARITAL_STATUS { get; set; }
        public char? EMP_GENDER { get; set; }
        public DateTime? EMP_DOB { get; set; }
        public string? EMP_FAMILY_NAME { get; set; }
        public string? EMP_PLACE_BIRTH { get; set; }
        public string? EMP_DIST_ID { get; set; }
        public string? EMP_STATE_ID { get; set; }
        public string? EMP_NAT_ID { get; set; }
        public string? RLG_ID { get; set; }
        public string? EMP_CAST { get; set; }
        public decimal? EMP_HEIGHT { get; set; }
        public decimal? EMP_WEIGHT { get; set; }
        public string? EMP_COL_EYE { get; set; }
        public string? EMP_IDENT_MARK { get; set; }
        public string? EMP_GUARDIAN { get; set; }
        public string? EMP_GUARDIAN_REL { get; set; }
        public int? EMP_GUARDIAN_AGE { get; set; }
        public string? EMP_GUARDIAN_PROF { get; set; }
        public string? EMP_PRES_PLACE_ID { get; set; }
        public string? EMP_PRES_ADDR { get; set; }
        public string? EMP_PRES_PHONE { get; set; }
        public string? EMP_PRES_MOBILE { get; set; }
        public string? EMP_PERM_PLACE_ID { get; set; }
        public string? EMP_PERM_ADDR { get; set; }
        public string? EMP_PERM_PHONE { get; set; }
        public string? EMP_PERM_MOBILE { get; set; }
        public string? EMP_CONT_PERSON { get; set; }
        public string? EMP_CONT_PER_ADDR { get; set; }
        public string? EMP_CONT_PER_PHONE { get; set; }
        public string? EMP_PASSPORT_NO { get; set; }
        public string? EMP_PASS_ISSU_PLACE { get; set; }
        public DateTime? EMP_PASS_ISSU_DATE { get; set; }
        public DateTime? EMP_PASS_EXP_DATE { get; set; }
        public char? EMP_EMIGR_STATUS { get; set; }
        public string? EMP_REL_HERID_DISEASE { get; set; }
        public string? EMP_REL_PHY_MENTAL { get; set; }
        public char? EMP_ASHTMA { get; set; }
        public char? EMP_COL_BLINDNESS { get; set; }
        public char? EMP_DEAFNESS { get; set; }
        public char? EMP_VD { get; set; }
        public char? EMP_STD { get; set; }
        public string? EMP_CIVIL_CRIMINAL_CS { get; set; }
        public DateTime? EMP_DOJ { get; set; }
        public char? EMP_HOUSE_CONST_TYPE { get; set; }
        public string? EMP_AGRI_LAND { get; set; }
        public string? EMP_OTHER_LAND { get; set; }
        public string? EMP_BUSINESS_TYPE { get; set; }
        public decimal? EMP_BUSINESS_INCOME { get; set; }
        public string? EMP_NM_REG_NO { get; set; }
        public decimal? EMP_CUR_BASIC { get; set; }
        public decimal? EMP_CUR_DA { get; set; }
        public decimal? EMP_CUR_ALLOW { get; set; }
        public string? EMP_REMARKS { get; set; }
        public string? EMP_PF_ACC_NO { get; set; }
        public string? EMP_ESI_NO { get; set; }
        public char? EMP_LEAVE_ALLOW { get; set; }
        public DateTime? EMP_RETIREMENT_DT { get; set; }
        public decimal? EMP_EXTRA_PF_AMT { get; set; }
        public decimal? EMP_EXTRA_PF_PERC { get; set; }
        public decimal? EMP_SEC_DEPOSIT { get; set; }
        public string? BNK_ID { get; set; }
        public string? EMP_BNK_ACC_NO { get; set; }
        public char? EMP_PAY_MODE { get; set; }
        public string? EMP_REC_EMP_ID { get; set; }
        public string? EMP_REC_PERSON { get; set; }
        public string? EMP_PUNCH_CARD { get; set; }
        public DateTime? EMP_PF_START_DT { get; set; }
        public string? EMP_ESI_IP_NO { get; set; }
        public string? EMP_PAN_NO { get; set; }
        public string? EMP_BADGE_NO { get; set; }
        public string? EMP_LOGIN_NAME { get; set; }
        public string? EMP_PASSWORD { get; set; }
        public string? EMP_LEVEL_STRING { get; set; }
        public string? STS_ID { get; set; }
        public string? EMP_COMPUTER_KNOW { get; set; }
        public char? EMP_ACCOMODATION { get; set; }
        public char? EMP_ACCOMODATION_TYPE { get; set; }
        public string? ENQ_ID { get; set; }
        public string? SCT_ID { get; set; }
        public string? EMP_ID_OLD { get; set; }
        public char? EMP_ACTIVE_STATUS { get; set; }
        public string? EMP_CTN_CODE { get; set; }
        public string? EMP_GROUP_PHOTO { get; set; }
        public char? TMP_DESIG { get; set; }
        public char? NEED_PATIENT_EDIT { get; set; } = 'N';
        public char? EMP_SHORT_NAME { get; set; }
        public char? IS_LIMIT_APPLICABLE { get; set; }
        public char? SW_USER { get; set; }
        public char? SW_SUPER_USER { get; set; }
        public char? EMP_PRINT_MESSAGE { get; set; }
        public char? ALLOW_DISCH_BILL_EDIT { get; set; }
        public int? EMP_ID_HR { get; set; }
        public char? ALLOW_VIEW_OLD_BILLS { get; set; }
        public char? ALLOW_DUPLICATE_BILL_PRINT { get; set; }
        public DateTime? RESIGN_DATE { get; set; }
        public char? SUPER_USER { get; set; }
        public string? SALT_ID { get; set; }
        public char? OP_CASH_ONLY { get; set; }
        public char? VIEW_ALL_SALE_BILLS { get; set; }
        public int? ACC_MASTER_ID { get; set; }
        public decimal? USR_DUE_LIMIT_PERC { get; set; }
        public int? FDR_ACC_MASTER_ID { get; set; }
        public int? ADV_ACC_MASTER_ID { get; set; }
        public int? LOAN_ACC_MASTER_ID { get; set; }
        public string? EMP_ACC_MPD_USR { get; set; }
        public DateTime? EMP_ACC_MPD_DATE { get; set; }
        public string? OLD_EMP_CODE { get; set; }
        public int? CPR_ID { get; set; }
        public char? EMP_EMR_DOCT_PREV { get; set; }
        public char? EMP_VIEW_ALL_PATIENTS { get; set; }
        public char? ALLOW_GEN_REFUND { get; set; }
        public char? EMR_LOGIN_ALLOWED { get; set; }
        public char? EMR_VIEW { get; set; }
        public string? EMP_ISR_CODE { get; set; }
        public char SHOW_PREV_FIN_DATA { get; set; } = 'Y';
        public char? DEFAULTSESSION { get; set; }
        public char? PHY_TOKEN_LOG_ALLOW { get; set; }
        public char EMP_OT_PARAM_EDITABLE { get; set; } = 'N';
        public string? EMP_UNIQUE_CODE { get; set; }
        public char EMP_OT_EYE_CHANGE { get; set; } = 'N';
        public char? EMR_SPL_PERMISSION { get; set; }
        public char? EMP_CREDIT_LIMIT_TYPE { get; set; }
        public char? IP_EMR_PERMISSION { get; set; }
        public decimal? EMP_EXTRA_KSW_AMT { get; set; }
        public decimal? EMP_EXTRA_KSW_PERC { get; set; }
        public DateTime? EMP_KSW_START_DT { get; set; }
        public string? EMP_KSW_ACC_NO { get; set; }
        public string? EMR_LOGIN_TYPE { get; set; } = "000";
        public int? DUG_ID { get; set; }
        public string? EMP_OFFL_MOBILE_NO { get; set; }
        public string? EMP_OFFL_EMAIL_ID { get; set; }
        public char? STOCK_UPDATE { get; set; }
        public decimal? CTN_CREDIT_LIMIT { get; set; }
        public char? CTN_CREDIT { get; set; }
        public string? UPDATE_USER { get; set; }
        public DateTime? UPDATE_DATE { get; set; }
        public int? KCEWF_CODE { get; set; }
        public char? DEPT_NAME_AS_DOCTOR { get; set; }
        public char? IS_DUMMY_EMP { get; set; }
        public char? EMR_ALL_PATIENT_VIEW_IP { get; set; }
        public char? IS_RMO { get; set; }
        public char? IS_THERAPIST { get; set; } = 'N';
        public char? SHOW_ON_SAMPLE_COLLECTION { get; set; }
        public decimal? EMP_COMP_OFF_COUNT { get; set; }
        public decimal? EMP_COMP_OFF_SETTLE_COUNT { get; set; }
        public char? OUTSIDE_DOCTOR { get; set; }
        public char? IS_OT { get; set; }
        public int? EMP_PUNCH_NA { get; set; }
        public int? OT_ELIGIBLE { get; set; }
        public decimal? ADDL_ESI_AMT { get; set; }
        public int? DUG_PRIORITY { get; set; }
        public char? STAFF_CREDIT_ALLOWED { get; set; }
        public int? INDEM_ELIGIBLE { get; set; }
        public char? IS_APP_BOOKING { get; set; }
        public string? EMP_PRES_SAVE_CODE { get; set; }
        public int? SHIFT_ID { get; set; }
        public char? STAFF_CREDIT_BILLED { get; set; }
        public char? ALLOW_MODULE_MULTIPLE_LOGIN { get; set; } = 'N';
        public char? DOCT_ORDER_ALLOWED { get; set; }
        public char? DOC_ORDER_APP_LOGIN { get; set; }
        public int? ATTN_GRACE_PERIOD_MINUTES { get; set; }
        public int? WEB_MENU_GROUP_ID { get; set; }
        public int? WEB_SCT_GROUP_ID { get; set; }
        public int? WEB_WARD_GROUP_ID { get; set; }
        public int? APP_EMP_GROUP_ID { get; set; }
        public string? MOD_LANGUAGE { get; set; }
        public int? RAD_PRC_GROUP_ID { get; set; }
        public char? NEED_ONLINE_LIVE_TOKEN_STS { get; set; }
    }



}

