namespace UserService.Application.DTOS.Responses.Residents;

public record BulkResidentUploadResponse(
	int TotalRecords,
	int SuccessCount,
	int FailedCount,
	List<BulkResidentError> Errors
);

public record BulkResidentError(
	int RowNumber,
	string Email,
	string ErrorMessage
);