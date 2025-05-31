using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Data.Class
{
    public class UCHMASTER_WEB_MENU_GROUP
    {
        [Key]
        public int WEB_MENU_GROUP_ID { get; set; }
        public string? WEB_MENU_GROUP { get; set; }
        public char? ACTIVE_STATUS { get; set; } = 'A';
        public string? CREATE_USER { get; set; }
        public string? UPDATE_USER { get; set; }
        public DateTime? UPDATE_DATE { get; set; }
    }
}
