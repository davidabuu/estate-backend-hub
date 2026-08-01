namespace UserService.Application.DTOS.Requests.Auth
{
	public record EstateSubAdminRequestDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? EmailAddress { get; set; }

        public string? Role = "Estate Exec";

        public string? EstateId { get; set; }


    }
}
