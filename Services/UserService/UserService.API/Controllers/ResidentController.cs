using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Resident;
using UserService.Application.DTOS.Requests.Auth;
using UserService.Application.Queries.Dues;
using UserService.Infrastructure.Data;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/resident")]
[Authorize(Roles = "Resident")]
public class ResidentController(IMediator mediator, UserDbContext dbContext) : ControllerBase
{
	private readonly IMediator _mediator = mediator;
	private readonly UserDbContext _dbContext = dbContext;




	[HttpPost("register")]
	[Authorize(Roles = "EstateManager")]
	public async Task<IActionResult> Register([FromBody] ResidentRegistrationRequestDto request)
	{
		var command = new RegisterResidentCommand(
			FirstName: request.FirstName!,
			LastName: request.LastName!,
			Email: request.Email!,
			PhoneNumber: request.PhoneNumber!,
			Password: request.Password!,
			RegisterAs:"Resident",
			PropertyType: request.PropertyType!,
			MeterNumber: request.MeterNumber!,
			HouseAddress: request.HouseAddress!,
			EstateId: request.EstateId
		);

		var result = await _mediator.Send(command);
		return Ok(result);
	}

	[HttpGet("my-dues")]
	public async Task<IActionResult> GetMyDues()
	{
		// Get UserId from JWT token
		var userId = User.FindFirst("userId")?.Value;
		if (string.IsNullOrEmpty(userId))
		{
			return Unauthorized("User not found");
		}

		// Get ResidentId from UserId
		var resident = await _dbContext.ResidentRegistration
			.FirstOrDefaultAsync(r => r.UserId == Guid.Parse(userId));

		if (resident == null)
		{
			return NotFound("Resident not found");
		}

		var query = new GetResidentDuesQuery(resident.Id);
		var result = await _mediator.Send(query);
		return Ok(result);
	}
}