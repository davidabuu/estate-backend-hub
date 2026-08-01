namespace UserService.Application.DTOs.Responses.Dues;

public record CreateDueResponse(
	Guid EstateDueId,
	string DueName,
	int ResidentsAssigned,
	List<string> ResidentsWithoutAmount,
	string Message,
	string? Warning,
	DateTime CreatedAt
);