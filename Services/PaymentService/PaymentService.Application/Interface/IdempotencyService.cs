namespace PaymentService.Application.Interface;

public interface IIdempotencyService
{
	Task<bool> IsProcessedAsync(string key, CancellationToken ct = default);
	Task MarkAsProcessedAsync(string key, string response, CancellationToken ct = default);
	Task<string?> GetResponseAsync(string key, CancellationToken ct = default);
	Task CleanupExpiredAsync(CancellationToken ct = default);
	string GenerateKey(string prefix, params string[] parts);
}