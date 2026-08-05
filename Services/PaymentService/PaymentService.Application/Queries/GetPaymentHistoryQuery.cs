using MediatR;
using PaymentService.Application.DTO.Responses;

namespace PaymentService.Application.Queries;

public record GetPaymentHistoryQuery(
	Guid UserId
) : IRequest<List<PaymentHistoryResponseDto>>;