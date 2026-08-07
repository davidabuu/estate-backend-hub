using EstateHub.Contracts.Events;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Commands.Dues;
using UserService.Application.DTOs.Responses.Dues;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Application.Handler.Dues;

public class CreateDueCommandHandler : IRequestHandler<CreateDueCommand, CreateDueResponse>
{
	private readonly UserDbContext _dbContext;
	private readonly IPublishEndpoint _publishEndpoint;
	private readonly ILogger<CreateDueCommandHandler> _logger;

	public CreateDueCommandHandler(
		UserDbContext dbContext,
		IPublishEndpoint publishEndpoint,
		ILogger<CreateDueCommandHandler> logger)
	{
		_dbContext = dbContext;
		_publishEndpoint = publishEndpoint;
		_logger = logger;
	}

	public async Task<CreateDueResponse> Handle(CreateDueCommand command, CancellationToken ct)
	{
		// 1. Check if Estate exists
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

		// 4. Get all Active Residents in this Estate (INCLUDE the User so we can get Email)
		var residents = await _dbContext.ResidentRegistration
			.Include(r => r.User)  // ✅ Include User to get email
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

		// 6. Create ResidentDues for EACH Resident
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
				UserId = resident.UserId,            
				EstateId = command.EstateId,        
				DueName = command.DueName,
				Description = command.Description,
				Amount = amount,
				DueType = command.DueType,
				DueDate = command.DueDate,
				PropertyType = resident.PropertyType,
				Status = DueStatus.Pending,
				IsPaid = false,
				
				Email = resident.Email,
			
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

		// 8. ✅ Publish event for each resident due created
		foreach (var residentDue in residentDues)
		{
			await _publishEndpoint.Publish(new ResidentDueCreatedEvent(
				ResidentDueId: residentDue.Id,
				 UserId: residentDue.UserId,
				EstateId: command.EstateId,
				Amount: residentDue.Amount,
				Email: residentDue.Email!, // ✅ Ensure email is always provided
				DueName: residentDue.DueName!,
				DueDate: residentDue.DueDate
			), ct);

			_logger.LogInformation("📤 Published ResidentDueCreatedEvent for {ResidentDueId} (Email: {Email})",
				residentDue.Id, residentDue.Email);
		}

		// 9. Return Response
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