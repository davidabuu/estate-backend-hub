namespace UserService.Application.DTOs.Responses.Vendors;

public record VendorPaymentResponse(
	int Id,
	string Name,
	string EstateId,
	string ServiceType,
	string Bank,
	string Duration,
	string ContactName,
	string Phone,
	DateTime ServiceDate,
	string PaymentStatus,
	bool IsActive,
	string Description,
	DateTime DatePaid
);