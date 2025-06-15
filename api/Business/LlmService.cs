using System.Text;
using System.Text.Json;

public interface ILlmService
{
    Task<List<StackOverflowQuestion>?> RerankItemsWithLLMAsync(List<StackOverflowQuestion> items, string contextPrompt);
}

public class LlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LlmService> _logger;

    public LlmService(IHttpClientFactory httpClientFactory, ILogger<LlmService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OllamaClient");
        _logger = logger;
    }


    public async Task<List<StackOverflowQuestion>?> RerankItemsWithLLMAsync(
        List<StackOverflowQuestion> items,
        string contextPrompt)
    {
        if (!items.Any()) return new List<StackOverflowQuestion>();

        StringBuilder llmPromptBuilder = new StringBuilder();
        llmPromptBuilder.AppendLine($"You are an expert in information retrieval and ranking. Your task is to re-rank a list of items based on the following criteria: {contextPrompt}.");
        llmPromptBuilder.AppendLine("Provide the re-ranked list of items by their original numerical ID in a JSON array. If an item is completely irrelevant or low quality, you can choose to omit it from the re-ranked list. Do not include any other text besides the JSON array.");
        llmPromptBuilder.AppendLine("---");
        llmPromptBuilder.AppendLine("Items to re-rank (ID | Description snippet):");

        foreach (var item in items)
        {
            long id = item.question_id;
            string description = item.title;
            string descriptionSnippet = description.Length > 200 ? description.Substring(0, 200) + "..." : description;
            llmPromptBuilder.AppendLine($"- ID: {id}, Description: {descriptionSnippet.Replace("\n", " ").Replace("\r", "")}");
        }
        llmPromptBuilder.AppendLine("---");
        llmPromptBuilder.AppendLine("Please provide a JSON array of IDs in the new re-ranked order. Example: [123, 456, 789]");

        string llmPrompt = llmPromptBuilder.ToString();
        _logger.LogDebug("LLM Reranking Prompt for type  {Prompt}", llmPrompt);

        try
        {
            // Prepare the REST call payload
            var payload = new
            {
                model = "qwen2:0.5B",
                prompt = llmPrompt
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://localhost:11434/api/generate", requestContent);
            response.EnsureSuccessStatusCode();

            // Read the response as a stream and extract the 'response' property
            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var jsonDoc = await JsonDocument.ParseAsync(responseStream);
            var root = jsonDoc.RootElement;

            string llmRawResponse = root.TryGetProperty("response", out var responseElement)
                ? responseElement.GetString() ?? ""
                : "";
            llmRawResponse = llmRawResponse.Replace("\n", "").Trim();

            if (string.IsNullOrWhiteSpace(llmRawResponse))
            {
                _logger.LogWarning("LLM returned empty or null response for re-ranking type ");
                return null;
            }

            _logger.LogDebug("LLM Raw Response for type  {Response}", llmRawResponse);

            List<long>? reRankedIds = null;
            try
            {
                reRankedIds = JsonSerializer.Deserialize<List<long>>(llmRawResponse);
            }
            catch (JsonException parseEx)
            {
                _logger.LogError(parseEx, "Failed to parse LLM response as JSON array of long IDs for type  Attempting to clean. Raw: {Response}", llmRawResponse);
                string cleanedResponse = ExtractJsonArray(llmRawResponse); // Reusing the helper
                if (!string.IsNullOrEmpty(cleanedResponse))
                {
                    try
                    {
                        reRankedIds = JsonSerializer.Deserialize<List<long>>(cleanedResponse);
                    }
                    catch (JsonException cleanParseEx)
                    {
                        _logger.LogError(cleanParseEx, "Failed to parse cleaned LLM response as JSON array of long IDs for type Cleaned: {Response}", cleanedResponse);
                    }
                }
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
            return text.Substring(startIndex, endIndex - startIndex + 1);
        }
        return string.Empty;
    }
}