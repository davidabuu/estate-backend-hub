namespace PaymentService.Application.DTO.Responses;

public record InitializePaymentResponseDto(
	string Reference,
	string AuthorizationUrl,
	string AccessCode,
	Guid PaymentId,
	string Message
);