using System.Diagnostics;
using TikTokToFacebook.Utils;

namespace TikTokToFacebook.Services
{
    public class VideoService : IVideoService
    {
        public async Task DownloadVideoAsync(string videoUrl, string outputPath)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(videoUrl);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = File.Create(outputPath);
            await stream.CopyToAsync(fileStream);
        }

        public void RunFfmpeg(string inputVideo, string outputVideo, string watermark)
        {
            Utils.Utils.KillProcessByName("ffmpeg");
            string ffmpegPath = @"C:\ffmpeg-master-latest-win64-gpl-shared\bin\ffmpeg.exe";
            string arguments = $"-i \"{inputVideo}\" -vf \"drawtext=text='{watermark}':fontcolor=yellow:fontsize=16:x=10:y=10\" -codec:a copy \"{outputVideo}\"";

            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = processInfo })
            {
                process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
        }
    }
}
