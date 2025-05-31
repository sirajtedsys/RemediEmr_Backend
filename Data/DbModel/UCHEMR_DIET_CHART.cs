using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Data.DbModel
{
    public class UCHEMR_DIET_CHART
    {
        [Key]
        public string? PATI_ID { get; set; }
        public string? EMR_DOC_ID { get; set; }
        public string? CREATED_USER { get; set; }
        public DateTime? CREATED_DATE { get; set; }
        public string? DIET_SPEC { get; set; }
        public string? FOOD_PREF { get; set; }
        public string? TOTAL_FLUID { get; set; }
        public string? EARLY_MORNING { get; set; }
        public string? BREAKFAST { get; set; }
        public string? ELEVENPM { get; set; }
        public string? LUNCH { get; set; }
        public string? FOURPM { get; set; }

        public string? SEVENPM { get; set; }
        public string? BEDTIME { get; set; }
        public string? SPECIAL_INSTRUCTION { get; set; }
        public string? EIGHT { get; set; }
    }
}
