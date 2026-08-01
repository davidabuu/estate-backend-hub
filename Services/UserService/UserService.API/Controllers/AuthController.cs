using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Commands.Auth;
using UserService.Application.DTOS.Requests.Auth;


namespace UserService.API.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController(IMediator mediator) : ControllerBase
{
	private readonly IMediator _mediator = mediator;

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
	{
		var command = new LoginCommand(
			Email: request.Email!,
			Password: request.Password!
		);

		var result = await _mediator.Send(command);
		return Ok(result);
	}
}