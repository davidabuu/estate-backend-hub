namespace UserService.Application.DTOS.Responses.Estates;

public record EarningReportResponse(
	int Year,
	int? Month,
	DateTime StartDate,
	DateTime EndDate,
	decimal TotalEarnings,
	decimal OutstandingPayments
);