using MediatR;
using Microsoft.AspNetCore.Identity;
using UserService.Application.Commands.Auth;
using UserService.Application.DTOS.Responses.Auth;
using UserService.Domain.Entities;

namespace UserService.Application.Handler.Auth;

public class RegisterAdminCommandHandler : IRequestHandler<RegisterAdminCommand, RegisterAdminResponse>
{
	private readonly UserManager<ApplicationUser> _userManager;

	public RegisterAdminCommandHandler(UserManager<ApplicationUser> userManager)
	{
		_userManager = userManager;
	}

	public async Task<RegisterAdminResponse> Handle(RegisterAdminCommand command, CancellationToken ct)
	{
		// 1. Check if user exists
		var existingUser = await _userManager.FindByEmailAsync(command.Email);
		if (existingUser != null)
		{
			throw new Exception("User with this email already exists");
		}

		// 2. Create Admin User
		var user = new ApplicationUser
		{
			Id = Guid.NewGuid(),
			UserName = command.Email,
			Email = command.Email,
			FullName = command.FullName,
			CreatedAt = DateTime.UtcNow,
			IsActive = true,
			EmailConfirmed = true 
		};

		var result = await _userManager.CreateAsync(user, command.Password);

		if (!result.Succeeded)
		{
			var errors = string.Join(", ", result.Errors.Select(e => e.Description));
			throw new Exception($"User creation failed: {errors}");
		}

		// 3. Assign Admin Role
		await _userManager.AddToRoleAsync(user, "Admin");

		// 4. Return Response
		return new RegisterAdminResponse(
			UserId: user.Id,
			Email: user.Email,
			FullName: user.FullName,
			Role: "Admin",
			Message: "Admin registered successfully!",
			CreatedAt: user.CreatedAt
		);
	}
}