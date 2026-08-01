namespace UserService.Application.DTOS.Requests.Auth
{
	public record AuthResultRequestDto
	{
		public bool Success { get; set; }
		public string? Message { get; set; }
		public string? Token { get; set; }
		public IList<string>? Roles { get; set; }
		public string? UserId { get; set; }
	}

}