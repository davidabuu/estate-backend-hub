using PaymentService.Application.Model;

namespace PaymentService.Application.Interface;

public interface IPaystackService
{
	Task<PaystackResponse<InitializePaymentData>> InitializePaymentAsync(
		string email,
		decimal amount,
		string reference,
		string? callbackUrl = null);

	Task<PaystackResponse<VerifyPaymentData>> VerifyPaymentAsync(string reference);

	Task<bool> VerifyWebhookSignatureAsync(string payload, string signature);
}