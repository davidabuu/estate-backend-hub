using MassTransit;
using Microsoft.EntityFrameworkCore;
using EstateHub.Contracts.Events;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace UserService.Application.Consumers;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
	private readonly UserDbContext _dbContext;
	private readonly ILogger<PaymentProcessedConsumer> _logger;

	public PaymentProcessedConsumer(
		UserDbContext dbContext,
		ILogger<PaymentProcessedConsumer> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
	{
		var @event = context.Message;

		_logger.LogInformation("📥 Received PaymentProcessedEvent for Payment {PaymentId}", @event.PaymentId);

		// ✅ Find the resident due in UserService database
		var residentDue = await _dbContext.ResidentDues
			.FirstOrDefaultAsync(d => d.Id == @event.ResidentDueId, context.CancellationToken);

		if (residentDue == null)
		{
			_logger.LogWarning("ResidentDue {ResidentDueId} not found in UserService", @event.ResidentDueId);
			return;
		}

		// ✅ Mark as paid
		residentDue.IsPaid = true;
		residentDue.PaidAt = @event.ProcessedAt;
		residentDue.PaymentReference = @event.Reference;
		residentDue.Status = DueStatus.Paid;
		residentDue.UpdatedAt = DateTime.UtcNow;

		await _dbContext.SaveChangesAsync(context.CancellationToken);

		_logger.LogInformation("✅ ResidentDue {ResidentDueId} marked as paid in UserService", @event.ResidentDueId);
	}
}