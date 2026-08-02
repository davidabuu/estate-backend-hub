using UserService.Application.Enums;
using UserService.Domain.Enums;

namespace UserService.Application.DTOS.Requests.Auth;

public record ResidentRegistrationRequestDto
{
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Password { get; set; }
	public UserType? RegisterAs { get; set; }
	public PropertyType PropertyType { get; set; } 
	public string? MeterNumber { get; set; }
	public Guid EstateId { get; set; }
	public string? HouseAddress { get; set; }
	public bool? IsRegistered { get; set; } = false;
}