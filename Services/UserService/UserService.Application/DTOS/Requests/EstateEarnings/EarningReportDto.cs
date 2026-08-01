namespace UserService.Application.DTOS.Requests.EstateEarnings
{
	public record EarningReportDto
    {
        public int Year { get; set; } 

        public int? Month { get; set; } 

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; } 

        public decimal TotalEarnings { get; set; } 

        public decimal OutstandingPayments { get; set; } 
    }

}
