
namespace UserService.Application.DTOs.Responses.Residents;

public record ResidentProfileResponse(
	Guid Id,
	string FirstName,
	string LastName,
	string Email,
	string PhoneNumber,
	string Sex,
	string HouseType,
	string MeterNumber,
	string HouseAddress,
	string EstateName,
	string RegisterAs,
	bool IsRegistered,
	DateTime JoinedAt
);