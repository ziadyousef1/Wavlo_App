using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wavlo.CloudStorage.CloudService;

namespace Wavlo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly ICloudStorageService _cloud;
        public StorageController(ICloudStorageService cloud)
        {
            _cloud = cloud;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            var result = await _cloud.UploadAsync("files",file,"userId");
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("video/{fileName}")]
        public async Task<IActionResult> GetVideo(string fileName)
        {
            var result = await _cloud.DownloadAsync(fileName);
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }
            return File(result.Blob.FileStream.ToArray(), result.Blob.ContentType, enableRangeProcessing: true);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteVideo(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return BadRequest("Filename is required.");
            }

            var result = await _cloud.DeleteAsync("files", filename);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
