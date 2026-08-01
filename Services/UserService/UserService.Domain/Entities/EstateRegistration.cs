
using UserService.Application.Enums;

namespace UserService.Domain.Entities;

public class EstateRegistration
{
	public Guid Id { get; set; }

	public Guid UserId { get; set; }

	public ApplicationUser User { get; set; } = null!;

	public string EstateName { get; set; } = string.Empty;

	public string EstateAddress { get; set; } = string.Empty;

	public string EstateState { get; set; } = string.Empty;

	public List<PropertyType>? PropertyTypes { get; set; }

	public string? BankName { get; set; }

	public string? AccountName { get; set; }

	public string? AccountNumber { get; set; }

	public string? BankCode { get; set; }

	public string? EstateRegistrationDocUrl { get; set; }

	public string? EstateAssociationRegistrationDocUrl { get; set; }

	public bool IsApproved { get; set; } = false;		

	public DateTime CreatedAt { get; set; }

	public DateTime? ApprovedAt { get; set; }

	
}