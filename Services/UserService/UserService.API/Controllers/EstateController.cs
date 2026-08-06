using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Dues;
using UserService.Application.Commands.Estate;
using UserService.Application.Commands.Resident;
using UserService.Application.DTOs.Requests.Resident;
using UserService.Application.DTOS.Requests.Auth;
using UserService.Application.DTOS.Requests.EstateDues;
using UserService.Application.Enums;
using UserService.Application.Queries.Dues;
using UserService.Infrastructure.Data;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/estate")]
public class EstateController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly IWebHostEnvironment _env;

	private readonly UserDbContext _dbContext;  

	public EstateController(
		IMediator mediator,
		IWebHostEnvironment env,
		UserDbContext dbContext)  
	{
		_mediator = mediator;
		_env = env;
		_dbContext = dbContext;
	}
	private async Task<string> WriteFile(IFormFile file)
	{
		string filename = "";
		try
		{
			var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
			filename = DateTime.Now.Ticks.ToString() + extension;

			var filepath = Path.Combine(_env.ContentRootPath, "Uploads", "Files");

			if (!Directory.Exists(filepath))
			{
				Directory.CreateDirectory(filepath);
			}

			var exactpath = Path.Combine(filepath, filename);
			using (var stream = new FileStream(exactpath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			return filename;
		}
		catch (Exception ex)
		{
			// Log error
			return filename;
		}
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register(
		[FromForm] EstateRegistrationRequestDto request,
		IFormFile estateRegistrationDoc,
		IFormFile estateAssociationRegistrationDoc)
	{
		
		var estateRegistrationDocUrl = await WriteFile(estateRegistrationDoc);
		var estateAssociationDocUrl = await WriteFile(estateAssociationRegistrationDoc);

	
		var command = new RegisterEstateCommand(
			
			Email: request.Email!,
			Password: request.Password!,
			EstateName: request.EstateName!,
			EstateAddress: request.EstateAddress!,
		
			PropertyTypes: request.PropertyTypes ?? new List<PropertyType>(),
			BankName: request.BankName!,
			AccountName: request.AccountName!,
			AccountNumber: request.AccountNumber!,
			BankCode: request.BankCode!,
			EstateRegistrationDocUrl: estateRegistrationDocUrl,
			EstateAssociationRegistrationDocUrl: estateAssociationDocUrl
		);

		var result = await _mediator.Send(command);
		return Ok(result);
	}
	[HttpPost("upload-residents")]
	[Authorize(Roles = "EstateManager")]
	public async Task<IActionResult> UploadResidents(
	 [FromForm] BulkResidentUploadRequestDto request)
	{
		var command = new BulkUploadResidentCommand(
			EstateId: request.EstateId,
			ExcelFile: request.ExcelFile
		);

		var result = await _mediator.Send(command);
		return Ok(result);
	}
	[HttpPost("register-single")]
	[Authorize(Roles = "EstateManager")]
	public async Task<IActionResult> Register([FromBody] ResidentRegistrationRequestDto request)
	{
		var command = new RegisterResidentCommand(
			FirstName: request.FirstName!,
			LastName: request.LastName!,
			Email: request.Email!,
			PhoneNumber: request.PhoneNumber!,
			Password: request.Password!,
			RegisterAs: "Resident",
			PropertyType: request.PropertyType!,
			MeterNumber: request.MeterNumber!,
			HouseAddress: request.HouseAddress!,
			EstateId: request.EstateId
		);

		var result = await _mediator.Send(command);
		return Ok(result);
	}
	[HttpPost("create-due")]
	[Authorize(Roles = "EstateManager")]
	public async Task<IActionResult> CreateDue([FromBody] CreateDueRequestDto request)
	{
		var command = new CreateDueCommand(
			EstateId: request.EstateId,
			DueName: request.DueName,
			Description: request.Description,
			DueType: request.DueType,
			DueDate: request.DueDate,
			PropertyTypeAmounts: request.PropertyTypeAmounts
		);

		var result = await _mediator.Send(command);
		return Ok(result);
	}
	[HttpPost("assign-due-to-resident")]
	[Authorize(Roles = "EstateManager")]
	public async Task<IActionResult> AssignDueToResident([FromBody] AssignDueToResidentRequestDto request)
	{
		var command = new AssignDueToResidentCommand(
			EstateDueId: request.EstateDueId,
			ResidentId: request.ResidentId
		);

		var result = await _mediator.Send(command);
		return Ok(result);
	}


	[HttpPut("update-due")]
	[Authorize(Roles = "EstateManager")]
	public async Task<IActionResult> UpdateDue([FromBody] UpdateDueRequestDto request)
	{
		var command = new UpdateDueCommand(
			EstateDueId: request.EstateDueId,
			DueName: request.DueName,
			Description: request.Description,
			DueType: request.DueType,
			DueDate: request.DueDate,
			PropertyTypeAmounts: request.PropertyTypeAmounts,
			IsActive: request.IsActive
		);

		var result = await _mediator.Send(command);
		return Ok(result);
	}


	[HttpDelete("delete-due")]
	[Authorize(Roles = "EstateManager")]
	public async Task<IActionResult> DeleteDue([FromBody] DeleteDueRequestDto request)
	{
		var command = new DeleteDueCommand(
			EstateDueId: request.EstateDueId
		);

		var result = await _mediator.Send(command);
		return Ok(result);
	}
	
	[HttpGet("my-dues")]
	[Authorize(Roles = "EstateManager")]
	public async Task<IActionResult> GetMyEstateDues()
	{
		
		var userId = User.FindFirst("userId")?.Value;
		if (string.IsNullOrEmpty(userId))
		{
			return Unauthorized("User not found");
		}

		var estate = await _dbContext.EstateRegistration
			.FirstOrDefaultAsync(e => e.UserId == Guid.Parse(userId));

		if (estate == null)
		{
			return NotFound("Estate not found");
		}

		var query = new GetEstateDuesQuery(estate.Id);
		var result = await _mediator.Send(query);
		return Ok(result);
	}
}