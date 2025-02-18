using TikTokToFacebook.Helper;
using TikTokToFacebook.Utils;

namespace TikTokToFacebook
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create Services using Dependency Injection
            var httpClient = ServiceHelper.CreateHttpClient();
            var tikTokService = ServiceHelper.CreateTikTokService(httpClient);
            var videoService = ServiceHelper.CreateVideoService();
            var facebookService = ServiceHelper.CreateFacebookService();
            var databaseService = ServiceHelper.CreateDatabaseService();

            // Create Video Processor
            var videoProcessor = new VideoProcessor(tikTokService, videoService, facebookService, databaseService);

            // Start Processing Videos
            await videoProcessor.ProcessVideosAsync(AppConfig.User, AppConfig.VideoQuantity);
        }
    }
}
