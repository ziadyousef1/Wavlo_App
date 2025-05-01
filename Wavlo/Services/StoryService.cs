using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wavlo.Data;
using Wavlo.DTOs;
using Wavlo.Models;

namespace Wavlo.Services
{
    public class StoryService : IStoryService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ChatDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StoryService(IWebHostEnvironment env, ChatDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<StoryUploadDto?> UploadStoryAsync(CreateStoryDto dto)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "stories");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.MediaFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.MediaFile.CopyToAsync(stream);
            }

            var story = new Story
            {
                UserId = userId,
                MediaUrl = $"/stories/{fileName}",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            _context.Stories.Add(story);
            await _context.SaveChangesAsync();

            return new StoryUploadDto
            {
                UserId = userId,
                MediaUrl = story.MediaUrl
            };
        }

        public async Task<List<StoryResponseDto>> GetActiveStoriesAsync()
        {
            var now = DateTime.UtcNow;

            var stories = await _context.Stories
                .Include(s => s.User)
                    .ThenInclude(u => u.UserImages)
                .Where(s => s.ExpiresAt > now)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var response = stories.Select(s => new StoryResponseDto
            {
                UserId = s.UserId,
                UserName = $"{s.User.FirstName} {s.User.LastName}",
                ProfileImageUrl = s.User.UserImages.FirstOrDefault()?.ImageUrl,
                MediaUrl = s.MediaUrl,
                CreatedAt = s.CreatedAt
            }).ToList();

            return response;
        }
        public async Task<Story> GetStoryByIdAsync(Guid storyId)
        {
            return await _context.Stories
                .Where(s => s.Id == storyId)
                .FirstOrDefaultAsync();
        }

        public async Task AddStoryViewAsync(StoryView storyView)
        {
            _context.StoryViews.Add(storyView);
            await _context.SaveChangesAsync();
        }

        public async Task<StoryViewersDto> GetStoryViewersAsync(Guid storyId)
        {
            var viewers = await _context.StoryViews
        .Where(v => v.StoryId == storyId)
        .Include(v => v.User)
        .Select(v => new ViewerDto
        {
            Id = v.User.Id,
            Username = v.User.UserName,
            ViewedAt = v.ViewedAt 
        })
        .ToListAsync();

            return new StoryViewersDto
            {
                Count = viewers.Count,
                Viewers = viewers
            };
        }

        public async Task CleanupExpiredStoriesAsync()
        {
            var now = DateTime.UtcNow;
            var expired = await _context.Stories
                .Where(s => s.ExpiresAt <= now)
                .ToListAsync();

            if (expired.Any())
            {
                _context.Stories.RemoveRange(expired);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> DeleteStoryAsync(Guid storyId)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return false;

            var story = await _context.Stories.FirstOrDefaultAsync(s => s.Id == storyId && s.UserId == userId);
            if (story == null) return false;

            
            var fullPath = Path.Combine(_env.WebRootPath, story.MediaUrl.TrimStart('/'));
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            _context.Stories.Remove(story);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
