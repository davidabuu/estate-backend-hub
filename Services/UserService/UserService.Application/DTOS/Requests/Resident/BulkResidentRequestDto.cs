using Microsoft.AspNetCore.Http;

namespace UserService.Application.DTOs.Requests.Resident;

public class BulkResidentUploadRequestDto
{
	public Guid EstateId { get; set; }
	public IFormFile ExcelFile { get; set; } = null!;
}