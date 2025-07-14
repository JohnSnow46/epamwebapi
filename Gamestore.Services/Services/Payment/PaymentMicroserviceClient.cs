using System.Text;
using System.Text.Json;
using Gamestore.Services.Dto.PaymentDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Payment;
public class PaymentMicroserviceClient(HttpClient httpClient, IConfiguration configuration, ILogger<PaymentMicroserviceClient> logger) : IPaymentMicroserviceClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<PaymentMicroserviceClient> _logger = logger;
    private readonly int _maxRetries = configuration.GetValue("PaymentSettings:RetryAttempts", 3);
    private readonly int _baseDelayMs = configuration.GetValue("PaymentSettings:RetryDelayMs", 1000);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
    public async Task<bool> ProcessVisaPaymentAsync(VisaMicroserviceRequestDto request)
    {
        _logger.LogInformation("Processing Visa payment for amount {Amount}", request.TransactionAmount);

        try
        {
            var json = JsonSerializer.Serialize(request, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug("Sending request to microservice: {Json}", json);

            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await _httpClient.PostAsync("/api/payments/visa", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Visa payment processed successfully");
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Visa payment failed with status {StatusCode}: {Error}",
                    response.StatusCode, errorContent);
                return false;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing Visa payment");
            return false;
        }
    }

    public async Task<bool> ProcessIBoxPaymentAsync(BoxMicroserviceRequestDto request)
    {
        _logger.LogInformation("Processing IBox payment for amount {Amount}, account {AccountNumber}",
            request.TransactionAmount, request.AccountNumber);

        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.PostAsync("/api/payments/ibox", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("IBox payment processed successfully");
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("IBox payment failed with status {StatusCode}: {Error}",
                response.StatusCode, errorContent);
            return false;
        });
    }

    private async Task<bool> ExecuteWithRetryAsync(Func<Task<bool>> operation)
    {
        var attempt = 0;
        var delay = _baseDelayMs;

        while (attempt < _maxRetries)
        {
            attempt++;

            try
            {
                var result = await operation();
                if (result)
                {
                    return true;
                }

                if (attempt < _maxRetries)
                {
                    _logger.LogInformation("Payment attempt {Attempt} failed, retrying in {Delay}ms", attempt, delay);
                    await Task.Delay(delay);
                    delay *= 2;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment attempt {Attempt} threw exception", attempt);

                if (attempt < _maxRetries)
                {
                    await Task.Delay(delay);
                    delay *= 2;
                }
                else
                {
                    throw;
                }
            }
        }

        return false;
    }
}

