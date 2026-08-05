namespace PaymentService.Domain.Entities;

public class IdempotencyRecord
{
	public Guid Id { get; set; }
	public string Key { get; set; } = string.Empty;
	public string Response { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
	public DateTime? ExpiresAt { get; set; }
}