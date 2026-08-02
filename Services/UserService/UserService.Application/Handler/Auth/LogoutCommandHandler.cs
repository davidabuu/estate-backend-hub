using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Auth;
using UserService.Application.DTOs.Responses.Auth;
using UserService.Domain.Entities;

namespace UserService.Application.Handlers.Auth;

public class LogoutCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<LogoutCommand, LogoutResponse>
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;

	public async Task<LogoutResponse> Handle(LogoutCommand command, CancellationToken ct)
	{
		if (string.IsNullOrEmpty(command.RefreshToken))
		{
			return new LogoutResponse(true, "Logged out successfully");
		}

		var user = await _userManager.Users
			.FirstOrDefaultAsync(u => u.RefreshToken == command.RefreshToken, ct);

		if (user != null)
		{
			user.RefreshToken = null;
			user.RefreshTokenExpiry = null;
			await _userManager.UpdateAsync(user);
		}

		return new LogoutResponse(true, "Logged out successfully");
	}
}