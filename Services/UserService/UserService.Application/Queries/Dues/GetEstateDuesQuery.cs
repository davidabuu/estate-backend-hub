using MediatR;
using UserService.Application.DTOs.Responses.Dues;


namespace UserService.Application.Queries.Dues;

public record GetEstateDuesQuery(
	Guid EstateId
) : IRequest<List<EstateDueResponse>>;