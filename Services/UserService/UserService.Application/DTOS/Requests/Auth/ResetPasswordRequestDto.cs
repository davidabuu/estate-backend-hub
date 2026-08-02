namespace UserService.Application.DTOs.Requests.Auth;

public record ResetPasswordRequestDto(
	string Email,
	string Token,
	string NewPassword,
	string ConfirmPassword
);