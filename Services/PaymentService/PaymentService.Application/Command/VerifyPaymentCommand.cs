using MediatR;
using PaymentService.Application.DTO.Responses;

namespace PaymentService.Application.Command;

public record VerifyPaymentCommand(
	string Reference
) : IRequest<VerifyPaymentResponseDto>;