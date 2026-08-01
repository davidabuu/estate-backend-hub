using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Dues;
using UserService.Application.DTOs.Responses.Dues;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Application.Handler.Dues;

public class AssignDueToResidentCommandHandler : IRequestHandler<AssignDueToResidentCommand, AssignDueToResidentResponse>
{
	private readonly UserDbContext _dbContext;

	public AssignDueToResidentCommandHandler(UserDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<AssignDueToResidentResponse> Handle(AssignDueToResidentCommand command, CancellationToken ct)
	{
		// 1. Get the EstateDue
		var estateDue = await _dbContext.EstateDue
			.FirstOrDefaultAsync(e => e.Id == command.EstateDueId, ct);

		if (estateDue == null)
		{
			throw new Exception("Due not found");
		}

		// 2. Get the Resident
		var resident = await _dbContext.ResidentRegistration
			.FirstOrDefaultAsync(r => r.Id == command.ResidentId, ct);

		if (resident == null)
		{
			throw new Exception("Resident not found");
		}

		// 3. Check if EstateDue belongs to the same estate as Resident
		if (estateDue.EstateId != resident.EstateId)
		{
			throw new Exception("This due does not belong to the resident's estate");
		}

		// 4. Check if Resident already has this due assigned
		var existingResidentDue = await _dbContext.ResidentDues
			.FirstOrDefaultAsync(r => r.ResidentId == command.ResidentId
				&& r.EstateDueId == command.EstateDueId, ct);

		if (existingResidentDue != null)
		{
			throw new Exception("This due is already assigned to this resident");
		}

		// 5. Get amount based on resident's PropertyType
		var propertyType = resident.PropertyType;
		if (!estateDue.PropertyTypeAmounts.TryGetValue(propertyType, out var amount))
		{
			throw new Exception($"No amount set for property type: {propertyType}");
		}

		// 6. Create ResidentDue
		var residentDue = new ResidentDues
		{
			Id = Guid.NewGuid(),
			ResidentId = resident.Id,
			EstateDueId = estateDue.Id,
			DueName = estateDue.DueName,
			Description = estateDue.Description,
			Amount = amount,
			DueType = estateDue.DueType,
			DueDate = estateDue.DueDate,
			PropertyType = resident.PropertyType,
			Status = DueStatus.Pending,
			IsPaid = false,
			CreatedAt = DateTime.UtcNow
		};

		await _dbContext.ResidentDues.AddAsync(residentDue, ct);
		await _dbContext.SaveChangesAsync(ct);

		// 7. Return Response
		return new AssignDueToResidentResponse(
			ResidentDueId: residentDue.Id,
			DueName: estateDue.DueName,
			ResidentName: $"{resident.FirstName} {resident.LastName}",
			Amount: amount,
			Message: $"Due '{estateDue.DueName}' assigned to {resident.FirstName} {resident.LastName} successfully",
			AssignedAt: residentDue.CreatedAt
		);
	}
}