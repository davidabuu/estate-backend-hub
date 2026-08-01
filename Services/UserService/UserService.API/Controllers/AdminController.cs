using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Admin;
using UserService.Application.Commands.Auth;
using UserService.Application.DTOs.Requests.Admin;
using UserService.Application.DTOS.Requests.Auth;
using UserService.Application.Queries.Admin;
using UserService.Infrastructure.Data;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(IMediator mediator, UserDbContext dbContext, IWebHostEnvironment env) : ControllerBase
{
	private readonly IMediator _mediator = mediator;
	private readonly UserDbContext _dbContext = dbContext;
	private readonly IWebHostEnvironment _env = env;



	// POST: api/admin/register
	[HttpPost("register")]
	[AllowAnonymous]
	public async Task<IActionResult> Register([FromBody] RegisterAdminRequestDto request)
	{
		var command = new RegisterAdminCommand(
			Email: request.Email!,
			Password: request.Password!,
			FullName: request.FullName!
		);

		var result = await _mediator.Send(command);
		return Ok(result);
	}

	

	

	[HttpGet("estates")]
	public async Task<IActionResult> GetAllEstates()
	{
		var query = new GetAllEstatesQuery();
		var result = await _mediator.Send(query);
		return Ok(result);
	}

	
	[HttpGet("estates/{estateId}")]
	public async Task<IActionResult> GetEstateById(Guid estateId)
	{
		var query = new GetEstateByIdQuery(estateId);
		var result = await _mediator.Send(query);
		return Ok(result);
	}


	[HttpPut("estates/verify")]
	public async Task<IActionResult> VerifyEstate([FromBody] VerifyEstateRequestDto request)
	{
		var command = new VerifyEstateCommand(
			EstateId: request.EstateId,
			IsApproved: request.IsApproved,
			RejectionReason: request.RejectionReason
		);

		var result = await _mediator.Send(command);
		return Ok(result);
	}

	
	[HttpGet("estates/{estateId}/download/{documentType}")]
	public async Task<IActionResult> DownloadDocument(Guid estateId, string documentType)
	{
		var estate = await _dbContext.EstateRegistration
			.FirstOrDefaultAsync(e => e.Id == estateId);

		if (estate == null)
		{
			return NotFound("Estate not found");
		}

		string? fileUrl = documentType.ToLower() switch
		{
			"registration" => estate.EstateRegistrationDocUrl,
			"association" => estate.EstateAssociationRegistrationDocUrl,
			_ => null
		};

		if (string.IsNullOrEmpty(fileUrl))
		{
			return NotFound($"Document '{documentType}' not found for this estate");
		}

		// Get the file path
		var filePath = Path.Combine(_env.ContentRootPath, "Uploads", "Files", fileUrl);

		if (!System.IO.File.Exists(filePath))
		{
			return NotFound("File not found on server");
		}

		// Get file extension and content type
		var extension = Path.GetExtension(filePath);
		var contentType = extension.ToLower() switch
		{
			".pdf" => "application/pdf",
			".jpg" or ".jpeg" => "image/jpeg",
			".png" => "image/png",
			".doc" => "application/msword",
			".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
			_ => "application/octet-stream"
		};

		var fileName = $"{estate.EstateName}_{documentType}{extension}";
		var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

		return File(fileBytes, contentType, fileName);
	}

	
	[HttpDelete("estates")]
	public async Task<IActionResult> DeleteEstate([FromBody] DeleteEstateRequestDto request)
	{
		var command = new DeleteEstateCommand(
			EstateId: request.EstateId
		);

		var result = await _mediator.Send(command);
		return Ok(result);
	}
}