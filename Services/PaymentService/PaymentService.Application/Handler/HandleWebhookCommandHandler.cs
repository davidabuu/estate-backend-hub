using MediatR;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PaymentService.Application.Command;
using PaymentService.Application.Interface;
using Microsoft.Extensions.Logging;

namespace PaymentService.Application.Handler;

public class HandleWebhookCommandHandler(
	PaymentDbContext dbContext,
	IPaystackService paystackService,
	ILogger<HandleWebhookCommandHandler> logger) : IRequestHandler<HandleWebhookCommand, bool>
{
	private readonly PaymentDbContext _dbContext = dbContext;
	private readonly IPaystackService _paystackService = paystackService;
	private readonly ILogger<HandleWebhookCommandHandler> _logger = logger;

	public async Task<bool> Handle(HandleWebhookCommand command, CancellationToken ct)
	{
		// 1. Verify webhook signature
		var isValid = await _paystackService.VerifyWebhookSignatureAsync(command.Payload, command.Signature);
		if (!isValid)
		{
			_logger.LogWarning("Invalid webhook signature");
			return false;
		}

		// 2. Parse webhook event
		using var doc = JsonDocument.Parse(command.Payload);
		var root = doc.RootElement;

		var eventType = root.GetProperty("event").GetString();
		var data = root.GetProperty("data");

		if (eventType == "charge.success")
		{
			var reference = data.GetProperty("reference").GetString();
			var status = data.GetProperty("status").GetString();

			// 3. Update payment
			var payment = await _dbContext.Payments
				.FirstOrDefaultAsync(p => p.Reference == reference, ct);

			if (payment == null)
			{
				_logger.LogWarning("Payment not found for reference: {Reference}", reference);
				return false;
			}

			// Update payment status
			payment.Status = status?.ToLower() switch
			{
				"success" => PaymentStatus.Success,
				"failed" => PaymentStatus.Failed,
				"abandoned" => PaymentStatus.Abandoned,
				_ => payment.Status
			};

			if (payment.Status == PaymentStatus.Success)
			{
				payment.PaidAt = DateTime.UtcNow;
				payment.AmountPaid = data.GetProperty("amount").GetDecimal() / 100;
			}

			payment.WebhookProcessed = true;
			payment.WebhookProcessedAt = DateTime.UtcNow;
			payment.WebhookAttempts++;
			payment.UpdatedAt = DateTime.UtcNow;

			await _dbContext.SaveChangesAsync(ct);

			_logger.LogInformation("Webhook processed for payment {Reference}: {Status}", reference, payment.Status);

			// TODO: Publish PaymentProcessedEvent to Service Bus
			// await _publishEndpoint.Publish(new PaymentProcessedEvent(...));
		}

		return true;
	}
}