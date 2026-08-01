using UserService.Application.Enums;

namespace UserService.Domain.Entities;

public class ResidentRegistration
{
	public Guid Id { get; set; }

	public Guid UserId { get; set; }
	public ApplicationUser User { get; set; } = null!;

	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }

	public Guid EstateId { get; set; }
	public string? EstateName { get; set; }
	public UserType? RegisterAs { get; set; }
	public string? HouseType { get; set; }
	public string? MeterNumber { get; set; }
	public string? HouseAddress { get; set; }

	
	public PropertyType PropertyType { get; set; }

	public bool IsActive { get; set; }
	public bool IsRegistered { get; set; }
	public DateTime CreatedAt { get; set; }
}