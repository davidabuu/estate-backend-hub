using MediatR;
using UserService.Application.DTOs.Responses.Auth;

namespace UserService.Application.Commands.Auth;

public record RefreshTokenCommand(
	string RefreshToken
) : IRequest<RefreshTokenResponse>;