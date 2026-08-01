
namespace EstateHub.Contracts.Events;
public record EstateManagerRegisteredEvent(
	Guid UserId,
	string Email,
	string CompanyName,
	string PhoneNumber,
	string Address,
	string RegistrationNumber, 
	DateTime RegisteredAt
);