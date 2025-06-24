using System.Text;
using System.Text.Json;

public class LlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LlmService> _logger;
    private readonly string ollamaBaseAddrs;
    private readonly string mlxBaseAddrs;

    public LlmService(HttpClient httpClient, ILogger<LlmService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        this.ollamaBaseAddrs = configuration["Ollama:BaseAddress"];
        this.mlxBaseAddrs = configuration["MlxApi:BaseAddress"];
    }

    public async Task<string> GetLLMResponseOllama(string llmPrompt)
    {
        var llmRawResponse = "";
        try
        {
            var payload = new
            {
                model = "phi3:3.8b",
                prompt = llmPrompt
            };
            _logger.LogDebug("LLM Prompt for type  {Prompt}", llmPrompt);
            var requestContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{ollamaBaseAddrs}api/generate", requestContent);
            response.EnsureSuccessStatusCode();

            var responseStream = await response.Content.ReadAsStreamAsync();
            var reader = new StreamReader(responseStream);

            // Read line by line since Ollama returns streaming JSON
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrEmpty(line)) continue;
                var jsonResponse = JsonSerializer.Deserialize<OllamaResponse>(line);
                if (jsonResponse?.response != null)
                {
                    llmRawResponse += jsonResponse.response;
                }
            }
            llmRawResponse = llmRawResponse.Trim();
            _logger.LogDebug("LLM Raw Response for type  {Response}", llmRawResponse);

            if (string.IsNullOrWhiteSpace(llmRawResponse))
            {
                _logger.LogWarning("LLM returned empty");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during LLM re-ranking for type.");
            return null;
        }
        return llmRawResponse;
    }

    public async Task<string> GetLLMResponseMlx(string llmPrompt)
    {
        var llmRawResponse = "";
        try
        {
            var payload = new
            {
                prompt = llmPrompt,
                model_name = "Qwen2.5-3B-Instruct-4bit",
                max_tokens = 100,
                temperature = 0.7
            };
            _logger.LogDebug("MLX LLM Prompt: {Prompt}", llmPrompt);
            var requestContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(mlxBaseAddrs+ "generate", requestContent);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            if (doc.RootElement.TryGetProperty("response", out var respElement))
            {
                llmRawResponse = respElement.GetString()?.Trim();
            }
            else if (doc.RootElement.TryGetProperty("result", out var resultElement))
            {
                llmRawResponse = resultElement.GetString()?.Trim();
            }

            _logger.LogDebug("MLX LLM Raw Response: {Response}", llmRawResponse);

            if (string.IsNullOrWhiteSpace(llmRawResponse))
            {
                _logger.LogWarning("MLX LLM returned empty");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during MLX LLM call.");
            return null;
        }
        return llmRawResponse;
    }

    private class OllamaResponse
    {
        public string response { get; set; }
    }
}