namespace UserService.Application.DTOs.Requests.Auth;

public record LogoutRequestDto(
	string? RefreshToken
);