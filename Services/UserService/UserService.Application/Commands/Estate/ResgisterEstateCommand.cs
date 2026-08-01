using MediatR;
using UserService.Application.DTOS.Responses.Auth;
using UserService.Application.Enums;

namespace UserService.Application.Commands.Estate;

public record RegisterEstateCommand(
	string Email,
	string Password,
	string EstateName,
	string EstateAddress,
	List<PropertyType> PropertyTypes,
	string BankName,
	string AccountName,
	string AccountNumber,
	string BankCode,
	string? EstateRegistrationDocUrl,
	string? EstateAssociationRegistrationDocUrl
) : IRequest<EstateRegistrationResponse>;