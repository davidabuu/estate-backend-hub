namespace UserService.Application.DTOs.Responses.Residents;

public record ResidentListResponse(
	List<ResidentSummaryResponse> Residents,
	int TotalCount
);

public record ResidentSummaryResponse(
	Guid Id,
	string FullName,
	string Email,
	string PhoneNumber,
	string HouseType,
	string EstateName,
	DateTime JoinedAt
);