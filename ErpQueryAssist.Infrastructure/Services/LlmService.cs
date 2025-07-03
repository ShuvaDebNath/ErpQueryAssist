using ErpQueryAssist.Application.Interfaces;
using ErpQueryAssist.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ErpQueryAssist.Infrastructure.Services
{
    public class LlmService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAIOptions _options;
        private readonly string _apiKey;

        public LlmService(IOptions<OpenAIOptions> options, IConfiguration configuration)
        {
            _options = options.Value;
            _httpClient = new HttpClient();
            _apiKey = Environment.GetEnvironmentVariable("OpenAI__ApiKey")
              ?? configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new Exception("OpenAI API Key not configured.");
        }

        public async Task<string> AskAsync(string prompt)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    //new { role = "system", content = "You are an expert ERP assistant. Convert the user's request into a SQL Server query." },
                    new { role = "system", content = "You are an expert SQL assistant. Only respond with valid SQL Server SELECT statements. No explanations, no descriptions — just the SQL code only." },
                    new { role = "user", content = prompt }
                }
            };

            var requestJson = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

            var response = await _httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(3000); // wait 3 seconds
                var retryRequest = new HttpRequestMessage(HttpMethod.Post, request.RequestUri)
                {
                    Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                };
                retryRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
                response = await _httpClient.SendAsync(retryRequest);
            }

            var responseJson = await response.Content.ReadAsStringAsync();
           
            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var content = doc.RootElement
                                 .GetProperty("choices")[0]
                                 .GetProperty("message")
                                 .GetProperty("content")
                                 .GetString();

                return content?.Trim() ?? "[Empty Response]";
            }
            catch (Exception ex)
            {
                return $"[OpenAI response parse error]: {ex.Message}";
            }
        }
    }
}
