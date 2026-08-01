namespace UserService.Application.DTOS.Responses.Dues;

public record DueResponse(
	int Id,
	string DueName,
	decimal ServiceChargeFee,
	decimal FineFee,
	bool Paid,
	bool IsActive,
	DateTime DateCreated
);