using MediatR;
using UserService.Application.DTOs.Responses.Auth;


namespace UserService.Application.Commands.Auth;

public record LoginCommand(
	string Email,
	string Password
) : IRequest<LoginResponse>;