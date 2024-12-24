using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Data.DbModel
{
    public class UCHMASTER_Hrm_employee_branch_Link
    {
        [Key]
        public int EMP_LINK_ID { get; set; }
        public int EMP_ID { get; set; }
        public int BRANCH_ID { get; set; }
        public int CREATE_USER {  get; set; }
        public DateTime? CREATE_DATE    { get; set; }
    }
}
