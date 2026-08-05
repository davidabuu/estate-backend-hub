using MediatR;

namespace PaymentService.Application.Command;

public record HandleWebhookCommand(
	string Payload,
	string Signature
) : IRequest<bool>;