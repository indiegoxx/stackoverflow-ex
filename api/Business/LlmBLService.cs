using System.Text;
using System.Text.Json;
using Business;

public interface ILlmBLService
{
    Task<List<StackOverflowQuestion>?> RerankItemsWithLLMAsync(List<StackOverflowQuestion> items, string contextPrompt);
    Task<string?> LLMSuggestedAnswer(string question);
}

public class LlmBLService : ILlmBLService
{
    private readonly ICacheLoggerService _logger;
    private readonly ILlmService _llmService;
    private record OllamaResponse
    {
        public string? response { get; init; }
    }

    public LlmBLService(ICacheLoggerService logger, ILlmService llmService)
    {
        _logger = logger;
        _llmService = llmService;
    }


    public async Task<string?> LLMSuggestedAnswer(string question)
    {
        StringBuilder llmPromptBuilder = new StringBuilder();
        llmPromptBuilder.AppendLine($"You are an expert question and answer assistant in context of Programming & Software Development. Please provide a prompt and factual answer to the following question. Answer as Plain Text, do not use Markdown or any other formatting.");
        llmPromptBuilder.AppendLine("---"); // Separator for clarity
        llmPromptBuilder.AppendLine($"question: {question}");
        string llmPrompt = llmPromptBuilder.ToString();
        var llmRawResponse = await _llmService.GetLLMResponseMlx(llmPrompt,100);
        return llmRawResponse;
    }
    public async Task<List<StackOverflowQuestion>?> RerankItemsWithLLMAsync(
        List<StackOverflowQuestion> items,
        string baseQuestion)
    {
        if (!items.Any()) return new List<StackOverflowQuestion>();

        var scoringTasks = new List<Task<(long questionId, int score)>>();
        var semaphore = new SemaphoreSlim(15, 15); // Limit concurrent calls to 15

        // Create scoring tasks for each question
        foreach (var item in items)
        {
            scoringTasks.Add(ScoreQuestionRelevanceAsync(item, baseQuestion, semaphore));
        }

        try
        {
            // Wait for all scoring tasks to complete
            var scores = await Task.WhenAll(scoringTasks);

            // Create dictionary of question ID to score
            var scoresDictionary = scores.ToDictionary(s => s.questionId, s => s.score);

            _logger.LogInfo("LLM Scoring Results: {Scores}",
                string.Join(", ", scoresDictionary.Select(kvp => $"ID:{kvp.Key}={kvp.Value}")));

            // Sort questions by score (highest first) and return
            var sortedItems = items
                .Where(item => scoresDictionary.ContainsKey(item.question_id) && scoresDictionary[item.question_id] > 0)
                .OrderByDescending(item => scoresDictionary[item.question_id])
                .ToList();

            // Set the relevanceScore property for each item
            foreach (var item in sortedItems)
            {
                if (scoresDictionary.TryGetValue(item.question_id, out var score))
                {
                    item.relevanceScore = score.ToString();
                }
                else
                {
                    item.relevanceScore = "0";
                }
            }

            return sortedItems;
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred during LLM scoring.", ex);
            return null;
        }
    }

    private async Task<(long questionId, int score)> ScoreQuestionRelevanceAsync(
        StackOverflowQuestion question,
        string baseQuestion,
        SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();

        try
        {
            string questionSnippet = question.title;

            StringBuilder promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("You are an expert at measuring question similarity and relevance.");
            promptBuilder.AppendLine($"Base Question='{baseQuestion}'");
            promptBuilder.AppendLine($"Compare Question='{questionSnippet.Replace("\n", " ").Replace("\r", "")}'");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Rate the similarity and relevance of the 'Compare Question' to the 'Base Question' on a scale of 0-100:");
            promptBuilder.AppendLine("- 0-20: Completely unrelated");
            promptBuilder.AppendLine("- 21-40: Somewhat related but different focus");
            promptBuilder.AppendLine("- 41-60: Related with some overlap");
            promptBuilder.AppendLine("- 61-80: Highly related with significant overlap");
            promptBuilder.AppendLine("- 81-100: Very similar or identical intent");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Return ONLY the numerical score (0-100). No explanations or additional text.");

            string prompt = promptBuilder.ToString();

            _logger.LogInfo($"LLM Scoring Prompt for Question ID {question.question_id}: {prompt}");

            var llmResponse = await _llmService.GetLLMResponseMlx(prompt, 3);
            string cleanedResponse = llmResponse.Trim();

            _logger.LogInfo($"LLM Raw Scoring Response for Question ID {question.question_id}: {cleanedResponse}");

            // Extract numerical score from response
            int score = ExtractScoreFromResponse(cleanedResponse);

            _logger.LogInfo($"Extracted Score for Question ID {question.question_id}: {score}");

            return (question.question_id, score);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to score question ID  ${question.question_id}", ex);
            return (question.question_id, 0); // Return 0 score on error
        }
        finally
        {
            semaphore.Release();
        }
    }

    private int ExtractScoreFromResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return 0;

        // Try to extract number from response
        var match = System.Text.RegularExpressions.Regex.Match(response, @"\b(\d{1,3})\b");

        if (match.Success && int.TryParse(match.Groups[1].Value, out int score))
        {
            // Ensure score is within valid range
            return Math.Max(0, Math.Min(100, score));
        }

        // If no valid number found, try parsing the entire response
        if (int.TryParse(response.Trim(), out int directScore))
        {
            return Math.Max(0, Math.Min(100, directScore));
        }
        _logger.LogError($"Could not extract valid score from LLM response: ", new ArgumentException("Invalid LLM response format", response));
        return 0;
    }

}