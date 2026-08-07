using MassTransit;
using Microsoft.Extensions.Logging;
using EstateHub.Contracts.Events;

namespace PaymentService.Application.Consumers;

public class TestConsumer : IConsumer<TestEvent>
{
	private readonly ILogger<TestConsumer> _logger;

	public TestConsumer(ILogger<TestConsumer> logger)
	{
		_logger = logger;
	}

	public async Task Consume(ConsumeContext<TestEvent> context)
	{
		_logger.LogInformation("✅ TEST EVENT RECEIVED: {Message}", context.Message.Message);
		await Task.CompletedTask;
	}
}