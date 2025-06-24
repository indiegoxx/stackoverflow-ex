public interface ILlmService
{
    Task<string> GetLLMResponseOllama(string llmPrompt);
    Task<string> GetLLMResponseMlx(string llmPrompt,int maxTokens);
}