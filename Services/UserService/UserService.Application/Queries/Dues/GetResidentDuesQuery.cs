using MediatR;
using UserService.Application.DTOs.Responses.Dues;

namespace UserService.Application.Queries.Dues;

public record GetResidentDuesQuery(
	Guid ResidentId
) : IRequest<List<ResidentDueResponse>>;