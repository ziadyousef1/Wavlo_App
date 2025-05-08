using Wavlo.DTOs;
using Wavlo.Models;

namespace Wavlo.Services
{
    public interface IStoryService
    {
        public Task<StoryUploadDto?> UploadStoryAsync(CreateStoryDto dto);

        public Task<List<StoryResponseDto>> GetActiveStoriesAsync();

      //  public Task AddStoryViewAsync(StoryView storyView);
      //  public Task<StoryViewersDto> GetStoryViewersAsync(Guid storyId);
        public Task<Story> GetStoryByIdAsync(Guid storyId);

        public Task CleanupExpiredStoriesAsync();

        public Task<bool> DeleteStoryAsync(Guid storyId);

    }
}
