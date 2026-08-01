namespace EstateHub.Contracts.Events;

public record ResidentAddedEvent(
	Guid ResidentId,
	string Email,
	string FullName,
	Guid EstateId,
	DateTime AddedAt
);