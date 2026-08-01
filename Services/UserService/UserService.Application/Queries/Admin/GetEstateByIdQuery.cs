using MediatR;
using UserService.Application.DTOs.Responses.Admin;

namespace UserService.Application.Queries.Admin;

public record GetEstateByIdQuery(
	Guid EstateId
) : IRequest<AdminEstateDetailResponse>;