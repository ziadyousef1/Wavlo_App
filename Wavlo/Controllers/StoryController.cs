using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wavlo.DTOs;
using Wavlo.Services;

namespace Wavlo.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StoryController : ControllerBase
    {
        private readonly IStoryService _service;

        public StoryController(IStoryService service)
        {
            _service = service;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadStory([FromForm] CreateStoryDto dto)
        {
            var result = await _service.UploadStoryAsync(dto);
            if (result == null)
                return Unauthorized("You must be logged in.");

            return Ok(result);
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStory(Guid id)
        {
            var result = await _service.DeleteStoryAsync(id);

            if (!result)
                return NotFound("Story not found or you are not authorized to delete it.");

            return Ok("Story deleted successfully.");
        }

    }
}
