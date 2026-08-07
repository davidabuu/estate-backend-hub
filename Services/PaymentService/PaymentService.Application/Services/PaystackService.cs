using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Interface;

using PaymentService.Application.Model;

using System.Text;
using System.Text.Json;

namespace PaymentService.Infrastructure.Services;

public class PaystackService : IPaystackService
{
	private readonly HttpClient _httpClient;
	private readonly string _secretKey;
	private readonly ILogger<PaystackService> _logger;

	public PaystackService(
		HttpClient httpClient,
		IConfiguration configuration,
		ILogger<PaystackService> logger)
	{
		_httpClient = httpClient;
		_secretKey = configuration["Paystack:SecretKey"]
			?? throw new Exception("Paystack SecretKey is missing");
		_logger = logger;
	}

	public async Task<PaystackResponse<InitializePaymentData>> InitializePaymentAsync(
		string email,
		decimal amount,
		string reference,
		string? callbackUrl = null)
	{
		try
		{
			var request = new
			{
				email,
				amount = (int)(amount * 100), // Convert to kobo
				reference,
				callback_url = callbackUrl
			};

			var requestJson = JsonSerializer.Serialize(request);
			_logger.LogInformation("📤 Paystack Request: {Request}", requestJson);

			var content = new StringContent(
				JsonSerializer.Serialize(request),
				Encoding.UTF8,
				"application/json"
			);

			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_secretKey}");

			var response = await _httpClient.PostAsync("/transaction/initialize", content);

			var responseContent = await response.Content.ReadAsStringAsync();
			_logger.LogInformation("📥 Paystack Raw Response: {Response}", responseContent);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("❌ Paystack error: {StatusCode} - {Response}", response.StatusCode, responseContent);
				throw new Exception($"Paystack error: {responseContent}");
			}

			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
			};

			var result = JsonSerializer.Deserialize<PaystackResponse<InitializePaymentData>>(
				responseContent,
				options
			);

			if (result == null)
			{
				_logger.LogError("❌ Failed to deserialize Paystack response");
				throw new Exception("Failed to parse Paystack response");
			}

			if (!result.Status)
			{
				_logger.LogError("❌ Paystack initialization failed: {Message}", result.Message);
				throw new Exception(result.Message ?? "Failed to initialize payment");
			}

			if (result.Data == null)
			{
				_logger.LogError("❌ Paystack Data is NULL");
				throw new Exception("Paystack returned null data");
			}

			_logger.LogInformation("✅ Paystack Success - AuthUrl: {AuthUrl}, AccessCode: {AccessCode}",
				result.Data.AuthorizationUrl, result.Data.AccessCode);

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "❌ Error initializing Paystack payment");
			throw;
		}
	}

	public async Task<PaystackResponse<VerifyPaymentData>> VerifyPaymentAsync(string reference)
	{
		try
		{
			_logger.LogInformation("🔍 Verifying payment with reference: {Reference}", reference);

			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_secretKey}");

			var response = await _httpClient.GetAsync($"/transaction/verify/{reference}");

			var responseContent = await response.Content.ReadAsStringAsync();
			_logger.LogInformation("📥 Paystack Verify Response: {Response}", responseContent);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("❌ Paystack verify error: {StatusCode} - {Response}", response.StatusCode, responseContent);
				throw new Exception($"Paystack verify error: {responseContent}");
			}

			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
			};

			var result = JsonSerializer.Deserialize<PaystackResponse<VerifyPaymentData>>(
				responseContent,
				options
			);

			if (result == null)
			{
				throw new Exception("Failed to parse Paystack response");
			}

			_logger.LogInformation("✅ Payment verification status: {Status}", result.Status);

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "❌ Error verifying Paystack payment for reference: {Reference}", reference);
			throw;
		}
	}

	public async Task<bool> VerifyWebhookSignatureAsync(string payload, string signature)
	{
		try
		{
			var expectedSignature = GenerateSignature(payload);

			// ✅ FIX: Case-insensitive comparison
			var isValid = string.Equals(expectedSignature, signature, StringComparison.OrdinalIgnoreCase);

			if (!isValid)
			{
				_logger.LogWarning("⚠️ Invalid webhook signature. Expected: {Expected}, Received: {Received}",
					expectedSignature, signature);
			}
			else
			{
				_logger.LogInformation("✅ Webhook signature verified");
			}

			return isValid;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "❌ Error verifying webhook signature");
			return false;
		}
	}

	private string GenerateSignature(string payload)
	{
		var key = Encoding.UTF8.GetBytes(_secretKey);
		var payloadBytes = Encoding.UTF8.GetBytes(payload);

		using var hmac = new System.Security.Cryptography.HMACSHA512(key);
		var hash = hmac.ComputeHash(payloadBytes);

		return Convert.ToHexString(hash);
	}
}