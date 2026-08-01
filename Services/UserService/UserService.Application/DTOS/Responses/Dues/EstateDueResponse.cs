using UserService.Application.Enums;
using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Responses.Dues;

public record EstateDueResponse(
	Guid Id,
	string DueName,
	string? Description,
	decimal Amount,
	DueType DueType,
	DateTime DueDate,
	Dictionary<PropertyType, decimal> PropertyTypeAmounts,
	int TotalResidents,
	int PaidCount,
	int PendingCount,
	int OverdueCount,
	bool IsActive,
	DateTime CreatedAt,
	DateTime? UpdatedAt
);