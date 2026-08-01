namespace UserService.Application.DTOS.Requests.EstateDues
{
	public record EstateDuePaymentStatusDto
    {
        public string? DueName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal OutstandingFees { get; set; }
        public int PaidDues { get; set; }
        public int UnpaidDues { get; set; }
        public DateTime DateRegistered { get; set; }
    }
}
