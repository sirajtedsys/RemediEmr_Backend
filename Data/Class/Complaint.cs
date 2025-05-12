using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace RemediEmr.Data.Class
{
    public class Complaint
    {
        // UCHEMR.EMR_SP_PRESC_ADVICE
        public string? P_EMR_DOC_ID { get; set; }
        public string? P_PRESC_ADV { get; set; }
        public string? P_INVEST_ADV { get; set; }
        public string? P_RADIO_ADV { get; set; }
        public string? P_CREATED_USER { get; set; }
        public int? P_TYPE { get; set; }
        public string? SYSTEM_EXAM { get; set; }
        public string? PHYSICAL_EXAM { get; set; }
        public string? SYSTEMIC { get; set; }
        public string? LOCALEX { get; set; }

        // UCHEMR.SP_ONLINE_TAB_PMH_OTHER_SAVE
        public string? PATIID { get; set; }
        public string? p_mh_Other { get; set; }
        public string? P_FAMILY_MED_HISTORY { get; set; }
        public string? P_DOCID { get; set; }
        //public string? P_EMR_DOC_ID { get; set; }
        // UCHEMR.SP_ONLINE_COMP_IMMU_SAVE
        //public string? PATIID { get; set; }
        public string? EDOCID { get; set; }
        public string? DOCID { get; set; }
        public string? COMPLNT { get; set; }
        public string? HSTRY { get; set; }
        public string? DOCT_REMARKS { get; set; }
        public string? PRSNT_ILLNESS { get; set; }

        public string? IMMUN { get; set; }
        public string? GENRMRKS { get; set; }
        public string? I_TREATMENT_REMARKS { get; set; }
        public string? P_TREATMENT_REMARKS_NEW { get; set; }
        public string? P_NOTES { get; set; }
        public string? P_DOCT_NOTES { get; set; }
        //RETVAL OUT NUMBER
        //UCHEMR.SP_ONLINE_PATI_OPTN_NOTE_SAVE
        //public string? PATIID { get; set; }
        public string? EDOC_ID { get; set; }
        public string? P_OPERTN_NOTE { get; set; }
        public string? USER_ID { get; set; }
        // UCHEMR.SP_ONLINE_DOC_ICDVISIT_INS
        public string? CREATEUSR { get; set; }
        public int? ICDSL_NO { get; set; }
        //public string? EDOCID { get; set; }
        //public string? DOCID { get; set; }
        public string? ICDDIGNOSIS { get; set; }
        public string? ICD_DIGNOSIS { get; set; }

        public string? PATI { get; set; }
        public string? BRANCHCODE { get; set; }
        //RETVAL OUT NUMBER
        //UCHEMR.EMR_BASIC_ASSESSMENT-table
        public string? CURRENT_MEDICATION { get; set; }
        public string? PATI_ID { get; set; }


        public Vitals vital { get; set; }

        public List<ICDselection> IcdList { get; set; }

        // UCHEMR.SP_ONLINE_ALLERGY_UPD
        //public string? PATIID { get; set; }
        public string? ALLERGYDTLS { get; set; }
        public string? P_ALLERGY_STATUS { get; set; }
        public string? P_DOCT_ID { get; set; }

        //UCHEMR.SP_CPAST_MEDISUR_SAVE

        //public string? PATIID { get; set; }
        //public string? EDOC_ID { get; set; }
        public string? P_MEDI_SUR_HISTORY { get; set; }
        public string? P_PATI_ADVICE { get; set; }
        public string? P_PATI_EDU { get; set; }
        public string? P_PATI_COMORBIDITY { get; set; }
        public string? P_PSYCHO_SOCIAL { get; set; }
        //public string? USER_ID { get; set; }

    }
}
