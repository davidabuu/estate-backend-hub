namespace UserService.Application.DTOs.Responses.Auth;

public record ResetPasswordResponse(
	bool Success,
	string Message
);