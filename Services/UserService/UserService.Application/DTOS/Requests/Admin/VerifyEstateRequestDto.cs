namespace UserService.Application.DTOs.Requests.Admin;

public record VerifyEstateRequestDto(
	Guid EstateId,
	bool IsApproved,
	string? RejectionReason
);