using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wavlo.DTOs;
using Wavlo.Models;
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

        [HttpGet("{storyId}")]
        public async Task<IActionResult> GetStory(Guid storyId)
        {
            var story = await _service.GetStoryByIdAsync(storyId);

            if (story == null)
            {
                return NotFound();
            }

            return Ok(story);
        }

        [HttpPost("{storyId}/view")]
        public async Task<IActionResult> ViewStory(Guid storyId)
        {
            var story = await _service.GetStoryByIdAsync(storyId);
            if (story == null)
            {
                return NotFound();
            }

            var userId = User.Identity.Name; 
            var storyView = new StoryView
            {
                StoryId = storyId,
                UserId = userId,
                ViewedAt = DateTime.UtcNow
            };

           
            await _service.AddStoryViewAsync(storyView);

            return Ok();
        }


        [HttpGet("{storyId}/viewers")]
        public async Task<IActionResult> GetViewers(Guid storyId)
        {
            var result = await _service.GetStoryViewersAsync(storyId);
            return Ok(result);
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
