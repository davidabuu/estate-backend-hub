namespace UserService.Application.DTOs.Responses.Payments;

public record PaymentHistoryResponse(
	int Id,
	string DueName,
	string EstateId,
	int DueId,
	string UserId,
	string PaymentReference,
	string Email,
	bool DisbursedToEstate,
	string DisbursementReference,
	decimal Amount,
	int DueDuration,
	bool IsPaid,
	DateTime PaymentDate
);