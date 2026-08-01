namespace UserService.Application.DTOS.Requests.EstateDues;

public record AssignDueToResidentRequestDto(
	Guid EstateDueId,
	Guid ResidentId
);