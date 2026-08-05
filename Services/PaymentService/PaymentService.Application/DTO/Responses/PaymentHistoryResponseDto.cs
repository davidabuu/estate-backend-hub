using PaymentService.Domain.Enums;

namespace PaymentService.Application.DTO.Responses;

public record PaymentHistoryResponseDto(
	Guid Id,
	string Reference,
	decimal Amount,
	PaymentStatus Status,
	string? GatewayResponse,
	DateTime CreatedAt,
	DateTime? PaidAt
);