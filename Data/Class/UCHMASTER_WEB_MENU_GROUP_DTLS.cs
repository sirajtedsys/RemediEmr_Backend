using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Data.Class
{
    public class UCHMASTER_WEB_MENU_GROUP_DTLS
    {
        [Key]
        public int WEB_MENU_GROUP_ID { get; set; }
        public int TAB_ID { get; set; }
        public int MODULE_ID { get; set; }
    }
}
