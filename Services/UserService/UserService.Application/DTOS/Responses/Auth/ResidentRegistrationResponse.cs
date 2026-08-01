namespace UserService.Application.DTOS.Responses.Auth;

public record ResidentRegistrationResponse(
	string Email,
	string FullName,
	string EstateName,
	string Message

);