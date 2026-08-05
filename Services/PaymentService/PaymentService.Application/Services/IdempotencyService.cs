using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Interface;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace PaymentService.Application.Services;

public class IdempotencyService : IIdempotencyService
{
	private readonly PaymentDbContext _dbContext;
	private readonly ILogger<IdempotencyService> _logger;
	private readonly TimeSpan _expiryTime = TimeSpan.FromHours(24);

	public IdempotencyService(
		PaymentDbContext dbContext,
		ILogger<IdempotencyService> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<bool> IsProcessedAsync(string key, CancellationToken ct = default)
	{
		try
		{
			var record = await _dbContext.IdempotencyRecords
				.FirstOrDefaultAsync(r => r.Key == key && r.ExpiresAt > DateTime.UtcNow, ct);

			var isProcessed = record != null;

			if (isProcessed)
			{
				_logger.LogDebug("Idempotency key already processed: {Key}", key);
			}

			return isProcessed;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error checking idempotency key: {Key}", key);
			return false;
		}
	}

	public async Task MarkAsProcessedAsync(string key, string response, CancellationToken ct = default)
	{
		try
		{
			var record = new IdempotencyRecord
			{
				Id = Guid.NewGuid(),
				Key = key,
				Response = response,
				CreatedAt = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow.Add(_expiryTime)
			};

			await _dbContext.IdempotencyRecords.AddAsync(record, ct);
			await _dbContext.SaveChangesAsync(ct);

			_logger.LogDebug("Idempotency key marked as processed: {Key}", key);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error marking idempotency key: {Key}", key);
			throw;
		}
	}

	public async Task<string?> GetResponseAsync(string key, CancellationToken ct = default)
	{
		try
		{
			var record = await _dbContext.IdempotencyRecords
				.FirstOrDefaultAsync(r => r.Key == key && r.ExpiresAt > DateTime.UtcNow, ct);

			return record?.Response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting idempotency response for key: {Key}", key);
			return null;
		}
	}

	public async Task CleanupExpiredAsync(CancellationToken ct = default)
	{
		try
		{
			var expired = await _dbContext.IdempotencyRecords
				.Where(r => r.ExpiresAt < DateTime.UtcNow)
				.ToListAsync(ct);

			if (expired.Any())
			{
				_dbContext.IdempotencyRecords.RemoveRange(expired);
				await _dbContext.SaveChangesAsync(ct);

				_logger.LogInformation("Cleaned up {Count} expired idempotency records", expired.Count);
			}
			else
			{
				_logger.LogDebug("No expired idempotency records to clean up");
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error cleaning up expired idempotency records");
			throw;
		}
	}

	public string GenerateKey(string prefix, params string[] parts)
	{
		try
		{
			var combined = string.Join(":", parts);

			using var sha256 = SHA256.Create();
			var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
			var hash = Convert.ToBase64String(hashBytes)
				.Replace('+', '-')
				.Replace('/', '_')
				.TrimEnd('=');

			var key = $"{prefix}:{hash}";

			_logger.LogDebug("Generated idempotency key: {Key}", key);

			return key;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error generating idempotency key");
			throw;
		}
	}
}