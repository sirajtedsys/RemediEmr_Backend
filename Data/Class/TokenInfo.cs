namespace RemediEmr.Data.Class
{
    public class TokenInfo
    {
        public string SpecName { get; set; }
        public string Doctor { get; set; }
        public string PatientOpNo { get; set; }
        public string BillCustomerName { get; set; }
        public string BillGender { get; set; }
        public string BillHdrId { get; set; }
        public string TokenCatId { get; set; }
        public string TokenReadVoiceStatus { get; set; }
        public string RoomName { get; set; }
        public string TokenNo { get; set; }
        public string TokenCallStatus { get; set; }
        public string HoldStatus { get; set; }
        public string TokenReadStatus { get; set; }
        public string RoomNo { get; set; }
        public string PROFILE_PIC { get; set; }
    }


    public class DoctorTokenInFo
    {

        public string DEPARTMENT { get; set; }
        public string DOCTOR_NAME { get; set; }
        public string OPVDTLS_ID { get; set; }
        public string ID { get; set; }
        public string TOKEN_READ_STS { get; set; }
        public string TOKEN_CALL_DATE { get; set; }
        public string LAST_CALLED { get; set; }
        public string OPNO { get; set; }
        public string PATIENT_NAME { get; set; }
        public string BG_COLOR { get; set; }
        public string TOKEN { get; set; }
        public string ROOM_NO { get; set; }
        public string EMP_ID { get; set; }
        public string PROFILE_PIC { get; set; }
        //        {
        //    "OPVDTLS_ID": "UCHM00000011468",
        //    "ID": 121,
        //    "TOKEN_READ_STS": "N",
        //    "TOKEN_CALL_DATE": "2025-04-25T12:01:35",
        //    "LAST_CALLED": "N",
        //    "OPNO": "23-645",
        //    "PATIENT_NAME": "ROSE MARIA SAJI ",
        //    "DOCTOR_NAME": "Dr.SUSAN THOMAS",
        //    "PROFILE_PIC": "doctor.png",
        //    "BG_COLOR": "#FFFFFF",
        //    "DEPARTMENT": "OBS & GYNAECOLOGY",
        //    "TOKEN": "W-1",
        //    "ROOM_NO": "S-OP2",
        //    "EMP_ID": "UCHM000013"
        //}
    }
}
