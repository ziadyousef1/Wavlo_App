namespace Wavlo.Services
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file);
    }
}
