using UserService.Application.Enums;
using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Responses.Dues;

public record ResidentDueResponse(
	Guid Id,
	Guid EstateDueId,
	string DueName,
	string? Description,
	decimal Amount,
	DueType DueType,
	DateTime DueDate,
	DueStatus Status,
	bool IsPaid,
	PropertyType PropertyType,
	DateTime? PaidAt,
	string? PaymentReference,
	int DaysUntilDue,
	bool IsOverdue,
	DateTime CreatedAt
);