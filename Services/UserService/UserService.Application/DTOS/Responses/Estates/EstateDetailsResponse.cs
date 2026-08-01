
using UserService.Application.Enums;

namespace UserService.Application.DTOS.Responses.Estates;

public record EstateDetailsResponse(
	Guid Id,
	string Name,
	string Address,

	
	List<PropertyType> PropertyTypes,
	DateTime CreatedAt,
	DateTime? ApprovedAt
);