namespace EstateHub.Contracts.Events;

public record PaymentFailedEvent(
	Guid UserId,
	string Email,
	decimal Amount,
	string FailureReason,
	DateTime FailedAt
);