namespace RemediEmr.Data.Class
{
	public class CaseDetails
	{
		public int CaseId { get; set; }
		public string? CaseName { get; set; }
		public string? ActiveStatus { get; set; }
		public string? CaseCode { get; set; }
		public string? emrDocId { get; set; }
		public string? TreatmentSts { get; set; } = "N";
		public string? Medication { get; set; }
		public string? Remarks { get; set; }
		public bool OnTreatment { get; set; }
		public bool CaseCheck { get; set; }
	

	}
}
