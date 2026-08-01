namespace EstateHub.Contracts.Events;

public record SubscriptionExpiredEvent(
	Guid SubscriptionId,
	Guid UserId,
	string Email,
	DateTime ExpiryDate
);