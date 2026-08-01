using MediatR;
using UserService.Application.DTOs.Responses.Dues;

namespace UserService.Application.Commands.Dues;

public record AssignDueToResidentCommand(
	Guid EstateDueId,
	Guid ResidentId
) : IRequest<AssignDueToResidentResponse>;