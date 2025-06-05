using Microsoft.AspNetCore.Mvc;
using e_learning.Models;
using e_learning.Service.Interfaces;

namespace e_learning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public PublicController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("contact")]
        public async Task<IActionResult> Contact([FromBody] ContactFormDto form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _emailService.SendContactFormEmailAsync(form);

            if (result)
                return Ok(new { message = "Message sent successfully." });
            else
                return StatusCode(500, new { error = "Failed to send email." });
        }

        [HttpGet("about")]
        public IActionResult About()
        {
            var content = "Framy is an e-learning platform helping learners gain skills through expert-led online courses.";
            return Ok(new { content });
        }
    }
}
