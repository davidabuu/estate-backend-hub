using System.Text.Json.Serialization;

namespace PaymentService.Application.Model;

public class PaystackResponse<T>
{
	[JsonPropertyName("status")]
	public bool Status { get; set; }

	[JsonPropertyName("message")]
	public string? Message { get; set; }

	[JsonPropertyName("data")]
	public T? Data { get; set; }
}

public class InitializePaymentData
{
	[JsonPropertyName("authorization_url")]
	public string? AuthorizationUrl { get; set; }

	[JsonPropertyName("access_code")]
	public string? AccessCode { get; set; }

	[JsonPropertyName("reference")]
	public string? Reference { get; set; }
}

public class VerifyPaymentData
{
	[JsonPropertyName("reference")]
	public string? Reference { get; set; }

	[JsonPropertyName("amount")]
	public decimal? Amount { get; set; }

	[JsonPropertyName("amount_paid")]
	public decimal? AmountPaid { get; set; }

	[JsonPropertyName("status")]
	public string? Status { get; set; }

	[JsonPropertyName("gateway_response")]
	public string? GatewayResponse { get; set; }

	[JsonPropertyName("channel")]
	public string? Channel { get; set; }

	[JsonPropertyName("currency")]
	public string? Currency { get; set; }

	[JsonPropertyName("fee")]
	public decimal? Fee { get; set; }

	[JsonPropertyName("customer")]
	public CustomerData? Customer { get; set; }
}

public class CustomerData
{
	[JsonPropertyName("email")]
	public string? Email { get; set; }

	[JsonPropertyName("phone")]
	public string? Phone { get; set; }

	[JsonPropertyName("first_name")]
	public string? FirstName { get; set; }

	[JsonPropertyName("last_name")]
	public string? LastName { get; set; }
}