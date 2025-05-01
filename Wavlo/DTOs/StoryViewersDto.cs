namespace Wavlo.DTOs
{
    public class StoryViewersDto
    {
        public int Count { get; set; }
        public List<ViewerDto> Viewers { get; set; } = new();
    }
}
