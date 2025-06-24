using System.Diagnostics;
using System.Text;
using System.Text.Json;

public class LlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LlmService> _logger;
    private readonly ICache _cache;
    private readonly string ollamaBaseAddrs;
    private readonly string mlxBaseAddrs;
    private const string LLM_LOG_KEY = "llm_requests_log";

    public LlmService(HttpClient httpClient, ILogger<LlmService> logger, ICache cache, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
        this.ollamaBaseAddrs = configuration["Ollama:BaseAddress"];
        this.mlxBaseAddrs = configuration["MlxApi:BaseAddress"];
    }

    public async Task<string> GetLLMResponseOllama(string llmPrompt)
    {
        var stopwatch = Stopwatch.StartNew();
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
            llmRawResponse = null;
        }
        finally
        {
            stopwatch.Stop();
            
            // Log to cache
            var logEntry = new LlmRequestLog
            {
                MethodName = nameof(GetLLMResponseOllama),
                Prompt = llmPrompt,
                Response = llmRawResponse,
                TimeTakenMs = stopwatch.ElapsedMilliseconds,
                Timestamp = DateTime.UtcNow,
                Model = "phi3:3.8b",
                Success = !string.IsNullOrEmpty(llmRawResponse)
            };
            
            try
            {
                await _cache.PushToListAsync(LLM_LOG_KEY, logEntry);
            }
            catch (Exception cacheEx)
            {
                _logger.LogError(cacheEx, "Failed to log LLM request to cache");
            }
        }
        
        return llmRawResponse;
    }

    public async Task<string> GetLLMResponseMlx(string llmPrompt,int maxTokens)
    {
                // model_name = "Qwen2.5-3B-Instruct-4bit",
        string modeltochoose = "Qwen2.5-7B-bf16";
        var stopwatch = Stopwatch.StartNew();
        var llmRawResponse = "";
        
        try
        {
            var payload = new
            {
                prompt = llmPrompt,
                // model_name = "Qwen2.5-3B-Instruct-4bit",
                model_name = modeltochoose,
                max_tokens = maxTokens,
                temperature = 0.7
            };
            
            _logger.LogDebug("MLX LLM Prompt: {Prompt}", llmPrompt);
            var requestContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(mlxBaseAddrs + "generate", requestContent);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            if (doc.RootElement.TryGetProperty("text", out var respElement))
            {
                llmRawResponse = respElement.GetString()?.Trim();
            }
            _logger.LogDebug("MLX LLM Raw Response: {Response}", llmRawResponse);

            if (string.IsNullOrWhiteSpace(llmRawResponse))
            {
                _logger.LogWarning("MLX LLM returned empty");
                return null;
            }
        }
        catch (Exception ex){
            _logger.LogError(ex, "An error occurred during MLX LLM call.");
            llmRawResponse = null;
        }
        finally
        {
            stopwatch.Stop();
            
            // Log to cache
            var logEntry = new LlmRequestLog
            {
                MethodName = nameof(GetLLMResponseMlx),
                Prompt = llmPrompt,
                Response = llmRawResponse,
                TimeTakenMs = stopwatch.ElapsedMilliseconds,
                Timestamp = DateTime.UtcNow,
                Model = modeltochoose,
                Success = !string.IsNullOrEmpty(llmRawResponse)
            };
            
            try
            {
                await _cache.PushToListAsync(LLM_LOG_KEY, logEntry);
            }
            catch (Exception cacheEx)
            {
                _logger.LogError(cacheEx, "Failed to log LLM request to cache");
            }
        }
        
        return llmRawResponse;
    }

    // Method to retrieve logged LLM requests
    public async Task<List<LlmRequestLog>> GetLlmRequestHistory()
    {
        try
        {
            return await _cache.GetListAsync<LlmRequestLog>(LLM_LOG_KEY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve LLM request history from cache");
            return new List<LlmRequestLog>();
        }
    }

    private class OllamaResponse
    {
        public string response { get; set; }
    }
}

// Data model for LLM request logging
public class LlmRequestLog
{
    public string MethodName { get; set; }
    public string Prompt { get; set; }
    public string Response { get; set; }
    public long TimeTakenMs { get; set; }
    public DateTime Timestamp { get; set; }
    public string Model { get; set; }
    public bool Success { get; set; }
}