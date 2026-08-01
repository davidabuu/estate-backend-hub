using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Admin;
using UserService.Application.DTOs.Responses.Admin;
using UserService.Infrastructure.Data;

namespace UserService.Application.Handler.Admin;

public class VerifyEstateCommandHandler : IRequestHandler<VerifyEstateCommand, VerifyEstateResponse>
{
	private readonly UserDbContext _dbContext;

	public VerifyEstateCommandHandler(UserDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<VerifyEstateResponse> Handle(VerifyEstateCommand command, CancellationToken ct)
	{
		var estate = await _dbContext.EstateRegistration
			.Include(e => e.User)
			.FirstOrDefaultAsync(e => e.Id == command.EstateId, ct);

		if (estate == null)
		{
			throw new Exception("Estate not found");
		}

		estate.IsApproved = command.IsApproved;

		if (command.IsApproved)
		{
			estate.ApprovedAt = DateTime.UtcNow;
			
		}
		else
		{
			estate.ApprovedAt = null;
			
		}

		await _dbContext.SaveChangesAsync(ct);

		var message = command.IsApproved
			? $"Estate '{estate.EstateName}' has been approved successfully"
			: $"Estate '{estate.EstateName}' has been rejected";

		return new VerifyEstateResponse(
			EstateId: estate.Id,
			EstateName: estate.EstateName,
			IsApproved: estate.IsApproved,
			Message: message,
			ProcessedAt: DateTime.UtcNow
		);
	}
}