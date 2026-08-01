namespace UserService.Application.DTOS.Requests.Auth
{
	public record ResetPasswordRequestDto
    {
       
      
      
        public string Email { get; set; } = "";

        public string Token { get; set; } = "";

		public string Password { get; set; } = "";

        public string ConfirmPassword { get; set; } = "";
	}
}
