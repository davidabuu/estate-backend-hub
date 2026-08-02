using MediatR;
using Microsoft.AspNetCore.Http;
using UserService.Application.DTOS.Responses.Residents;


namespace UserService.Application.Commands.Resident;

public record BulkUploadResidentCommand(
	Guid EstateId,
	IFormFile ExcelFile
) : IRequest<BulkResidentUploadResponse>;