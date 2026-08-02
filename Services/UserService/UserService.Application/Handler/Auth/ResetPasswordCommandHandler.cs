using MediatR;
using Microsoft.AspNetCore.Identity;
using UserService.Application.Commands.Auth;
using UserService.Application.DTOs.Responses.Auth;
using UserService.Domain.Entities;

namespace UserService.Application.Handler.Auth;

public class ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;

	public async Task<ResetPasswordResponse> Handle(ResetPasswordCommand command, CancellationToken ct)
	{
		// 1. Validate passwords match
		if (command.NewPassword != command.ConfirmPassword)
		{
			return new ResetPasswordResponse(false, "Passwords do not match");
		}

		// 2. Find user
		var user = await _userManager.FindByEmailAsync(command.Email);
		if (user == null)
		{
			return new ResetPasswordResponse(false, "Invalid email or token");
		}

		// 3. Reset password
		var result = await _userManager.ResetPasswordAsync(user, command.Token, command.NewPassword);

		if (!result.Succeeded)
		{
			var errors = string.Join(", ", result.Errors.Select(e => e.Description));
			return new ResetPasswordResponse(false, errors);
		}

		return new ResetPasswordResponse(true, "Password has been reset successfully. Please login.");
	}
}