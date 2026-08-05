using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.DTO.Responses;
using PaymentService.Application.Queries.GetPaymentStatus;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Application.Handler;

public class GetPaymentStatusQueryHandler(PaymentDbContext dbContext) : IRequestHandler<GetPaymentStatusQuery, VerifyPaymentResponseDto>
{
	private readonly PaymentDbContext _dbContext = dbContext;

	public async Task<VerifyPaymentResponseDto> Handle(GetPaymentStatusQuery query, CancellationToken ct)
	{
		var payment = await _dbContext.Payments
			.FirstOrDefaultAsync(p => p.Id == query.PaymentId && !p.IsDeleted, ct);

		if (payment == null)
		{
			throw new Exception("Payment not found");
		}

		return new VerifyPaymentResponseDto(
			PaymentId: payment.Id,
			Reference: payment.Reference!,
			Success: payment.Status == PaymentStatus.Success,
			Status: payment.Status,
			AmountPaid: payment.AmountPaid,
			Fee: payment.Fee,
			GatewayResponse: payment.GatewayResponse,
			Message: $"Payment status: {payment.Status}"
		);
	}
}