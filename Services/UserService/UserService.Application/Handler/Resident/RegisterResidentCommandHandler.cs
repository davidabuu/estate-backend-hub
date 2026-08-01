using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Resident;
using UserService.Application.DTOS.Responses.Auth;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Application.Handler.Resident;

public class RegisterResidentCommandHandler : IRequestHandler<RegisterResidentCommand, ResidentRegistrationResponse>
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly UserDbContext _dbContext;

	public RegisterResidentCommandHandler(
		UserManager<ApplicationUser> userManager,
		UserDbContext dbContext)
	{
		_userManager = userManager;
		_dbContext = dbContext;
	}

	public async Task<ResidentRegistrationResponse> Handle(RegisterResidentCommand command, CancellationToken ct)
	{
		var existingUser = await _userManager.FindByEmailAsync(command.Email);
		if (existingUser != null)
		{
			throw new Exception("User with this email already exists");
		}

		
		var estate = await _dbContext.EstateRegistration
			.FirstOrDefaultAsync(e => e.Id == command.EstateId, ct);

		if (estate == null)
		{
			throw new Exception("Estate not found");
		}

		if (!estate.IsApproved)
		{
			throw new Exception("Estate is not approved. Please wait for admin approval before adding residents.");
		}

		var user = new ApplicationUser
		{
			Id = Guid.NewGuid(),
			UserName = command.Email,
			Email = command.Email,
			FullName = $"{command.FirstName} {command.LastName}",
			CreatedAt = DateTime.UtcNow
		};

		var result = await _userManager.CreateAsync(user, command.Password);

		if (!result.Succeeded)
		{
			var errors = string.Join(", ", result.Errors.Select(e => e.Description));
			throw new Exception($"User creation failed: {errors}");
		}

		// 4. Assign Role
		await _userManager.AddToRoleAsync(user, "Resident");

		// 5. Create ResidentRegistration
		var resident = new ResidentRegistration
		{
			Id = Guid.NewGuid(),
			UserId = user.Id,
			EstateId = command.EstateId,
			FirstName = command.FirstName,
			LastName = command.LastName,
			Email = command.Email,
			PhoneNumber = command.PhoneNumber,
			HouseType = command.HouseType,
			MeterNumber = command.MeterNumber,
			HouseAddress = command.HouseAddress,
			IsActive = true,
			IsRegistered = true,
			CreatedAt = DateTime.UtcNow
		};

		await _dbContext.ResidentRegistration.AddAsync(resident, ct);
		await _dbContext.SaveChangesAsync(ct);

		// 6. Return Response
		return new ResidentRegistrationResponse(
			Email: user.Email,
			FullName: user.FullName,
			EstateName: estate.EstateName,
			Message: "Resident registered successfully!"
			
		);
	}
}