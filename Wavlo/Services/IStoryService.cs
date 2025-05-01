using Wavlo.DTOs;

namespace Wavlo.Services
{
    public interface IStoryService
    {
        public Task<StoryUploadDto?> UploadStoryAsync(CreateStoryDto dto);

        public Task<List<StoryResponseDto>> GetActiveStoriesAsync();

        public Task CleanupExpiredStoriesAsync();

    }
}
