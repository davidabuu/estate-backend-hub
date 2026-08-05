using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Command;
using PaymentService.Application.DTO.Requests;
using PaymentService.Application.Queries;
using PaymentService.Application.Queries.GetPaymentStatus;
using System.Security.Claims;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ILogger<PaymentController> _logger;

	public PaymentController(IMediator mediator, ILogger<PaymentController> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}

	/// <summary>
	/// Initialize a payment for a resident due (Residents only)
	/// </summary>
	[HttpPost("initialize")]
	[Authorize(Roles = "Resident")]  // ✅ Only Residents can pay
	public async Task<IActionResult> InitializePayment([FromBody] InitializePaymentRequestDto request)
	{
		// Get UserId from JWT token
		var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
		if (userIdClaim == null)
		{
			return Unauthorized(new { message = "User not authenticated" });
		}

		// ✅ Verify user is a Resident
		var roleClaim = User.FindFirst(ClaimTypes.Role);
		if (roleClaim == null || roleClaim.Value != "Resident")
		{
			return Forbid("Only residents can make payments");
		}

		var userId = Guid.Parse(userIdClaim.Value);

		// Generate idempotency key
		var idempotencyKey = $"{userId}:{request.ResidentDueId}:{DateTime.UtcNow:yyyyMMddHH}";

		var command = new InitializePaymentCommand(
			UserId: userId,
			ResidentDueId: request.ResidentDueId,
			IdempotencyKey: idempotencyKey
		);

		var result = await _mediator.Send(command);

		return Ok(result);
	}

	/// <summary>
	/// Verify a payment after user returns from Paystack (Residents only)
	/// </summary>
	[HttpGet("verify")]
	[Authorize(Roles = "Resident")]  // ✅ Only Residents can verify their payments
	public async Task<IActionResult> VerifyPayment([FromQuery] string reference)
	{
		if (string.IsNullOrEmpty(reference))
		{
			return BadRequest(new { message = "Reference is required" });
		}

		var command = new VerifyPaymentCommand(reference);
		var result = await _mediator.Send(command);

		return Ok(result);
	}

	/// <summary>
	/// Get payment history for the authenticated user
	/// </summary>
	[HttpGet("history")]
	public async Task<IActionResult> GetPaymentHistory()
	{
		var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
		if (userIdClaim == null)
		{
			return Unauthorized(new { message = "User not authenticated" });
		}

		var userId = Guid.Parse(userIdClaim.Value);

		var query = new GetPaymentHistoryQuery(userId);
		var result = await _mediator.Send(query);

		return Ok(result);
	}

	/// <summary>
	/// Get status of a specific payment
	/// </summary>
	[HttpGet("status/{paymentId}")]
	public async Task<IActionResult> GetPaymentStatus(Guid paymentId)
	{
		var query = new GetPaymentStatusQuery(paymentId);
		var result = await _mediator.Send(query);

		return Ok(result);
	}

	/// <summary>
	/// Paystack webhook endpoint (called by Paystack - Public)
	/// </summary>
	[HttpPost("webhook")]
	[AllowAnonymous]  // ✅ Webhook is public (Paystack calls it)
	public async Task<IActionResult> HandleWebhook()
	{
		using var reader = new StreamReader(Request.Body);
		var payload = await reader.ReadToEndAsync();

		if (!Request.Headers.TryGetValue("x-paystack-signature", out var signature))
		{
			_logger.LogWarning("Webhook called without signature header");
			return BadRequest(new { message = "Signature header missing" });
		}

		var command = new HandleWebhookCommand(payload, signature!);
		var result = await _mediator.Send(command);

		if (!result)
		{
			return BadRequest(new { message = "Webhook processing failed" });
		}

		return Ok(new { message = "Webhook processed successfully" });
	}
}