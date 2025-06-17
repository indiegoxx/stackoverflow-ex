using System.Text;
using System.Text.Json;

public interface ILlmService
{
    Task<List<StackOverflowQuestion>?> RerankItemsWithLLMAsync(List<StackOverflowQuestion> items, string contextPrompt);
    Task<string?> LLMSuggestedAnswer(string question);
}

public class LlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LlmService> _logger;
    string? ollamaBaseAddrs;
    private record OllamaResponse
    {
        public string? response { get; init; }
    }

    public LlmService(IHttpClientFactory httpClientFactory, ILogger<LlmService> logger,IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("OllamaClient");
        ollamaBaseAddrs = configuration["Ollama:BaseAddress"];
        _logger = logger;
    }

    public async Task<string> GetLLMResponse(string llmPrompt, string model)
    {
        var llmRawResponse = "";
        try
        {
            var payload = new
            {
                model,
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


    public async Task<string?> LLMSuggestedAnswer(string question)
    {
        StringBuilder llmPromptBuilder = new StringBuilder();
        llmPromptBuilder.AppendLine($"You are an expert question and answer assistant in context of Programming & Software Development. Please provide a prompt and factual answer to the following question. Answer as Plain Text, do not use Markdown or any other formatting.");
        llmPromptBuilder.AppendLine("---"); // Separator for clarity
        llmPromptBuilder.AppendLine($"question: {question}");
        string llmPrompt = llmPromptBuilder.ToString();
        var llmRawResponse = await GetLLMResponse(llmPrompt, "phi3:3.8b");
        return llmRawResponse;
    }

    public async Task<List<StackOverflowQuestion>?> RerankItemsWithLLMAsync(
        List<StackOverflowQuestion> items,
        string contextPrompt)
    {
        if (!items.Any()) return new List<StackOverflowQuestion>();
        StringBuilder llmPromptBuilder = new StringBuilder();
        llmPromptBuilder.AppendLine($"You are an expert in information ranking based on relevance. Your task is to re-rank a list of items based on the following criteria: {contextPrompt}.");
        llmPromptBuilder.AppendLine("Provide the re-ranked list of items by their original numerical ID in a JSON array. If an item is completely irrelevant or low quality, you can choose to omit it from the re-ranked list. Do not include any other text besides the JSON array.");
        llmPromptBuilder.AppendLine("---");
        llmPromptBuilder.AppendLine("Items to re-rank (ID | Description snippet):");
        foreach (var item in items)
        {
            long id = item.question_id;
            string description = item.title;
            string descriptionSnippet = description.Length > 200 ? description.Substring(0, 200) + "..." : description;
            llmPromptBuilder.AppendLine($"- ID: {id}, Question: {descriptionSnippet.Replace("\n", " ").Replace("\r", "")}");
        }
        llmPromptBuilder.AppendLine("---");
        llmPromptBuilder.AppendLine("Return only a JSON array of IDs in ranked order. No explanations or additional text required.");
        string llmPrompt = llmPromptBuilder.ToString();
        _logger.LogDebug("LLM Reranking Prompt for type  {Prompt}", llmPrompt);

        try
        {
            var llmRawResponse = await GetLLMResponse(llmPrompt, "phi3:3.8b");
            llmRawResponse = llmRawResponse.Trim();
            _logger.LogDebug("LLM Raw Response for type  {Response}", llmRawResponse);
            string cleanedResponse = ExtractJsonArray(llmRawResponse);
            List<long>? reRankedIds = null;
            try
            {
                reRankedIds = JsonSerializer.Deserialize<List<long>>(cleanedResponse);
            }
            catch (JsonException parseEx)
            {
                _logger.LogError(parseEx, "Failed to parse LLM response as JSON array of long IDs for type  Attempting to clean. Raw: {Response}", llmRawResponse);
            }

            if (reRankedIds == null || !reRankedIds.Any())
            {
                _logger.LogWarning("LLM re-ranking yielded no valid IDs for type .");
                return new List<StackOverflowQuestion>();
            }

            var orderedItems = new List<StackOverflowQuestion>();
            foreach (var id in reRankedIds)
            {
                var item = items.FirstOrDefault(i => i.question_id == id);
                if (item != null)
                {
                    orderedItems.Add(item);
                }
                else
                {
                    _logger.LogWarning("LLM returned ID {Id} which was not in the original list for type", id);
                }
            }
            return orderedItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during LLM re-ranking for type.");
            return null;
        }
    }


    private string ExtractJsonArray(string text)
    {
        int startIndex = text.IndexOf('[');
        int endIndex = text.LastIndexOf(']');
        if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
        {
            string jsonArray = text.Substring(startIndex, endIndex - startIndex + 1);
            try
            {
                var stringIds = JsonSerializer.Deserialize<List<string>>(jsonArray);
                if (stringIds != null)
                {
                    var longIds = stringIds.Select(id => long.Parse(id)).ToList();
                    return JsonSerializer.Serialize(longIds);
                }
            }
            catch
            {
                return jsonArray;
            }
        }
        return string.Empty;
    }
}