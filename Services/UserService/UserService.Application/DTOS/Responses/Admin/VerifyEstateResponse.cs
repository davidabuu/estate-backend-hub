namespace UserService.Application.DTOs.Responses.Admin;

public record VerifyEstateResponse(
	Guid EstateId,
	string EstateName,
	bool IsApproved,
	string Message,
	DateTime ProcessedAt
);