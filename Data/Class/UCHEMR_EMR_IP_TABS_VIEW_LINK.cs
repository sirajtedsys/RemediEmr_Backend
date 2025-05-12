using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Data.Class
{
    public class UCHEMR_EMR_IP_TABS_VIEW_LINK
    {
        [Key]
        public int TAB_ID { get; set; }
        public string? TAB_NAME { get; set; }
        public int TAB_ORDER { get; set; }
        public char? ACTIVE_STATUS { get; set; }
        public char? HISTORY_STATUS { get; set; }
        public int IVFPRIORITY { get; set; }
        public string? LINK { get; set; }
        public int MODULE_ID { get; set; }
        public int ADMIN_LINK_ID { get; set; }
        public int ROOT_LINK_ID { get; set; }
        public char? DISCHRGD_PATI_MENU { get; set; }
        public string? LINK_TYPE { get; set; } = "IP";
        public int COM_TYPE { get; set; } = 0;
        public string? FOOTER { get; set; }

        public char? INCLUDED_PROCEDURE { get; set; }
        public char? INCLUDED_OP { get; set; }
        public char? INCLUDED_IP { get; set; }
        public string? PRIORITY { get; set; }
        public string? OP_IP { get; set; } = "IP";
    }
}
