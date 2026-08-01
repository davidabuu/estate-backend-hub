using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Dues;
using UserService.Application.DTOs.Responses.Dues;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Application.Handlers.Dues;

public class CreateDueCommandHandler(UserDbContext dbContext) : IRequestHandler<CreateDueCommand, CreateDueResponse>
{
	private readonly UserDbContext _dbContext = dbContext;

	public async Task<CreateDueResponse> Handle(CreateDueCommand command, CancellationToken ct)
	{
		
		var estate = await _dbContext.EstateRegistration
			.FirstOrDefaultAsync(e => e.Id == command.EstateId, ct);

		if (estate == null)
		{
			throw new Exception("Estate not found");
		}

		// 2. Check if Estate is Approved
		if (!estate.IsApproved)
		{
			throw new Exception("Estate is not approved. Cannot create dues.");
		}

		// 3. Validate PropertyTypeAmounts
		if (command.PropertyTypeAmounts == null || command.PropertyTypeAmounts.Count == 0)
		{
			throw new Exception("Please specify amounts for at least one property type");
		}

		// 4. Get all Active Residents in this Estat
		var residents = await _dbContext.ResidentRegistration
			.Where(r => r.EstateId == command.EstateId && r.IsActive)
			.ToListAsync(ct);

		if (residents.Count == 0)
		{
			throw new Exception("No active residents found in this estate to assign dues");
		}

		// 5. Create EstateDue (Master due)
		var estateDue = new EstateDue
		{
			Id = Guid.NewGuid(),
			EstateId = command.EstateId,
			DueName = command.DueName,
			Description = command.Description,
			DueType = command.DueType,
			DueDate = command.DueDate,
			PropertyTypeAmounts = command.PropertyTypeAmounts,
			IsActive = true,
			CreatedAt = DateTime.UtcNow
		};

		await _dbContext.EstateDue.AddAsync(estateDue, ct);
		await _dbContext.SaveChangesAsync(ct);

	
		var residentDues = new List<ResidentDues>();
		var residentsWithoutAmount = new List<string>();

		foreach (var resident in residents)
		{
		
			var propertyType = resident.PropertyType;

			if (!command.PropertyTypeAmounts.TryGetValue(propertyType, out var amount))
			{
				residentsWithoutAmount.Add($"{resident.FirstName} ({propertyType})");
				continue;
			}

			var residentDue = new ResidentDues
			{
				Id = Guid.NewGuid(),
				ResidentId = resident.Id,
				EstateDueId = estateDue.Id,
				DueName = command.DueName,
				Description = command.Description,
				Amount = amount,
				DueType = command.DueType,
				DueDate = command.DueDate,
				PropertyType = resident.PropertyType, 
				Status = DueStatus.Pending,
				IsPaid = false,
				CreatedAt = DateTime.UtcNow
			};

			residentDues.Add(residentDue);
		}

		// 7. Save all ResidentDues
		if (residentDues.Count > 0)
		{
			await _dbContext.ResidentDues.AddRangeAsync(residentDues, ct);
			await _dbContext.SaveChangesAsync(ct);
		}

		// 8. Return Response
		var warningMessage = residentsWithoutAmount.Count > 0
			? $"Warning: {residentsWithoutAmount.Count} residents have no amount set for their property type: {string.Join(", ", residentsWithoutAmount)}"
			: null;

		return new CreateDueResponse(
			EstateDueId: estateDue.Id,
			DueName: command.DueName,
			ResidentsAssigned: residentDues.Count,
			ResidentsWithoutAmount: residentsWithoutAmount,
			Message: $"Due '{command.DueName}' created and assigned to {residentDues.Count} residents",
			Warning: warningMessage,
			CreatedAt: estateDue.CreatedAt
		);
	}
}