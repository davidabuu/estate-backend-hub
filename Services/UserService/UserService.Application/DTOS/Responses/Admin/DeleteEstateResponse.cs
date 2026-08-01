namespace UserService.Application.DTOs.Responses.Admin;

public record DeleteEstateResponse(
	Guid EstateId,
	string EstateName,
	int ResidentsDeleted,
	int DuesDeleted,
	string Message,
	DateTime DeletedAt
);