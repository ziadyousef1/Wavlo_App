using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wavlo.DTOs;
using Wavlo.Services;

namespace Wavlo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoryController : ControllerBase
    {
        private readonly StoryService _service;
        public StoryController(StoryService service)
        {
            _service = service;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadStory([FromForm] CreateStoryDto dto)
        {
            var result = await _service.UploadStoryAsync(dto);
            if (!result)
                return Unauthorized("You must be logged in.");

            return Ok("Story uploaded successfully.");
        }

        [HttpGet("stories")]
        public async Task<IActionResult> GetActiveStories()
        {
            var stories = await _service.GetActiveStoriesAsync();
            return Ok(stories);
        }

        [HttpDelete("cleanup")]
        public async Task<IActionResult> CleanupExpiredStories()
        {
            await _service.CleanupExpiredStoriesAsync();
            return Ok(new { message = "Expired stories cleaned up successfully." });
        }
    }
}
