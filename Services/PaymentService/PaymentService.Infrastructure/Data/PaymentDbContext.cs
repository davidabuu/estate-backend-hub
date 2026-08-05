using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;


namespace PaymentService.Infrastructure.Data;

public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
	public DbSet<Payment> Payments { get; set; }
	public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }
	public DbSet<ResidentDues> ResidentDues { get; set; }
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Payment Configuration
		modelBuilder.Entity<Payment>(entity =>
		{
			
			entity.HasKey(p => p.Id);

			entity.Property(p => p.Reference)
				  .HasMaxLength(50)
				  .IsRequired();

			entity.Property(p => p.Amount)
				  .HasPrecision(18, 2);

			entity.Property(p => p.AmountPaid)
				  .HasPrecision(18, 2);

			entity.Property(p => p.Fee)
				  .HasPrecision(18, 2);

			entity.Property(p => p.IdempotencyKey)
				  .HasMaxLength(100);

			// Indexes for performance
			entity.HasIndex(p => p.Reference)
				  .IsUnique();

			entity.HasIndex(p => p.UserId);
			entity.HasIndex(p => p.ResidentDueId);
			entity.HasIndex(p => p.Status);
			entity.HasIndex(p => p.TransactionReference);
			entity.HasIndex(p => p.IdempotencyKey);
			entity.HasIndex(p => p.CreatedAt);
		});

		// IdempotencyRecord Configuration
		modelBuilder.Entity<IdempotencyRecord>(entity =>
		{
			
			entity.HasKey(i => i.Id);

			entity.Property(i => i.Key)
				  .HasMaxLength(100)
				  .IsRequired();

			entity.HasIndex(i => i.Key)
				  .IsUnique();

			entity.HasIndex(i => i.ExpiresAt);
		});
	}
}