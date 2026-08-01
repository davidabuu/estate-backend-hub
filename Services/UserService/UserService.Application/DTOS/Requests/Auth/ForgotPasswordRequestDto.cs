namespace UserService.Application.DTOS.Requests.Auth
{
	public record ForgotPasswordRequestDto
    {
       
        public string Email { get; set; } = "";
    }
}
