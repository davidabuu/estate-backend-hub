using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Auth;
using UserService.Application.DTOs.Responses.Auth;
using UserService.Domain.Entities;
using UserService.Infrastructure.Service;

namespace UserService.Application.Handlers.Auth;

public class RefreshTokenCommandHandler(
	UserManager<ApplicationUser> userManager,
	IJwtTokenService jwtTokenService) : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	private readonly IJwtTokenService _jwtTokenService = jwtTokenService;

	public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand command, CancellationToken ct)
	{
		// 1. Find user by refresh token
		var user = await _userManager.Users
			.FirstOrDefaultAsync(u => u.RefreshToken == command.RefreshToken, ct);

		if (user == null)
		{
			throw new Exception("Invalid refresh token");
		}

		// 2. Check if refresh token is expired
		if (user.RefreshTokenExpiry < DateTime.UtcNow)
		{
			throw new Exception("Refresh token has expired. Please login again.");
		}

		// 3. Get user roles
		var roles = await _userManager.GetRolesAsync(user);

		// 4. Generate new tokens
		var newToken = _jwtTokenService.GenerateToken(user, roles);
		var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

		// 5. Update refresh token
		user.RefreshToken = newRefreshToken;
		user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
		await _userManager.UpdateAsync(user);

		return new RefreshTokenResponse(
			Token: newToken,
			RefreshToken: newRefreshToken,
			ExpiresAt: DateTime.UtcNow.AddMinutes(60)
		);
	}
}