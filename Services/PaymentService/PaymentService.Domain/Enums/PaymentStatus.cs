namespace PaymentService.Domain.Enums;

public enum PaymentStatus
{
	Pending = 1,       
	Initiated = 2,     
	Success = 3,      
	Failed = 4,       
	Abandoned = 5,     
	Refunded = 6,    
	Reversed = 7
}