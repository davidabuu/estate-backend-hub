namespace UserService.Application.DTOs.Responses.Dues;

public record UpdateDueResponse(
	Guid EstateDueId,
	string DueName,
	int ResidentsUpdated,
	string Message,
	DateTime UpdatedAt
);