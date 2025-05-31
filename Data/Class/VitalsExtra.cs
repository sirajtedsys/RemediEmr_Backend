namespace RemediEmr.Data.Class
{
    public class VitalsExtra
    {
        // Common Information
        public string? PatientId { get; set; }
        public string? DoctorId { get; set; }
        public string? EmrDocId { get; set; }

        // Medical History
        public string? MhOther { get; set; }
        public string? FamilyMedHistory { get; set; }

        // Complaint and Immunization
        public string? Complaint { get; set; }
        public string? History { get; set; }
        public string? Immunization { get; set; }
        public string? GeneralRemarks { get; set; }
        public string? TreatmentRemarksNew { get; set; }
        public string? Notes { get; set; }
        public string? DoctorNotes { get; set; }

        // Allergy Information
        public string? AllergyDetails { get; set; }
        public string? AllergyStatus { get; set; }
    }
}
