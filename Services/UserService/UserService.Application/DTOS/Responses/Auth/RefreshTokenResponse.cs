namespace UserService.Application.DTOs.Responses.Auth;

public record RefreshTokenResponse(
	string Token,
	string RefreshToken,
	DateTime ExpiresAt
);