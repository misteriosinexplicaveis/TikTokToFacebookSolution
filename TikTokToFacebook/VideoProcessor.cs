using TikTokToFacebook.Services;
using TikTokToFacebook.Utils;

namespace TikTokToFacebook
{
    public class VideoProcessor
    {
        private readonly ITikTokService _tikTokService;
        private readonly IVideoService _videoService;
        private readonly IFacebookService _facebookService;
        private readonly IDatabaseService _databaseService;

        public VideoProcessor(ITikTokService tikTokService, IVideoService videoService, IFacebookService facebookService, IDatabaseService databaseService)
        {
            _tikTokService = tikTokService;
            _videoService = videoService;
            _facebookService = facebookService;
            _databaseService = databaseService;
        }

        public async Task ProcessVideosAsync(string user, string qty)
        {
            try
            {
                // Get User's SecUid
                string secUid = await _tikTokService.GetUsersSecUidAsync(user);
                Console.WriteLine($"SecUid: {secUid}");

                // Get User's Videos
                var posts = await _tikTokService.GetUserPostsAsync(secUid, qty);
                if (posts.Data.ItemList == null || posts.Data.ItemList.Count == 0)
                {
                    Console.WriteLine("No videos found.");
                    return;
                }

                foreach (var post in posts.Data.ItemList)
                {
                    string videoId = post.Id;
                    long createTime = post.CreateTime;

                    if (_databaseService.RecordExists(videoId, user))
                    {
                        Console.WriteLine($"Skipping Video {videoId}, already processed.");
                        continue;
                    }

                    // Get video URL
                    var tiktok = await _tikTokService.GetTikTokVideoUrl(user, videoId);
                    var videoUrl = tiktok.Data.Url;

                    Console.WriteLine($"Downloading Video {videoId}: {videoUrl}");

                    // Download Video
                    string outputPath = $"video_{videoId}.mp4";
                    await _videoService.DownloadVideoAsync(videoUrl, outputPath);

                    // Process Video with FFmpeg
                    string outputVideo = $"video_{videoId}_processed.mp4";
                    _videoService.RunFfmpeg(outputPath, outputVideo, AppConfig.Watermark);

                    // Upload to Facebook
                    var desc =  Utils.Utils.TruncateString(tiktok.Data.Desc, 255);
                    var title = Utils.Utils.RemoveWordsWithStartWithHashes(desc);

                    bool uploadSuccess = await _facebookService.UploadVideoAsync(outputVideo, desc, title);
                    if (uploadSuccess)
                    {
                        Console.WriteLine($"Uploaded Video {videoId} Successfully!");

                        // Store in database
                        _databaseService.InsertRecord(videoId, createTime, user, "Success");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to upload video {videoId}.");
                        _databaseService.InsertRecord(videoId, createTime, user, "Failed");
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
}