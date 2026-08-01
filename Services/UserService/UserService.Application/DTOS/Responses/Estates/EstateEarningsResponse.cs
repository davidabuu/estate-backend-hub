namespace UserService.Application.DTOs.Responses.Estates;

public record EstateEarningsResponse(
	decimal TotalPaidAmount,
	decimal TotalUnPaidAmount
); 