namespace Wavlo.Services
{
    public class StoryCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1); 

        public StoryCleanupBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var storyService = scope.ServiceProvider.GetRequiredService<IStoryService>();
                    await storyService.CleanupExpiredStoriesAsync();
                }
                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
