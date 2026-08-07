using MassTransit;
using EstateHub.Contracts.Events;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace PaymentService.Application.Consumers;

public class ResidentDueCreatedConsumer : IConsumer<ResidentDueCreatedEvent>
{
	private readonly PaymentDbContext _dbContext;
	private readonly ILogger<ResidentDueCreatedConsumer> _logger;

	public ResidentDueCreatedConsumer(
		PaymentDbContext dbContext,
		ILogger<ResidentDueCreatedConsumer> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task Consume(ConsumeContext<ResidentDueCreatedEvent> context)
	{
		var @event = context.Message;

		_logger.LogInformation("📥 Received ResidentDueCreatedEvent for ResidentDueId: {ResidentDueId}", @event.ResidentDueId);

		var existing = await _dbContext.ResidentDues
			.FirstOrDefaultAsync(d => d.Id == @event.ResidentDueId, context.CancellationToken);

		if (existing != null)
		{
			_logger.LogWarning("ResidentDue {ResidentDueId} already exists in PaymentService", @event.ResidentDueId);
			return;
		}


		var residentDue = new ResidentDues
		{
			Id = @event.ResidentDueId,
			UserId = @event.UserId,
			EstateId = @event.EstateId,
			Amount = @event.Amount,
			Email = @event.Email,
			DueName = @event.DueName,
			DueDate = @event.DueDate,
			IsPaid = false,
			CreatedAt = DateTime.UtcNow
		};

		await _dbContext.ResidentDues.AddAsync(residentDue, context.CancellationToken);
		await _dbContext.SaveChangesAsync(context.CancellationToken);

		_logger.LogInformation("✅ ResidentDue {ResidentDueId} saved locally in PaymentService", @event.ResidentDueId);
	}
}