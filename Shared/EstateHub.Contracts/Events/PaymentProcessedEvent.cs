using UserService.Application.Enums;

namespace EstateHub.Contracts.Events;

public record PaymentProcessedEvent(
	Guid PaymentId,
	Guid UserId,
	decimal Amount,
	PaymentStatus Status,
	DateTime ProcessedAt
);