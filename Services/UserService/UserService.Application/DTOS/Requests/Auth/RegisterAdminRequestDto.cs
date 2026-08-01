namespace UserService.Application.DTOS.Requests.Auth;

public class RegisterAdminRequestDto
{
	public string? Email { get; set; } 
	public string? Password { get; set; }
	public string? FullName { get; set; }
}