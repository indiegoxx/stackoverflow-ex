using Business;
using Microsoft.AspNetCore.Mvc;

namespace api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        IQuestionService _questionService;
        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }
        // GET: api/Question/similar?title=your+question+title
        [HttpGet("similar")]
        public async Task<ActionResult<StackExchangeResponse>> GetSimilarQuestions([FromQuery] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Title query parameter is required.");
            }

            var result = await _questionService.GetSimilarQuestionsAsync(title);
            return Ok(result);
        }

        [HttpGet("ranked")]
        public async Task<ActionResult<StackExchangeResponse>> GetRankedQuestions([FromQuery] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Title query parameter is required.");
            }
            var result = await _questionService.GetSimilarQuestionsRankedAsync(title);
            return Ok(result);
        }

        [HttpGet("recent")]
        public async Task<ActionResult<List<RecentQuestion>>> GetRecentQuestions()
        {
            var result = await _questionService.GetCachedQuestionsAsync();
            if (result == null || result.Count == 0)
            {
                return NotFound("No recent questions found.");
            }
            return Ok(result);
        }

        [HttpGet("suggested-answer")]
        public async Task<ActionResult<string>> GetSuggestedAnswer([FromQuery] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return BadRequest("Question query parameter is required.");
            }
            var result = await _questionService.GetLlmSuggestedAnswer(question);
            if (result == null)
            {
                return NotFound("No suggested answer found.");
            }
            return Ok(new { answer = result });
        }
    }
}
