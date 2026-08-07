using UserService.Application.Enums;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

public class ResidentDues
{
	public Guid Id { get; set; }
	public Guid ResidentId { get; set; }
	public Guid EstateDueId { get; set; }
	public string? Email { get; set; }    // For PaymentService to use
	
	public string? DueName { get; set; }
	public string? Description { get; set; }
	public decimal Amount { get; set; }
	public DueType DueType { get; set; }
	public DateTime DueDate { get; set; }
	public DueStatus Status { get; set; } = DueStatus.Pending;
	public bool IsPaid { get; set; } = false;
	public DateTime? PaidAt { get; set; }
	public string? PaymentReference { get; set; }
	public PropertyType PropertyType { get; set; }  
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}