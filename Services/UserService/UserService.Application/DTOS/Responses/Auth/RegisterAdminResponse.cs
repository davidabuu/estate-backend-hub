namespace UserService.Application.DTOS.Responses.Auth;

public record RegisterAdminResponse(
	Guid UserId,
	string Email,
	string FullName,
	string Role,
	string Message,
	DateTime CreatedAt
);