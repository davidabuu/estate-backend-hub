using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Enums;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data;

public class UserDbContext(DbContextOptions<UserDbContext> options)
		: IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
	public DbSet<EstateRegistration> EstateRegistration { get; set; }
	public DbSet<ResidentRegistration> ResidentRegistration { get; set; }

	 public DbSet<EstateDue> EstateDue { get; set; }
	public DbSet<ResidentDues> ResidentDues { get; set; }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		
		builder.Entity<EstateRegistration>()
			.HasOne(e => e.User)
			.WithMany()
			.HasForeignKey(e => e.UserId)
			.OnDelete(DeleteBehavior.Cascade);


		
		builder.Entity<ResidentRegistration>()
			.HasOne(r => r.User)
			.WithMany()
			.HasForeignKey(r => r.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		// EstateDue Configuration
		builder.Entity<EstateDue>(entity =>
		{
		

		
			entity.Property(e => e.PropertyTypeAmounts)
				.HasConversion(
					v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
					v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<PropertyType, decimal>>(v, (System.Text.Json.JsonSerializerOptions?)null)
						?? new Dictionary<PropertyType, decimal>()
				);
		});
	}
}