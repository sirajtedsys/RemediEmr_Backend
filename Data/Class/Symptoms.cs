namespace RemediEmr.Data.Class
{
	public class Symptoms
	{
		public string? SYMP_ID { get; set; }
		public string? SYMP_NAME { get; set; }
		public string? SYMP_SHORT_NAME { get; set; }
		public string? SYMP_ACTIVE_STATUS { get; set; }
		public string? CREATE_USER { get; set; }
		public DateTime? CREATE_DATE { get; set; }

		public string? UPDATE_USER { get; set; }
		public DateTime? UPDATE_DATE { get; set; }
		public string? BRANCH_CODE { get; set; }
		public string? ACTIVE_YEAR { get; set; }
		public string? CREATE_SYMP_ALERT_STATUSUSER { get; set; }
		public string? SPETY_ID { get; set; }
		public string? ACTIVE_STATUS { get; set; }
	}
}
