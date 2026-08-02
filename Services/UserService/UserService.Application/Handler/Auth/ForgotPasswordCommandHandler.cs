using MediatR;
using Microsoft.AspNetCore.Identity;
using UserService.Application.Commands.Auth;
using UserService.Application.DTOs.Responses.Auth;
using UserService.Domain.Entities;

namespace UserService.Application.Handlers.Auth;

public class ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;

	public async Task<ForgotPasswordResponse> Handle(ForgotPasswordCommand command, CancellationToken ct)
	{
		var user = await _userManager.FindByEmailAsync(command.Email);

		if (user == null)
		{
			// For security, don't reveal if user exists
			return new ForgotPasswordResponse(
				true,
				"If your email is registered, you will receive a password reset link."
			);
		}

		// Generate password reset token
		var token = await _userManager.GeneratePasswordResetTokenAsync(user);

		// TODO: Send email with reset link
		// var resetLink = $"https://yourapp.com/reset-password?email={command.Email}&token={Uri.EscapeDataString(token)}";
		// await _emailService.SendPasswordResetEmail(command.Email, resetLink);

		return new ForgotPasswordResponse(
			true,
			"If your email is registered, you will receive a password reset link."
		);
	}
}