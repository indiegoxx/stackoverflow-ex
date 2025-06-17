using System.Text.Json;
namespace Business
{
    
    public interface IQuestionService
    {
        Task<StackExchangeResponse> GetSimilarQuestionsAsync(string title);
        Task<StackExchangeResponse> GetSimilarQuestionsRankedAsync(string title);
        Task<string?> GetLlmSuggestedAnswer(string question);
        Task<List<RecentQuestion>> GetCachedQuestionsAsync();
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

        public async Task<List<RecentQuestion>> GetCachedQuestionsAsync()
        {
            return await _cache.GetListAsync<RecentQuestion>("recentQuestion");
        }

        public async Task<string?> GetLlmSuggestedAnswer(string question)
        {
            return await _llmService.LLMSuggestedAnswer(question);
        }


        public async Task<StackExchangeResponse> GetSimilarQuestionsAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title query parameter is required.", nameof(title));

            var cacheKey = $"similar_questions_{title.Trim().ToLower()}";
            var cachedResult = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedResult))
            {
                return JsonSerializer.Deserialize<StackExchangeResponse>(cachedResult);
            }

            var encodedTitle = Uri.EscapeDataString(title);
            var apiUrl = $"https://api.stackexchange.com/2.3/search/advanced?order=desc&sort=relevance&q={encodedTitle}&site=stackoverflow";

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0 (https://example.com)");

            var response = await httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Failed to fetch data from Stack Overflow API.");

            var content = await response.Content.ReadAsStringAsync();

            await _cache.SetStringAsync(cacheKey, content, TimeSpan.FromMinutes(5));

            RecentQuestion cacheEntry = new RecentQuestion { Timestamp = DateTime.UtcNow, Title = title };
            await _cache.PushToListAsync("recentQuestion",cacheEntry);
            return JsonSerializer.Deserialize<StackExchangeResponse>(content);
        }

        public async Task<StackExchangeResponse> GetSimilarQuestionsRankedAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title query parameter is required.", nameof(title));

            var cacheKey = $"similar_questions_{title.Trim().ToLowerInvariant()}";
            var cachedResultJson = await _cache.GetStringAsync(cacheKey);
            StackExchangeResponse? initialSoResponse = null;
            try
            {
                initialSoResponse = JsonSerializer.Deserialize<StackExchangeResponse>(cachedResultJson);
            }
            catch
            {
                initialSoResponse = await GetSimilarQuestionsAsync(title);
            }

            List<StackOverflowQuestion>? questionsToRerank = initialSoResponse?.items;

            if (!questionsToRerank.Any())
            {
                return new StackExchangeResponse();
            }

            var reRankedQuestions = await _llmService.RerankItemsWithLLMAsync(
                questionsToRerank,
                title
                );

            StackExchangeResponse finalResponse;
            if (reRankedQuestions != null)
            {
                finalResponse = new StackExchangeResponse { items = reRankedQuestions };
            }
            else
            {
                finalResponse = initialSoResponse;
            }

            if (finalResponse.items != null && finalResponse.items.Any())
            {
                var finalResultJson = JsonSerializer.Serialize(finalResponse);
                await _cache.SetStringAsync(cacheKey, finalResultJson, TimeSpan.FromMinutes(10));
            }
            return finalResponse;
        }

    }

}