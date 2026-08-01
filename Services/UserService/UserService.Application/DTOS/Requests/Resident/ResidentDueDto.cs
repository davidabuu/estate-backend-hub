namespace UserService.Application.DTOS.Requests.Resident
{
	public record ResidentDueDto
    {
		public int Id { get; set; }
		public string? DueName { get; set; }
	
		public string? Severity { get; set; }
	
		public decimal? Amount { get; set; }
		public int DueDuration { get; set; }
		public bool IsActive { get; set; }
		
		public decimal? ServiceChargeFee { get; set; }
		

		
		public bool IsPaid { get; set; }
	

		
		
	}
}
