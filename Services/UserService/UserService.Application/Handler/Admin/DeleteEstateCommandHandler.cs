using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Admin;
using UserService.Application.DTOs.Responses.Admin;
using UserService.Infrastructure.Data;

namespace UserService.Application.Handler.Admin;

public class DeleteEstateCommandHandler : IRequestHandler<DeleteEstateCommand, DeleteEstateResponse>
{
	private readonly UserDbContext _dbContext;

	public DeleteEstateCommandHandler(UserDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<DeleteEstateResponse> Handle(DeleteEstateCommand command, CancellationToken ct)
	{
		// 1. Get Estate
		var estate = await _dbContext.EstateRegistration
			.FirstOrDefaultAsync(e => e.Id == command.EstateId, ct);

		if (estate == null)
		{
			throw new Exception("Estate not found");
		}

		var estateName = estate.EstateName;
		var estateId = estate.Id;

		// 2. Get ALL Residents in the Estate
		var residents = await _dbContext.ResidentRegistration
			.Where(r => r.EstateId == estateId)
			.ToListAsync(ct);

		var residentIds = residents.Select(r => r.Id).ToList();
		var residentCount = residentIds.Count;

		// 3. Get ALL ResidentDues linked to these Residents
		var residentDues = await _dbContext.ResidentDues
			.Where(rd => residentIds.Contains(rd.ResidentId))
			.ToListAsync(ct);

		var residentDuesCount = residentDues.Count;

		// 4. Get ALL EstateDues for this Estate
		var estateDues = await _dbContext.EstateDue
			.Where(e => e.EstateId == estateId)
			.ToListAsync(ct);

		var estateDuesCount = estateDues.Count;

		// 5. Delete in order (child first, parent last)
		// Delete ResidentDues
		if (residentDues.Any())
		{
			_dbContext.ResidentDues.RemoveRange(residentDues);
		}

		// Delete Residents
		if (residents.Any())
		{
			_dbContext.ResidentRegistration.RemoveRange(residents);
		}

		// Delete EstateDues
		if (estateDues.Any())
		{
			_dbContext.EstateDue.RemoveRange(estateDues);
		}

		// Delete Estate
		_dbContext.EstateRegistration.Remove(estate);

		// 6. Save Changes
		await _dbContext.SaveChangesAsync(ct);

		// 7. Return Response
		return new DeleteEstateResponse(
			EstateId: estateId,
			EstateName: estateName,
			ResidentsDeleted: residentCount,
			DuesDeleted: estateDuesCount + residentDuesCount,
			Message: $"Estate '{estateName}' deleted successfully. {residentCount} residents and {estateDuesCount + residentDuesCount} dues removed.",
			DeletedAt: DateTime.UtcNow
		);
	}
}