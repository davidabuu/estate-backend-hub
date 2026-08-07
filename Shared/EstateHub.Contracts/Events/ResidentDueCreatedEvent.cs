namespace EstateHub.Contracts.Events;

public record ResidentDueCreatedEvent(
	Guid ResidentDueId,
	Guid UserId,
	Guid EstateId,
	decimal Amount,
	string Email,
	string DueName,
	DateTime DueDate
);