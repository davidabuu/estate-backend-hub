namespace UserService.Application.DTOs.Responses.Auth;

public record LogoutResponse(
	bool Success,
	string Message
);