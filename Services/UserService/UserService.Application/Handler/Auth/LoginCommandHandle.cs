using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Auth;
using UserService.Application.DTOs.Responses.Auth;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Service;

namespace UserService.Application.Handler.Auth;

public class LoginCommandHandler(
	UserManager<ApplicationUser> userManager,
	SignInManager<ApplicationUser> signInManager,
	IJwtTokenService jwtTokenService,
	UserDbContext dbContext) : IRequestHandler<LoginCommand, LoginResponse>
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
	private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
	private readonly UserDbContext _dbContext = dbContext;

	public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken ct)
	{
		// 1. Find user
		var user = await _userManager.FindByEmailAsync(command.Email);
		if (user == null)
		{
			throw new Exception("Invalid email or password");
		}

		// 2. Check password
		var result = await _signInManager.CheckPasswordSignInAsync(user, command.Password, false);
		if (!result.Succeeded)
		{
			throw new Exception("Invalid email or password");
		}

		// 3. Check if user is active
		if (!user.IsActive)
		{
			throw new Exception("Your account is deactivated. Please contact support.");
		}

		// 4. Get user roles
		var roles = await _userManager.GetRolesAsync(user);
		var primaryRole = roles.FirstOrDefault() ?? "User";

		// 5. Get Estate details (if EstateManager or Resident)
		string? estateId = null;
		string? estateName = null;
		bool isApproved = false;

		if (primaryRole == "EstateManager")
		{
			var estate = await _dbContext.EstateRegistration
				.FirstOrDefaultAsync(e => e.UserId == user.Id, ct);

			if (estate != null)
			{
				estateId = estate.Id.ToString();
				estateName = estate.EstateName;
				isApproved = estate.IsApproved;
			}
		}
		else if (primaryRole == "Resident")
		{
			var resident = await _dbContext.ResidentRegistration
				.FirstOrDefaultAsync(r => r.UserId == user.Id, ct);

			if (resident != null)
			{
				estateId = resident.EstateId.ToString();
				var estate = await _dbContext.EstateRegistration
					.FirstOrDefaultAsync(e => e.Id == resident.EstateId, ct);
				estateName = estate?.EstateName;
			}
		}

		// 6. Generate JWT Token
		var token = _jwtTokenService.GenerateToken(user, roles);
		var refreshToken = _jwtTokenService.GenerateRefreshToken();

		// 7. Save Refresh Token
		user.RefreshToken = refreshToken;
		user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
		await _userManager.UpdateAsync(user);

		// 8. Return Response
		return new LoginResponse(
			Token: token,
			RefreshToken: refreshToken,
			UserId: user.Id,
			Email: user.Email!,
			FullName: user.FullName ?? user.Email!,
			Role: primaryRole,
			EstateId: estateId,
			EstateName: estateName,
			IsApproved: isApproved,
			ExpiresAt: DateTime.UtcNow.AddMinutes(60)
		);
	}
}