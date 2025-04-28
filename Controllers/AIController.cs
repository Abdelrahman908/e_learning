using e_learning.Service;
using Microsoft.AspNetCore.Mvc;

namespace e_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly RecommendationService _recommendationService;

        public AIController(RecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpPost("recommend")]
        public async Task<IActionResult> Recommend([FromBody] object input)
        {
            var result = await _recommendationService.RecommendAsync(input);
            return Ok(new { result });
        }
    }

}
