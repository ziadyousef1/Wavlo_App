using Wavlo.DTOs;

namespace Wavlo.Services
{
    public interface IStoryService
    {
        public Task<bool> UploadStoryAsync(CreateStoryDto dto);

        public Task<List<StoryResponseDto>> GetActiveStoriesAsync();

        public Task CleanupExpiredStoriesAsync();

    }
}
