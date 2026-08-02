namespace UserService.Application.DTOS.Responses.Auth;


public record EstateRegistrationResponse(
	Guid UserId,
	Guid EstateId,
	string Email,
	string EstateName,
	string Message,
	bool IsApproved,
	DateTime DateRegistered
);