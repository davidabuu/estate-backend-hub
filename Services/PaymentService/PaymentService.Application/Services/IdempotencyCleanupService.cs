using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Interface;


namespace PaymentService.Application.Services;

public class IdempotencyCleanupService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<IdempotencyCleanupService> _logger;

	public IdempotencyCleanupService(
		IServiceProvider serviceProvider,
		ILogger<IdempotencyCleanupService> logger)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using var scope = _serviceProvider.CreateScope();
				var idempotencyService = scope.ServiceProvider.GetRequiredService<IIdempotencyService>();

				await idempotencyService.CleanupExpiredAsync(stoppingToken);
				_logger.LogInformation("Idempotency records cleaned up");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error cleaning up idempotency records");
			}

			// Run every hour
			await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
		}
	}
}