

using PaymentService.Domain.Enums;

namespace EstateHub.Contracts.Events;

public record PaymentProcessedEvent(
	Guid PaymentId,
	Guid UserId,
	decimal Amount,
	PaymentStatus Status,
	DateTime ProcessedAt
);