namespace UserService.Application.DTOs.Responses.Payments;

public record PaymentReceiptResponse(
	string ReceiptNumber,
	string ResidentName,
	string ResidentEmail,
	string EstateName,
	string DueName,
	decimal Amount,
	decimal PaidAmount,
	DateTime PaidAt,
	string PaymentReference,
	string PaymentMethod,
	string Status
);