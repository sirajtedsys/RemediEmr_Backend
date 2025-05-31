namespace RemediEmr.Data.Class
{
    public class ICDselection
    {
        //(object)
        public int? icdId { get; set; }
        public int? icdSlNo { get; set; }
        public int? icdCodeId { get; set; }
        public int? icdCodeDtlsId { get; set; }
        public string? remarks { get; set; }
        public string? emrDocId { get; set; }
        public string? patiId { get; set; }
        public string? mrdUpdateSts { get; set; } = "N";
        public string? mrdUpdateUsr { get; set; } = null;
        public DateTime? mrdUpdateDate { get; set; } = null;
    }
}
