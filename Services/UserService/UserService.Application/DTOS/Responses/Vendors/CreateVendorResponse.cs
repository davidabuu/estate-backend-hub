namespace UserService.Application.DTOs.Responses.Vendors;

public record CreateVendorResponse(
	int VendorId,
	string Name,
	string EstateId,
	string ServiceType,
	string Message,
	bool IsCreated
);