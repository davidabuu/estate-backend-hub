using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Command;
using PaymentService.Application.DTO.Responses;
using PaymentService.Application.Interface;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Application.Handler;

public class InitializePaymentCommandHandler : IRequestHandler<InitializePaymentCommand, InitializePaymentResponseDto>
{
	private readonly PaymentDbContext _dbContext;
	private readonly IPaystackService _paystackService;
	private readonly IIdempotencyService _idempotencyService;
	private readonly ILogger<InitializePaymentCommandHandler> _logger;

	public InitializePaymentCommandHandler(
		PaymentDbContext dbContext,
		IPaystackService paystackService,
		IIdempotencyService idempotencyService,
		ILogger<InitializePaymentCommandHandler> logger)
	{
		_dbContext = dbContext;
		_paystackService = paystackService;
		_idempotencyService = idempotencyService;
		_logger = logger;
	}

	public async Task<InitializePaymentResponseDto> Handle(InitializePaymentCommand command, CancellationToken ct)
	{
		// 1. Check idempotency
		if (await _idempotencyService.IsProcessedAsync(command.IdempotencyKey, ct))
		{
			_logger.LogInformation("Idempotent request detected: {Key}", command.IdempotencyKey);
			var existingResponse = await _idempotencyService.GetResponseAsync(command.IdempotencyKey, ct);

			if (existingResponse != null)
			{
				return System.Text.Json.JsonSerializer.Deserialize<InitializePaymentResponseDto>(existingResponse)!;
			}

			throw new Exception("Idempotent request already processed but response not found");
		}

		// 2. Get the resident due
		var residentDue = await _dbContext.ResidentDues
			.FirstOrDefaultAsync(d => d.Id == command.ResidentDueId, ct);

		if (residentDue == null)
		{
			throw new Exception("Resident due not found");
		}

		if (residentDue.IsPaid)
		{
			throw new Exception("This due has already been paid");
		}

		// 3. Generate reference and create payment record
		var reference = GenerateReference();
		var payment = new Payment
		{
			Id = Guid.NewGuid(),
			UserId = command.UserId,
			ResidentDueId = command.ResidentDueId,
			Reference = reference,
			Amount = residentDue.Amount,
			Status = PaymentStatus.Pending,
			CustomerEmail = residentDue.Email,
			IdempotencyKey = command.IdempotencyKey,
			CreatedAt = DateTime.UtcNow
		};

		await _dbContext.Payments.AddAsync(payment, ct);
		await _dbContext.SaveChangesAsync(ct);

		// 4. Initialize Paystack payment
		var response = await _paystackService.InitializePaymentAsync(
			residentDue.Email!,
			residentDue.Amount,
			reference
		);

		if (!response.Status || response.Data == null)
		{
			payment.Status = PaymentStatus.Failed;
			payment.GatewayResponse = response.Message;
			await _dbContext.SaveChangesAsync(ct);

			throw new Exception(response.Message ?? "Failed to initialize payment");
		}

		// 5. Update payment with Paystack data
		payment.AccessCode = response.Data.AccessCode;
		payment.AuthorizationUrl = response.Data.AuthorizationUrl;
		payment.Status = PaymentStatus.Initiated;
		payment.InitiatedAt = DateTime.UtcNow;
		payment.UpdatedAt = DateTime.UtcNow;

		await _dbContext.SaveChangesAsync(ct);

		// 6. Create response
		var result = new InitializePaymentResponseDto(
			Reference: reference,
			AuthorizationUrl: response.Data.AuthorizationUrl!,
			AccessCode: response.Data.AccessCode!,
			PaymentId: payment.Id,
			Message: "Payment initialized successfully"
		);

		// 7. Store idempotency
		await _idempotencyService.MarkAsProcessedAsync(
			command.IdempotencyKey,
			System.Text.Json.JsonSerializer.Serialize(result),
			ct
		);

		return result;
	}

	private string GenerateReference()
	{
		return $"EST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 20).ToUpper();
	}
}