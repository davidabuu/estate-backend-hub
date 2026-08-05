using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.DTO.Responses;
using PaymentService.Application.Queries;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Application.Handler;

public class GetPaymentHistoryQueryHandler : IRequestHandler<GetPaymentHistoryQuery, List<PaymentHistoryResponseDto>>
{
	private readonly PaymentDbContext _dbContext;

	public GetPaymentHistoryQueryHandler(PaymentDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<List<PaymentHistoryResponseDto>> Handle(GetPaymentHistoryQuery query, CancellationToken ct)
	{
		// Get all payments for the user, ordered by most recent first
		var payments = await _dbContext.Payments
			.Where(p => p.UserId == query.UserId && !p.IsDeleted)
			.OrderByDescending(p => p.CreatedAt)
			.ToListAsync(ct);

		// Map to response DTOs
		return payments.Select(p => new PaymentHistoryResponseDto(
			Id: p.Id,
			Reference: p.Reference!,
			Amount: p.Amount,
			Status: p.Status,
			GatewayResponse: p.GatewayResponse,
			CreatedAt: p.CreatedAt,
			PaidAt: p.PaidAt
		)).ToList();
	}
}