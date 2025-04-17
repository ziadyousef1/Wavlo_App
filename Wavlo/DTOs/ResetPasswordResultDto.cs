namespace Wavlo.DTOs
{
    public class ResetPasswordResultDto
    {
        public string Message { get; set; }
        public bool IsSucceeded { get; set; }
        public List<string>? Errors { get; set; }
    }
}
