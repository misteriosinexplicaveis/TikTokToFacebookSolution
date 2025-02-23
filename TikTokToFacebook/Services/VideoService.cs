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
            Utils.Utils.KillProcessByName("ffmpeg");
            await using var fileStream = File.Create(outputPath);

            await stream.CopyToAsync(fileStream);
        }

        public void RunFfmpeg(string inputVideo, string outputVideo, string watermark)
        {
            try
            {
                // Ensure previous FFmpeg processes are not running
                Utils.Utils.KillProcessByName("ffmpeg");

                // Wait until input file exists
                if (!FileUtils.WaitForFile(inputVideo))
                {
                    Console.WriteLine($"Error: Input file '{inputVideo}' not found within timeout.");
                    return;
                }

                string ffmpegPath = @"C:\ffmpeg-master-latest-win64-gpl-shared\bin\ffmpeg.exe";
                if (!File.Exists(ffmpegPath))
                {
                    Console.WriteLine("Error: FFmpeg executable not found at specified path.");
                    return;
                }

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
                    process.OutputDataReceived += (sender, args) =>
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                            Console.WriteLine(args.Data);
                    };

                    process.ErrorDataReceived += (sender, args) =>
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            Console.WriteLine("FFmpeg Error: " + args.Data);

                            // Check for Fontconfig error
                            if (args.Data.Contains("Fontconfig error: Cannot load default config file"))
                            {
                                Console.WriteLine("Warning: Fontconfig default config file is missing. Ensure Fontconfig is properly installed.");
                            }
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        Console.WriteLine($"FFmpeg process exited with error code {process.ExitCode}.");
                    }
                    else
                    {
                        Console.WriteLine("FFmpeg processing completed successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred while running FFmpeg: " + ex.Message);
            }
        }
    }
}