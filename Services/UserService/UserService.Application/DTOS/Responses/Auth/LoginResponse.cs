namespace UserService.Application.DTOs.Responses.Auth;

public record LoginResponse(
	string Token,
	string RefreshToken,
	Guid UserId,
	string Email,
	string FullName,
	string Role,
	string? EstateId,
	string? EstateName,
	bool IsApproved,
	DateTime ExpiresAt
);