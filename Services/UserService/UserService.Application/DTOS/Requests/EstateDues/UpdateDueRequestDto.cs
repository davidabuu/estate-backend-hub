using UserService.Application.Enums;
using UserService.Domain.Enums;

namespace UserService.Application.DTOS.Requests.EstateDues;

public record UpdateDueRequestDto(
	Guid EstateDueId,
	string? DueName,
	string? Description,
	DueType? DueType,
	DateTime? DueDate,
	Dictionary<PropertyType, decimal>? PropertyTypeAmounts,
	bool? IsActive
);