
using PaymentService.Domain.Enums;
namespace PaymentService.Application.DTO.Responses;

public record VerifyPaymentResponseDto(
	Guid PaymentId,
	string Reference,
	bool Success,
	PaymentStatus Status,
	decimal AmountPaid,
	decimal? Fee,
	string? GatewayResponse,
	string? Message
);