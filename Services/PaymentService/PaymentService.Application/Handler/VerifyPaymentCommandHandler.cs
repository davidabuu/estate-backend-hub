using EstateHub.Contracts.Events;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Command;

using PaymentService.Application.DTO.Responses;

using PaymentService.Application.Interface;

using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Application.Handlers;

public class VerifyPaymentCommandHandler : IRequestHandler<VerifyPaymentCommand, VerifyPaymentResponseDto>
{
	private readonly PaymentDbContext _dbContext;
	private readonly IPaystackService _paystackService;
	private readonly IPublishEndpoint _publishEndpoint;
	private readonly ILogger<VerifyPaymentCommandHandler> _logger;

	public VerifyPaymentCommandHandler(
		PaymentDbContext dbContext,
		IPaystackService paystackService,
		IPublishEndpoint publishEndpoint,
		ILogger<VerifyPaymentCommandHandler> logger)
	{
		_dbContext = dbContext;
		_paystackService = paystackService;
		_publishEndpoint = publishEndpoint;
		_logger = logger;
	}

	public async Task<VerifyPaymentResponseDto> Handle(VerifyPaymentCommand command, CancellationToken ct)
	{
		// 1. Get payment
		var payment = await _dbContext.Payments
			.FirstOrDefaultAsync(p => p.Reference == command.Reference, ct);

		if (payment == null)
		{
			throw new Exception("Payment not found");
		}

		// 2. Verify with Paystack
		var response = await _paystackService.VerifyPaymentAsync(command.Reference);

		if (!response.Status)
		{
			throw new Exception(response.Message ?? "Payment verification failed");
		}

		// 3. Update payment status
		var verifyData = response.Data;
		payment.GatewayResponse = verifyData.GatewayResponse;
		payment.TransactionReference = verifyData.Reference;
		payment.AmountPaid = verifyData.AmountPaid ?? 0;
		payment.Fee = verifyData.Fee;
		payment.Channel = verifyData.Channel?.ToLower() switch
		{
			"card" => PaymentChannel.Card,
			"bank" => PaymentChannel.Bank,
			"ussd" => PaymentChannel.USSD,
			"qr" => PaymentChannel.QR,
			"mobile_money" => PaymentChannel.MobileMoney,
			"bank_transfer" => PaymentChannel.BankTransfer,
			_ => payment.Channel
		};

		payment.Status = verifyData.Status?.ToLower() switch
		{
			"success" => PaymentStatus.Success,
			"failed" => PaymentStatus.Failed,
			"abandoned" => PaymentStatus.Abandoned,
			"reversed" => PaymentStatus.Reversed,
			_ => PaymentStatus.Failed
		};

		if (payment.Status == PaymentStatus.Success)
		{
			payment.PaidAt = DateTime.UtcNow;
		}

		payment.UpdatedAt = DateTime.UtcNow;
		await _dbContext.SaveChangesAsync(ct);

		// 4. ✅ If payment was successful, publish event
		if (payment.Status == PaymentStatus.Success)
		{
			await _publishEndpoint.Publish(new PaymentProcessedEvent(
				PaymentId: payment.Id,
				UserId: payment.UserId,
				ResidentDueId: payment.ResidentDueId ?? Guid.Empty,
				Amount: payment.Amount,
				Reference: payment.Reference!,
				ProcessedAt: DateTime.UtcNow
			), ct);

			_logger.LogInformation("📤 Published PaymentProcessedEvent for Payment {PaymentId}", payment.Id);
		}

		return new VerifyPaymentResponseDto(
			PaymentId: payment.Id,
			Reference: payment.Reference!,
			Success: payment.Status == PaymentStatus.Success,
			Status: payment.Status,
			AmountPaid: payment.AmountPaid,
			Fee: payment.Fee,
			GatewayResponse: payment.GatewayResponse,
			Message: payment.Status == PaymentStatus.Success
				? "Payment verified successfully"
				: $"Payment status: {payment.Status}"
		);
	}
}