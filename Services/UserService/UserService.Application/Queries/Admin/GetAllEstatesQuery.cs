using MediatR;
using UserService.Application.DTOs.Responses.Admin;

namespace UserService.Application.Queries.Admin;

public record GetAllEstatesQuery : IRequest<List<AdminEstateListResponse>>;