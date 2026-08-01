using MediatR;
using UserService.Application.DTOs.Responses.Admin;

namespace UserService.Application.Commands.Admin;

public record VerifyEstateCommand(
	Guid EstateId,
	bool IsApproved,
	string? RejectionReason
) : IRequest<VerifyEstateResponse>;