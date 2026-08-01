namespace UserService.Application.DTOs.Responses.Dues;

public record DeleteDueResponse(
	Guid EstateDueId,
	string DueName,
	int ResidentsAffected,
	string Message,
	DateTime DeletedAt
);
