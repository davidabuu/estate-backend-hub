namespace UserService.Application.DTOS.Requests.EstateEarnings
{
	public record EstateEarnings
    {
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalUnPaidAmount { get; set; }
    }

}
