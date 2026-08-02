using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Commands.Auth;
using UserService.Application.DTOs.Requests.Auth;
using UserService.Application.DTOS.Requests.Auth;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
	private readonly IMediator _mediator;

	public AuthController(IMediator mediator)
	{
		_mediator = mediator;
	}

	// POST: api/auth/login
	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
	{
		var command = new LoginCommand(request.Email!, request.Password!);
		var result = await _mediator.Send(command);
		return Ok(result);
	}

	// POST: api/auth/refresh
	[HttpPost("refresh")]
	public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
	{
		var command = new RefreshTokenCommand(request.RefreshToken);
		var result = await _mediator.Send(command);
		return Ok(result);
	}

	// POST: api/auth/logout
	[HttpPost("logout")]
	[Authorize]
	public async Task<IActionResult> Logout([FromBody] LogoutRequestDto? request)
	{
		var command = new LogoutCommand(request?.RefreshToken);
		var result = await _mediator.Send(command);
		return Ok(result);
	}

	// POST: api/auth/forgot-password
	[HttpPost("forgot-password")]
	public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
	{
		var command = new ForgotPasswordCommand(request.Email);
		var result = await _mediator.Send(command);
		return Ok(result);
	}

	// POST: api/auth/reset-password
	[HttpPost("reset-password")]
	public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
	{
		var command = new ResetPasswordCommand(
			request.Email,
			request.Token,
			request.NewPassword,
			request.ConfirmPassword
		);
		var result = await _mediator.Send(command);
		return Ok(result);
	}
}