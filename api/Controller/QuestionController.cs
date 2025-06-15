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
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return BadRequest("Title query parameter is required.");
                }

                var result = await _questionService.GetSimilarQuestionsAsync(title);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("ranked")]
        public async Task<ActionResult<StackExchangeResponse>> GetRankedQuestions([FromQuery] string title)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return BadRequest("Title query parameter is required.");
                }

                var result = await _questionService.GetSimilarQuestionsRankedAsync(title);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
