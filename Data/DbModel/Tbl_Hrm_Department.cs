using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Class.DbModel
{
    public class Tbl_Hrm_Department
    {
        [Key]
            public string DPT_IDV { get; set; }
        public string DPT_DEPARTMENT { get; set; }
        public string DPT_EXT_CODE { get; set; }
        public string DPT_STATUS { get; set; }
        public int DPTYPE_ID { get; set; }
        public string DPT_MANUAL {  get; set; }
        public string DP_CODE { get; set; }
       public int DPT_ID_HR { get;set; }
        public string DEPARTMENT_MAIL { get; set; }
    }
}
