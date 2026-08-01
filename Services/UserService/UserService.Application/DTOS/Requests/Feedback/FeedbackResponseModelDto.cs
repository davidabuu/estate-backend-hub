namespace UserService.Application.DTOS.Requests.Feedback
{
    public record FeedbackResponseModelDto
    {
      
        public int ResponseId { get; set; }
        public int FeedbackId { get; set; }
        public string? Content { get; set; }
        public DateTime RespondedOn { get; set; }
        public string? Responded { get; set; }
    }
}
