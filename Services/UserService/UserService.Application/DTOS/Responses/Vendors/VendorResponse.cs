namespace UserService.Application.DTOs.Responses.Vendors;

public record VendorResponse(
	int VendorId,
	string Name,
	string EstateId,
	string ServiceType,
	string AccountNumber,
	string Duration,
	string Amount,
	string ContactName,
	string NoOfWorkers,
	string Phone,
	DateTime ServiceDate,
	string PaymentReference,
	string PaymentStatus,
	bool IsActive,
	string Description,
	DateTime DatePaid
);