using Wavlo.CloudStorage.CloudDTOs;

namespace Wavlo.CloudStorage.CloudService
{
    public interface ICloudStorageService
    {
        Task<BlobResponseDTO> UploadAsync(string containerName, IFormFile file, string userId);
        Task<BlobResponseDTO> DeleteAsync(string containerName, string fileName);
        Task<BlobResponseDTO> DownloadAsync(string fileName);
    }
}
