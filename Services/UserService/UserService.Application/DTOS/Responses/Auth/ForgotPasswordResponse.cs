namespace UserService.Application.DTOs.Responses.Auth;

public record ForgotPasswordResponse(
	bool Success,
	string Message
);