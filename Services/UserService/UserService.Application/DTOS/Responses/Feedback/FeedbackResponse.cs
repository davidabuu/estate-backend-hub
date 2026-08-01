namespace UserService.Application.DTOs.Responses.Feedback;

public record FeedbackResponse(
	int FeedbackId,
	string EstateId,
	string UserId,
	string Content,
	DateTime SubmittedOn,
	List<FeedbackResponseModel> Responses
);

public record FeedbackResponseModel(
	int ResponseId,
	int FeedbackId,
	string Content,
	DateTime RespondedOn,
	string RespondedBy
);