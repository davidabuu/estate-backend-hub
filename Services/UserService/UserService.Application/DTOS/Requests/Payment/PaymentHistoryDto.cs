namespace UserService.Application.DTOS.Requests.Payment
{
	public record PaymentHistoryDto
    {
        public int Id { get; set; }
        public string? DueName { get; set; }
        public string? EstateId { get; set; }
        public int DueId { get; set; }
        public string? UserId { get; set; }
        public string? PaymentReference { get; set; }
        public string? Email { get; set; }
        public bool DisbursedToEstate { get; set; }

        public string? DisbursementReference { get; set; }
        public decimal? Amount { get; set; }
        public int DueDuration { get; set; }
       
      
        public bool IsPaid { get; set; }
        public DateTime PaymentDate { get; set; }
      
    }
}
