namespace UserService.Application.DTOS.Requests.Vendor
{
	public record VendorPaymentDto
    {
		public int Id { get; set; }

		public string? Name { get; set; }

		public string? EstateId { get; set; }

		public string? ServiceType { get; set; }

		public string? Bank { get; set; }

		public string? Duration { get; set; }

		public string? ContactName { get; set; }

		public string? Phone { get; set; }

		public DateTime ServiceDate { get; set; }

		public string? PaymentStatus { get; set; }

		public bool IsActive { get; set; } = true;
		public string? Description { get; set; }

		public DateTime DatePaid { get; set; }
	}
}
