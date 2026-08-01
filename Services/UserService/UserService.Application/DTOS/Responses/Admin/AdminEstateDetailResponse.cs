using UserService.Application.Enums;


namespace UserService.Application.DTOs.Responses.Admin;

public record AdminEstateDetailResponse(
	Guid EstateId,
	string EstateName,
	string EstateAddress,
	List<PropertyType> PropertyTypes,
	string BankName,
	string AccountName,
	string AccountNumber,
	string BankCode,
	
	string? EstateRegistrationDocUrl,
	string? EstateAssociationRegistrationDocUrl,
	bool IsApproved,
	
	DateTime CreatedAt,
	DateTime? ApprovedAt,
	int TotalResidents,
	int TotalDues,
	List<ResidentSummary> Residents,
	List<DueSummary> Dues
);

public record ResidentSummary(
	Guid Id,
	string FullName,
	string Email,
	string PhoneNumber,
	string PropertyType,
	DateTime JoinedAt
);

public record DueSummary(
	Guid Id,
	string DueName,
	decimal Amount,
	string DueType,
	DateTime DueDate,
	int TotalResidents,
	int PaidCount,
	int PendingCount,
	int OverdueCount
);