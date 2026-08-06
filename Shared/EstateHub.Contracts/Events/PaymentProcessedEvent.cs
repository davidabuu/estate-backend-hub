namespace EstateHub.Contracts.Events;

public record PaymentProcessedEvent(
	Guid PaymentId,
	Guid UserId,
	Guid ResidentDueId,
	decimal Amount,
	string Reference,
	DateTime ProcessedAt
);