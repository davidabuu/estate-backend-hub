using MediatR;
using UserService.Application.DTOs.Responses.Dues;
using UserService.Application.Enums;
using UserService.Domain.Enums;

namespace UserService.Application.Commands.Dues;

public record CreateDueCommand(
	Guid EstateId,
	string DueName,
	string? Description,
	DueType DueType,
	DateTime DueDate,
	Dictionary<PropertyType, decimal> PropertyTypeAmounts
) : IRequest<CreateDueResponse>;