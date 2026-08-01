using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Dues;
using UserService.Application.DTOs.Responses.Dues;
using UserService.Infrastructure.Data;

namespace UserService.Application.Handler.Dues;

public class UpdateDueCommandHandler : IRequestHandler<UpdateDueCommand, UpdateDueResponse>
{
	private readonly UserDbContext _dbContext;

	public UpdateDueCommandHandler(UserDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<UpdateDueResponse> Handle(UpdateDueCommand command, CancellationToken ct)
	{
		
		var estateDue = await _dbContext.EstateDue
			.FirstOrDefaultAsync(e => e.Id == command.EstateDueId, ct);

		if (estateDue == null)
		{
			throw new Exception("Due not found");
		}

		
		var amountsChanged = false;
		var oldAmounts = estateDue.PropertyTypeAmounts;


		if (!string.IsNullOrEmpty(command.DueName))
		{
			estateDue.DueName = command.DueName;
		}

		if (command.Description != null)
		{
			estateDue.Description = command.Description;
		}

		if (command.DueType.HasValue)
		{
			estateDue.DueType = command.DueType.Value;
		}

		if (command.DueDate.HasValue)
		{
			estateDue.DueDate = command.DueDate.Value;
		}

		if (command.PropertyTypeAmounts != null && command.PropertyTypeAmounts.Count > 0)
		{
			estateDue.PropertyTypeAmounts = command.PropertyTypeAmounts;
			amountsChanged = true;
		}

		if (command.IsActive.HasValue)
		{
			estateDue.IsActive = command.IsActive.Value;
		}

		estateDue.UpdatedAt = DateTime.UtcNow;

		await _dbContext.SaveChangesAsync(ct);

		// 4. Update ALL ResidentDues linked to this EstateDue
		var residentDues = await _dbContext.ResidentDues
			.Where(r => r.EstateDueId == command.EstateDueId)
			.ToListAsync(ct);

		var updateCount = 0;

		foreach (var residentDue in residentDues)
		{
			bool shouldUpdate = false;

			// Update basic fields
			if (!string.IsNullOrEmpty(command.DueName))
			{
				residentDue.DueName = command.DueName;
				shouldUpdate = true;
			}

			if (command.Description != null)
			{
				residentDue.Description = command.Description;
				shouldUpdate = true;
			}

			if (command.DueType.HasValue)
			{
				residentDue.DueType = command.DueType.Value;
				shouldUpdate = true;
			}

			if (command.DueDate.HasValue)
			{
				residentDue.DueDate = command.DueDate.Value;
				shouldUpdate = true;
			}

			// Update amount if PropertyTypeAmounts changed
			if (amountsChanged && command.PropertyTypeAmounts != null)
			{
				var propertyType = residentDue.PropertyType;
				if (command.PropertyTypeAmounts.TryGetValue(propertyType, out var newAmount))
				{
					residentDue.Amount = newAmount;
					shouldUpdate = true;
				}
			}

			if (shouldUpdate)
			{
				residentDue.UpdatedAt = DateTime.UtcNow;
				updateCount++;
			}
		}

		if (updateCount > 0)
		{
			await _dbContext.SaveChangesAsync(ct);
		}

		// 5. Return Response
		return new UpdateDueResponse(
			EstateDueId: estateDue.Id,
			DueName: estateDue.DueName!,
			ResidentsUpdated: updateCount,
			Message: $"Due '{estateDue.DueName}' updated. {updateCount} residents' dues updated.",
			UpdatedAt: estateDue.UpdatedAt.Value
		);
	}
}