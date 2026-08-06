namespace EstateHub.Contracts.Events;

public record ResidentDueCreatedEvent(
	Guid ResidentDueId,
	Guid UserId,
	Guid EstateId,
	decimal Amount,
	string DueName,
	DateTime DueDate
);