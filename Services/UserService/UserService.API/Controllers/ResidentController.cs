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
[Authorize(Roles = "EstateManager, Resident")]  // ✅ Both roles can access
public class ResidentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserDbContext _dbContext;

    public ResidentController(IMediator mediator, UserDbContext dbContext)
    {
        _mediator = mediator;
        _dbContext = dbContext;
    }

    [HttpPost("register")]
    [Authorize(Roles = "EstateManager")]  // ✅ Only EstateManager can register residents
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

    [HttpGet("my-dues")]
    [Authorize(Roles = "Resident")]  // ✅ Only Residents can view their dues
    public async Task<IActionResult> GetMyDues()
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User not found");
        }

        var resident = await _dbContext.ResidentRegistrations
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
