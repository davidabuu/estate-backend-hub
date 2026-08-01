namespace UserService.Application.DTOS.Requests.Feedback
{

	public record FeedbackWithResponsesModelDto
        {
            public int FeedbackId { get; set; }
            public string? Content { get; set; }
            public DateTime SubmittedOn { get; set; }
            public List<FeedbackResponseModelDto> Responses { get; set; } = new List<FeedbackResponseModelDto>();
        }
    
}
