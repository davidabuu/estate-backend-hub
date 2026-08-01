

using UserService.Application.Enums;

namespace EstateHub.Contracts.Events
{
	public record SubscriptionCreatedEvent(
		Guid SubscriptionId,
		Guid UserId,
		SubscriptionPlan Plan,
		DateTime StartDate,
		DateTime EndDate
	);
}