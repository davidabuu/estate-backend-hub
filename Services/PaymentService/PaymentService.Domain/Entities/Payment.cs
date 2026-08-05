using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public class Payment
{
	public Guid Id { get; set; }

	// Related entities (from other services)
	public Guid UserId { get; set; }
	public Guid? ResidentId { get; set; }
	public Guid? EstateId { get; set; }
	public Guid? ResidentDueId { get; set; }
	public Guid? EstateDueId { get; set; }

	// Paystack specific
	public string? Reference { get; set; }
	public string? AccessCode { get; set; }
	public string? AuthorizationUrl { get; set; }

	// Payment details
	public decimal Amount { get; set; }
	public decimal AmountPaid { get; set; }
	public decimal? Fee { get; set; }
	public string? Currency { get; set; }

	// Status
	public PaymentStatus Status { get; set; }
	public string? GatewayResponse { get; set; }
	public PaymentChannel Channel { get; set; }
	public string? TransactionReference { get; set; }

	// Customer details
	public string? CustomerEmail { get; set; }
	public string? CustomerPhone { get; set; }
	public string? CustomerName { get; set; }

	// Metadata (JSON for extra data)
	public string? Metadata { get; set; }

	// Idempotency
	public string? IdempotencyKey { get; set; }

	// Timestamps
	public DateTime CreatedAt { get; set; }
	public DateTime? InitiatedAt { get; set; }
	public DateTime? PaidAt { get; set; }
	public DateTime? UpdatedAt { get; set; }

	// Webhook tracking
	public bool WebhookProcessed { get; set; }
	public DateTime? WebhookProcessedAt { get; set; }
	public int WebhookAttempts { get; set; }

	// Soft delete
	public bool IsDeleted { get; set; }
}