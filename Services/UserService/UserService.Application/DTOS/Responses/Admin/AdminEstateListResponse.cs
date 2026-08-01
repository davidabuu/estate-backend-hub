namespace UserService.Application.DTOs.Responses.Admin;

public record AdminEstateListResponse(
	Guid EstateId,
	string EstateName,
	string EstateAddress,
	string EstateState,
	
	string AdminEmail,
	string AdminFullName,
	bool IsApproved,
	DateTime CreatedAt,
	DateTime? ApprovedAt,
	int TotalResidents,
	int TotalDues
);