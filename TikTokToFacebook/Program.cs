using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TikTokToFacebook.Model;
using TikTokToFacebook.Services;
using TikTokToFacebook.Utils;


class Program
{
    static async Task Main(string[] args)
    {
        // Configuration
        string watermark = "https\\://www.facebook.com/misteriosinexplicaveisoficial";
        string user = "@momentocuriosos";
        string qty = "15";
        string apiKey = "a06d1bb238mshcfa03782743f37bp1d64c0jsn0703764c6935";
        string apiHost = "tiktok-api23.p.rapidapi.com";
        string facebookPageId = "2789468501067417";
        string facebookAccessToken = "EAAJCfA3UHIQBOzyHSVZCOwBC1TAtaAcDEX4zEthBZBHrygETLMGblyAxDAWX2arhnt5V37n4MgdmPgzaJKXgSfmL4VYjINeicqQqdismhCuXE4Fyksj27i0hudF94WPdiaXj9j9lSAWVg2UCqSluxr65OHSTbEcXWwQMX2aBAgaGZAZCxvm74Lwv3q8uyAZDZD";
        string connectionString = "server=localhost;database=tiktok;user=tiktok;password=Jid4N7!SL80pEd;";

        // Dependency Injection
        var httpClient = new HttpClient();
        ITikTokService tikTokService = new TikTokService(httpClient, apiKey, apiHost);
        IVideoService videoService = new VideoService();
        IFacebookService facebookService = new FacebookService(facebookPageId, facebookAccessToken);
        IDatabaseService databaseService = new DatabaseService(connectionString);

        try
        {
            // Get User's SecUid
            string secUid = await tikTokService.GetUsersSecUidAsync(user);
            Console.WriteLine($"SecUid: {secUid}");

            // Get User's Videos
            var posts = await tikTokService.GetUserPostsAsync(secUid, qty);
            
            if (posts.Data.ItemList == null || posts.Data.ItemList.Count == 0)
            {
                Console.WriteLine("No videos found.");
                return;
            }

            foreach (var post in posts.Data.ItemList)
            {
                string videoId = post.Id;
                long createTime = post.CreateTime;

                // Check if video is already processed
                if (databaseService.RecordExists(videoId, user))
                {
                    Console.WriteLine($"Skipping Video {videoId}, already processed.");
                    continue;
                }

                // Get video URL
                var tiktok = await tikTokService.GetTikTokVideoUrl(user, videoId);
                var videoUrl = tiktok.Data.Url;

                Console.WriteLine($"Downloading Video {videoId}: {videoUrl}");

                // Download Video
                string outputPath = $"video_{videoId}.mp4";
                await videoService.DownloadVideoAsync(videoUrl, outputPath);

                // Process Video with FFmpeg
                string outputVideo = $"video_{videoId}_processed.mp4";
                videoService.RunFfmpeg(outputPath, outputVideo, watermark);

                // Upload to Facebook
                var desc = tiktok.Data.Desc;
                var title = Utils.RemoveWordsStartingWithHash(desc);
                bool uploadSuccess = await facebookService.UploadVideoAsync(outputVideo, desc, title);
                if (uploadSuccess)
                {
                    Console.WriteLine($"Uploaded Video {videoId} Successfully!");

                    // Store in database
                    databaseService.InsertRecord(videoId, createTime, user);
                }
                else
                {
                    Console.WriteLine($"Failed to upload video {videoId}.");
                }

                // Clean up
                File.Delete(outputPath);
                File.Delete(outputVideo);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}