using MediatR;
using UserService.Application.DTOs.Responses.Dues;

namespace UserService.Application.Commands.Dues;

public record DeleteDueCommand(
	Guid EstateDueId
) : IRequest<DeleteDueResponse>;