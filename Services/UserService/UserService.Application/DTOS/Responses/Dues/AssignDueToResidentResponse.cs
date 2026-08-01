namespace UserService.Application.DTOs.Responses.Dues;

public record AssignDueToResidentResponse(
	Guid ResidentDueId,
	string DueName,
	string ResidentName,
	decimal Amount,
	string Message,
	DateTime AssignedAt
);