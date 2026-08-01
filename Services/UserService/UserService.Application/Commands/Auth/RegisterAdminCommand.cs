using MediatR;
using UserService.Application.DTOS.Responses.Auth;




namespace UserService.Application.Commands.Auth;

public record RegisterAdminCommand(
	string Email,
	string Password,
	string FullName
) : IRequest<RegisterAdminResponse>;