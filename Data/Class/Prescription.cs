namespace RemediEmr.Data.Class
{
    public class Prescription
    {
        public int? MedPslno { get; set; }
        public string? EdocId { get; set; }
        public string? DocId { get; set; }
        public string? MedId { get; set; }
        public string? FreqId { get; set; }
        public string? MedRouteId { get; set; }
        public int? Dur { get; set; }
        public string? Rmrks { get; set; }
        public string? Bf { get; set; }
        public string? Af { get; set; }
        public string? PatI { get; set; }
        public string? CreateUsr { get; set; }
        public string? DosId { get; set; }
        public int? ClaimId { get; set; }
        public int? ClaimPercent { get; set; }
        public int? DiscPercent { get; set; }
        public int? AprvlTypeId { get; set; }
        public string? ReadyToBill { get; set; }
        public decimal? QtyTotal { get; set; }
        public decimal? PSaleBrkQty { get; set; }
        public decimal? PSaleBrkUnit { get; set; }
        public string? DoNotBill { get; set; }
        public string? PTestDose { get; set; }
        public string? Obs { get; set; }
        public string? TotUnit { get; set; }
        public string? PDUnitId { get; set; }
        public string? PDosageVal { get; set; }
        public int? RetVal { get; set; }

    }
}
