using MediatR;
using UserService.Application.DTOs.Responses.Auth;

namespace UserService.Application.Commands.Auth;

public record ForgotPasswordCommand(
	string Email
) : IRequest<ForgotPasswordResponse>;