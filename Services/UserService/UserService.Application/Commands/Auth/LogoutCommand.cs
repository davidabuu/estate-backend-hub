using MediatR;
using UserService.Application.DTOs.Responses.Auth;

namespace UserService.Application.Commands.Auth;

public record LogoutCommand(
	string? RefreshToken
) : IRequest<LogoutResponse>;