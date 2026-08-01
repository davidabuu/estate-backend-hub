using MediatR;

using UserService.Application.DTOS.Responses.Auth;
using UserService.Application.Enums;

namespace UserService.Application.Commands.Resident;

public record RegisterResidentCommand(
	string FirstName,
	string LastName,
	string Email,
	string PhoneNumber,
	string Password,
	string RegisterAs,
	string HouseType,
	string MeterNumber,
	string HouseAddress,
	Guid EstateId
) : IRequest<ResidentRegistrationResponse>;