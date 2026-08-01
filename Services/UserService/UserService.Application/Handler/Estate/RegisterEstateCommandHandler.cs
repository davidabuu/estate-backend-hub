using MediatR;
using Microsoft.AspNetCore.Identity;
using UserService.Application.Commands.Estate;
using UserService.Application.DTOS.Responses.Auth;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Application.Handler.Estate;

public class RegisterEstateCommandHandler(
	UserManager<ApplicationUser> userManager,
	UserDbContext dbContext) : IRequestHandler<RegisterEstateCommand, EstateRegistrationResponse>
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	private readonly UserDbContext _dbContext = dbContext;

	public async Task<EstateRegistrationResponse> Handle(RegisterEstateCommand command, CancellationToken ct)
	{

		var user = new ApplicationUser
		{
			Id = Guid.NewGuid(),
			UserName = command.Email,
			Email = command.Email,
			FullName = command.EstateName,
			CreatedAt = DateTime.UtcNow
		};

		var result = await _userManager.CreateAsync(user, command.Password);

		if (!result.Succeeded)
		{
			var errors = string.Join(", ", result.Errors.Select(e => e.Description));
			throw new Exception($"User creation failed: {errors}");
		}

	
		await _userManager.AddToRoleAsync(user, "EstateManager");

		var estate = new EstateRegistration
		{
			Id = Guid.NewGuid(),
			UserId = user.Id,
			EstateName = command.EstateName,
			EstateAddress = command.EstateAddress,

			PropertyTypes = command.PropertyTypes,
			BankName = command.BankName,
			AccountName = command.AccountName,
			AccountNumber = command.AccountNumber,
			BankCode = command.BankCode,
			EstateRegistrationDocUrl = command.EstateRegistrationDocUrl,
			EstateAssociationRegistrationDocUrl = command.EstateAssociationRegistrationDocUrl,
			CreatedAt = DateTime.UtcNow
		};

		await _dbContext.EstateRegistration.AddAsync(estate, ct);
		await _dbContext.SaveChangesAsync(ct);


		return new EstateRegistrationResponse(
			UserId: user.Id,
			EstateId: estate.Id,
			Email: user.Email,
			EstateName: estate.EstateName,
			Message: "Estate registration successful! Awaiting admin approval.",
			IsApproved: false,
		DateRegistered: estate.CreatedAt
		);
	}
}