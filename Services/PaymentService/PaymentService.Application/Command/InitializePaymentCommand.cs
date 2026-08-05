using MediatR;
using PaymentService.Application.DTO.Responses;

namespace PaymentService.Application.Command;

public record InitializePaymentCommand(
	Guid UserId,
	Guid ResidentDueId,
	string IdempotencyKey
) : IRequest<InitializePaymentResponseDto>;