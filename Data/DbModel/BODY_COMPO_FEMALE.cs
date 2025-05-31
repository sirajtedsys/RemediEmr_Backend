using System.ComponentModel.DataAnnotations;

namespace RemediEmr.Data.DbModel
{
    public class BODY_COMPO_FEMALE
    {
        [Key]
        public string? EMR_DOC_ID { get; set; }
        public string? PATI_ID { get; set; }
        public DateTime? SAVE_TIME { get; set; }
        public string? WEIGHT { get; set; }
        public string? BODY_FAT { get; set; }
        public string? VISCERAL_FAT { get; set; }
        public string? RESTING_METABOLISM { get; set; }
        public string? SKELETAL_MUSCLE { get; set; }
        public string? SKELETAL_MUSCLE_ARMS { get; set; }

        public string? SKELETAL_MUSCLE_TRUNK { get; set; }
        public string? SKELETAL_MUSCLE_LEGS { get; set; }
        public string? SUBCUT_FAT { get; set; }
        public string? SUBCUT_ARMS { get; set; }
        public string? SUBCUT_TRUNK { get; set; }
        public string? SUBCUT_LEG { get; set; }
        public string? BMI { get; set; }
        public string? BODY_AGE { get; set; }
        public string? HEIGHT { get; set; }

        public string? WEIGHT1 { get; set; }
        public string? CREATED_USER { get; set; }
        public DateTime? CREATED_DATE { get; set; }
    }
}
