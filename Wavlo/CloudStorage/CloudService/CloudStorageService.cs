using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Wavlo.CloudStorage.CloudDTOs;
using Wavlo.CloudStorage.StorageSettings;

namespace Wavlo.CloudStorage.CloudService
{
    public class CloudStorageService : ICloudStorageService
    {
        private readonly CloudStorageSettings _cloud;
        private readonly BlobServiceClient _blob;
        public CloudStorageService(CloudStorageSettings cloud , BlobServiceClient blob)
        {
            _cloud = cloud;
            _blob = blob;
            
        }
        public async Task<BlobResponseDTO> UploadAsync(string containerName, IFormFile file, string userId)
        {
            var blobContainer = _blob.GetBlobContainerClient(containerName);
            var blobName = $"{userId}-{file.FileName}";
            var blobClient = blobContainer.GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };
            await blobClient.UploadAsync(file.OpenReadStream(), new BlobUploadOptions { HttpHeaders = blobHttpHeaders });

            return new BlobResponseDTO
            {
                Message = "File Uploaded Successfully!",
                IsSuccess = true,
                Blob = new BlobDTO
                {
                    FileUrl = blobClient.Uri.AbsoluteUri,
                    FileName = blobClient.Name,
                }
            };
        }
        public async Task<BlobResponseDTO> DownloadAsync(string fileName)
        {
            var blobName = fileName;
            var blobContainer = _blob.GetBlobContainerClient(_cloud.Containers.Files);
            var blobClient = blobContainer.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                return new BlobResponseDTO
                {
                    IsSuccess = false,
                    Message = $"File {fileName} not found",
                    Blob = null
                };
            }

            var properties = await blobClient.GetPropertiesAsync();

            var response = await blobClient.DownloadAsync();
            var blobDownloadInfo = response.Value;

            var memoryStream = new MemoryStream();
            await blobDownloadInfo.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return new BlobResponseDTO
            {
                IsSuccess = true,
                Message = "File downloaded successfully",
                Blob = new BlobDTO
                {
                    FileStream = memoryStream,
                    ContentType = properties.Value.ContentType,
                    FileName = fileName,
                    FileUrl = blobClient.Uri.AbsoluteUri,

                }
            };


        }

        public async Task<BlobResponseDTO> DownloadAsync(string containerName, string filename)
        {
            var blobName = filename;
            var blobContainer = _blob.GetBlobContainerClient(containerName);
            var blobClient = blobContainer.GetBlobClient(blobName);
            using (var stream = new MemoryStream())
            {
                await blobClient.DownloadToAsync(stream);
                stream.Position = 0;
                return new BlobResponseDTO
                {
                    Message = "File downloaded successfully",
                    IsSuccess = true,
                    Blob = new BlobDTO
                    {
                        FileStream = stream,
                        FileName = blobClient.Name,
                        ContentType = blobClient.GetProperties().Value.ContentType,
                        FileUrl = blobClient.Uri.AbsoluteUri
                    }
                };
            }
        }
        public async Task<BlobResponseDTO> DeleteAsync(string containerName, string fileName)
        {
            var blobName = fileName;
            var blobContainer = _blob.GetBlobContainerClient(containerName);
            var blobClient = blobContainer.GetBlobClient(blobName);
            var response = await blobClient.DeleteIfExistsAsync();

            return new BlobResponseDTO
            {
                Message = response ? "File deleted successfully" : "File not found",
                IsSuccess = response

            };
        }  
    }
}
