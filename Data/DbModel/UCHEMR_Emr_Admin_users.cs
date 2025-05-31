using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Class.DbModel
{
    public class UCHEMR_Emr_Admin_users
    {
        [Key]
        public int AUSR_ID { get; set; }
        public string AUSR_USERNAME { get; set; }
        public string AUSR_PWD { get; set; }
        public string AUSR_STATUS { get; set; }
        public string APP_LOGIN_ALLOWED { get; set; }
    }
}
