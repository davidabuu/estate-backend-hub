using UserService.Application.Enums;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

public class EstateDue
{
	public Guid Id { get; set; }
	public Guid EstateId { get; set; }
	public string? DueName { get; set; }
	public string? Description { get; set; }
	public decimal Amount { get; set; }
	public DueType DueType { get; set; }
	public DateTime DueDate { get; set; }
	public Dictionary<PropertyType, decimal> PropertyTypeAmounts { get; set; } = new();
	public bool IsActive { get; set; } = true;
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}