using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Dues;
using UserService.Application.DTOs.Responses.Dues;
using UserService.Infrastructure.Data;

namespace UserService.Application.Handler.Dues;

public class DeleteDueCommandHandler : IRequestHandler<DeleteDueCommand, DeleteDueResponse>
{
	private readonly UserDbContext _dbContext;

	public DeleteDueCommandHandler(UserDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<DeleteDueResponse> Handle(DeleteDueCommand command, CancellationToken ct)
	{
		// 1. Get the EstateDue
		var estateDue = await _dbContext.EstateDue
			.FirstOrDefaultAsync(e => e.Id == command.EstateDueId, ct);

		if (estateDue == null)
		{
			throw new Exception("Due not found");
		}

		var dueName = estateDue.DueName;

		// 2. Get count of ResidentDues linked to this EstateDue
		var residentDueCount = await _dbContext.ResidentDues
			.CountAsync(r => r.EstateDueId == command.EstateDueId, ct);

		// 3. Delete ALL ResidentDues linked to this EstateDue
		var residentDues = await _dbContext.ResidentDues
			.Where(r => r.EstateDueId == command.EstateDueId)
			.ToListAsync(ct);

		if (residentDues.Any())
		{
			_dbContext.ResidentDues.RemoveRange(residentDues);
		}

		// 4. Delete the EstateDue
		_dbContext.EstateDue.Remove(estateDue);

		await _dbContext.SaveChangesAsync(ct);

		// 5. Return Response
		return new DeleteDueResponse(
			EstateDueId: command.EstateDueId,
			DueName: dueName!,
			ResidentsAffected: residentDueCount,
			Message: $"Due '{dueName}' deleted. {residentDueCount} residents' dues removed.",
			DeletedAt: DateTime.UtcNow
		);
	}
}