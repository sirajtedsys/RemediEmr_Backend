using System;

namespace RemediEmr.Data.Class
{
    public class Careplan
    {

        public string? PATI_ID { get; set; }
        public string? EMR_DOC_ID { get; set; }
        public string? CREATED_USER { get; set; }
        public DateTime? CREATED_DATE { get; set; }
        public DateTime? EntryDate { get; set; }
        public string? Subjective { get; set; }
        public string? Objective { get; set; }
        public string? Planning { get; set; }
        public string? Assessment { get; set; }
        public int? PhysioId { get; set; }
    }
}
