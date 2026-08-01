using Microsoft.AspNetCore.Identity;

namespace UserService.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
	public string? FullName { get; set; }
	public DateTime CreatedAt { get; set; }
	public bool IsActive { get; set; } = true;
	public bool IsApproved { get; set; }
	public string? RefreshToken { get; set; }
	public DateTime? RefreshTokenExpiry { get; set; }
}