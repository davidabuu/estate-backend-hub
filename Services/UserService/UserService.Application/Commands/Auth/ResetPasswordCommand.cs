using MediatR;
using UserService.Application.DTOs.Responses.Auth;

namespace UserService.Application.Commands.Auth;

public record ResetPasswordCommand(
	string Email,
	string Token,
	string NewPassword,
	string ConfirmPassword
) : IRequest<ResetPasswordResponse>;