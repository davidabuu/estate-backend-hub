namespace UserService.Application.DTOs.Responses.Feedback;

public record FeedbackWithResponsesResponse(
	int FeedbackId,
	string Content,
	DateTime SubmittedOn,
	List<FeedbackResponseModel> Responses
);