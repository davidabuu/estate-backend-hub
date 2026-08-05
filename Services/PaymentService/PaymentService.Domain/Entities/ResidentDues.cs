

using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public class ResidentDues
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public Guid EstateId { get; set; }
	public string? DueName { get; set; }
	public string? Description { get; set; }
	public decimal Amount { get; set; }
	public DueType DueType { get; set; }
	public DateTime DueDate { get; set; }
	public DueStatus Status { get; set; }
	public bool IsPaid { get; set; }
	public DateTime? PaidAt { get; set; }
	public string? PaymentReference { get; set; }
	public string? Email { get; set; }
	public PropertyType PropertyType { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}