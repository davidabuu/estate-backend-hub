using MediatR;
using PaymentService.Application.DTO.Responses;

namespace PaymentService.Application.Queries.GetPaymentStatus;

public record GetPaymentStatusQuery(
	Guid PaymentId
) : IRequest<VerifyPaymentResponseDto>;