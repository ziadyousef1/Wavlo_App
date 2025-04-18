using System.Text.Json.Serialization;

namespace Wavlo.CloudStorage.CloudDTOs
{
    public class BlobDTO
    {
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        [JsonIgnore]
        public MemoryStream? FileStream { get; set; }
        public string? ContentType { get; set; }
    }
}
