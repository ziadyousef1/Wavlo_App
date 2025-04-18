namespace Wavlo.CloudStorage.CloudDTOs
{
    public class BlobResponseDTO
    {
        public BlobResponseDTO()
        {
            Blob = new BlobDTO();
        }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public BlobDTO Blob { get; set; }
    }
}
