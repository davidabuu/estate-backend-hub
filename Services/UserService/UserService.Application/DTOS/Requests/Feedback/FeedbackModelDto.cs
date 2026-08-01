
namespace UserService.Application.DTOS.Requests.Feedback
{
	public record FeedbackModelDto
    {
   
        public int FeedbackId { get; set; }

        public string? EstateId { get; set; }   
        public string? UserId { get; set; }  
        public string? Content { get; set; }
        public DateTime SubmittedOn { get; set; }
     
    }
}
