namespace RemediEmr.Data.Class
{
    public class Vitals
    {
        public string PatiId { get; set; }
        public string VisitId { get; set; }
        public string DoctId { get; set; }
        public string EdocId { get; set; }
        public string? EmpId { get; set; }
        public decimal? Height1 { get; set; }
        public decimal? Weight1 { get; set; }
        public decimal? Temp1 { get; set; }
        public decimal? Pulse1 { get; set; }
        public decimal? MaxBp { get; set; }
        public decimal? MinBp { get; set; }
        public decimal? Rr1 { get; set; }
        public string? Unit1 { get; set; }
        public decimal? HCircum { get; set; }
        public decimal? WCircum { get; set; }
        public decimal? BWeight { get; set; }
        public string? PGrbs { get; set; }
        public decimal? MaxBpLeft { get; set; }
        public decimal? MinBpLeft { get; set; }
        public string? PTempUnit { get; set; }
        public decimal? PSpo2 { get; set; }
        public string? PNurseNote { get; set; }
        public string? PNurseRemarks { get; set; }
        public string? PPainAssessment { get; set; }
        public string? PLocation { get; set; }
        public string? PRemarks { get; set; }
        public string? PAssessmentDistrus { get; set; }
        public string? PBg { get; set; }
        public string? PFsh { get; set; }
        public string? PSupplemental { get; set; }
        public string? PLevelOfConsciousness { get; set; }
        public string? PDuration { get; set; }
        public string? PPainRadiating { get; set; }
        public string? POnsetPain { get; set; }
        public string? PClinicTriage { get; set; }
        public string? PPriority { get; set; }
        public string? PBpDown { get; set; }
        public string? PBpUp { get; set; }
        public string? PHealthEducation { get; set; }
        public string? PUrineAcr { get; set; }
        public string? PHba1c { get; set; }
        public string? PVitaminD { get; set; }
        public string? PNotes { get; set; }
        public string? PBldGrpId { get; set; }

        public string? currentmedication { get; set; }
        public VitalsExtra? Ve { get; set; }
    }
}
