namespace RemediEmr.Data.Class
{
    public class DailyToken
    {
        public string? Patient { get; set; }
        public string? Address { get; set; }
        public string? Sex { get; set; }
        public DateTime? Dob { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? NationalityId { get; set; }
        public string? PatiTypeId { get; set; }
        public string? MaritalStatus { get; set; }
        public string? TokenId { get; set; }
        public string? CounterId { get; set; }
        public string? AgeYear { get; set; }

        public string? P_SALT_ID { get; set; }
        public string? P_IDCARD_ID { get; set; }
        public string? P_CUST_ID { get; set; }
        public string? P_IDCARD_NO { get; set; }

        public string? TokenSeries {  get; set; }
        public string? DeviceId { get; set; }

    }
}
