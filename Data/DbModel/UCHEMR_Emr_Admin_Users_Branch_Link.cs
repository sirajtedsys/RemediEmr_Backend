using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Data.DbModel
{
    public class UCHEMR_Emr_Admin_Users_Branch_Link
    {
        [Key]
        public int AUSR_ID { get; set; }
        public int BRANCH_ID { get; set; }
        public int CREATE_USER { get; set; }
        public DateTime CREATE_DATE { get; set; }
    }
}
