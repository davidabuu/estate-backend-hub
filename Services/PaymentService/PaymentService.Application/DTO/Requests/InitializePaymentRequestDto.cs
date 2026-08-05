namespace PaymentService.Application.DTO.Requests;

public record InitializePaymentRequestDto(
	Guid ResidentDueId
);