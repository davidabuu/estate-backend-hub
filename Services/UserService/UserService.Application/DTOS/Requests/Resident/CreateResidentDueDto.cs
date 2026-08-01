

using UserService.Application.Enums;
using UserService.Domain.Enums;

namespace UserService.Application.DTOS.Requests.Resident    
{
	public record CreateResidentDueDto
  
	{
		public Guid EstateId { get; set; }
		public string DueName { get; set; } = string.Empty;
		public string? Description { get; set; }
		public decimal Amount { get; set; }
		public DueType DueType { get; set; }
		public DateTime DueDate { get; set; }


	}
    
}
