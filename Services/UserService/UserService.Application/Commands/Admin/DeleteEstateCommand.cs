using MediatR;
using UserService.Application.DTOs.Responses.Admin;

namespace UserService.Application.Commands.Admin;

public record DeleteEstateCommand(
	Guid EstateId
) : IRequest<DeleteEstateResponse>;