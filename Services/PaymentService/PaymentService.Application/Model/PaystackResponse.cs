namespace PaymentService.Application.Model;

public class PaystackResponse<T>
{
	public bool Status { get; set; }
	public string? Message { get; set; }
	public T? Data { get; set; }
}

public class InitializePaymentData
{
	public string? AuthorizationUrl { get; set; }
	public string? AccessCode { get; set; }
	public string? Reference { get; set; }
}

public class VerifyPaymentData
{
	public string? Reference { get; set; }
	public decimal? Amount { get; set; }
	public decimal? AmountPaid { get; set; }
	public string? Status { get; set; }
	public string? GatewayResponse { get; set; }
	public string? Channel { get; set; }
	public string? Currency { get; set; }
	public decimal? Fee { get; set; }
	public CustomerData? Customer { get; set; }
}

public class CustomerData
{
	public string? Email { get; set; }
	public string? Phone { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
}