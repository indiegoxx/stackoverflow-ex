using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Business
{
    public interface IQuestionService
    {
        Task<StackExchangeResponse> GetSimilarQuestionsAsync(string title);
        Task<StackExchangeResponse> GetSimilarQuestionsRankedAsync(string title);
    }

    public class QuestionService : IQuestionService
    {
        private readonly ICache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILlmService _llmService;

        public QuestionService(ICache cache, IHttpClientFactory httpClientFactory, ILlmService llmService)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _llmService = llmService;
        }

        public async Task<StackExchangeResponse> GetSimilarQuestionsAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title query parameter is required.", nameof(title));

            var cacheKey = $"similar_questions_{title.Trim().ToLower()}";
            var cachedResult = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedResult))
            {
                // Deserialize the cached JSON string to StackExchangeResponse
                return System.Text.Json.JsonSerializer.Deserialize<StackExchangeResponse>(cachedResult);
            }

            var encodedTitle = Uri.EscapeDataString(title);
            var apiUrl = $"https://api.stackexchange.com/2.3/search?order=desc&sort=activity&intitle={encodedTitle}&site=stackoverflow";

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0 (https://example.com)");

            var response = await httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Failed to fetch data from Stack Overflow API.");

            var content = await response.Content.ReadAsStringAsync();

            // Cache the result for 5 minutes
            await _cache.SetStringAsync(cacheKey, content, TimeSpan.FromMinutes(5));

            return System.Text.Json.JsonSerializer.Deserialize<StackExchangeResponse>(content);
        }

        public async Task<StackExchangeResponse> GetSimilarQuestionsRankedAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title query parameter is required.", nameof(title));

            var cacheKey = $"similar_questions_ranked_{title.Trim().ToLowerInvariant()}"; // More specific cache key
            var cachedResultJson = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedResultJson))
            {
                // return JsonSerializer.Deserialize<StackExchangeResponse>(cachedResultJson) ?? new StackExchangeResponse();
            }


            // 1. Fetch initial questions from Stack Overflow API
            // Use the 'similar' endpoint to get questions similar to a provided title.
            // Example: https://api.stackexchange.com/2.3/similar?order=desc&sort=relevance&title=your+title&site=stackoverflow&filter=default
            var encodedTitle = Uri.EscapeDataString(title);
            var apiUrl = $"https://api.stackexchange.com/2.3/search?order=desc&sort=activity&intitle={encodedTitle}&site=stackoverflow";

            StackExchangeResponse? initialSoResponse = null;
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0 (https://example.com)");
                var response = await httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode(); // Throws on HTTP error codes

                var jsonResponse = await response.Content.ReadAsStringAsync();
                initialSoResponse = JsonSerializer.Deserialize<StackExchangeResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return new StackExchangeResponse();
            }


            List<StackOverflowQuestion>? questionsToRerank = initialSoResponse?.items;

            if (!questionsToRerank.Any())
            {
                return new StackExchangeResponse();
            }

            // For LLM re-ranking, we need an "original question" to compare against.
            // Since `similar` API gives related questions, we can synthesize an "original"
            // based on the `title` provided by the user, or fetch the top result from a search.
            // Let's assume the `title` itself is the context for the "original question".
            // If you actually search for the 'title' first and get a top `StackOverflowQuestion`
            // that would be better. For simplicity, we'll create a dummy one here.
            
            var reRankedQuestions = await _llmService.RerankItemsWithLLMAsync(
                questionsToRerank,
                title // Pass the context of the user's search
                );

            StackExchangeResponse finalResponse;
            if (reRankedQuestions != null)
            {
                finalResponse = new StackExchangeResponse { items = reRankedQuestions }; // Assume no more if LLM processed all
            }
            else
            {
                finalResponse = initialSoResponse; // Fallback to original if LLM fails
            }

            // 3. Cache the final re-ranked result
            if (finalResponse.items != null && finalResponse.items.Any())
            {
                var finalResultJson = JsonSerializer.Serialize(finalResponse);
                await _cache.SetStringAsync(cacheKey, finalResultJson, TimeSpan.FromMinutes(10)); // Cache for 10 minutes
            }

            return finalResponse;
        }



    }

}