using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TikTokToFacebook;

class Program
{
    private static readonly string facebookPageId = "2789468501067417";
    private static readonly string facebookAccessToken = "EAAJCfA3UHIQBOwjGQdMWhKv9eEmxdzrjQYHosZAhOvc6IKu8DD9QFgBIyo5RLm8tkDicsNUceHUS8BpN5CclZADDsU71iwKasbkiAkVqgZCiZCU6n3dhyoUEh1891QEGQw85pRYIFKldZCqwxuHqoWU2fU3ZC5cOzGsqFT1vfMod6j4mZBcgpZAZCeqZCwZCAwWGcYrtKpeaMZBZAsOqMryXQDiitwAZDZD";
    private static readonly string tiktokVideoUrl = "https://www.tiktok.com/@momentocuriosos/video/7463478484153961734";
    private static readonly string rapidApiKey = "a06d1bb238mshcfa03782743f37bp1d64c0jsn0703764c6935";
    private static readonly string videoDownloadPath = "testvideo.mp4";
    private static string desc;
    private static string title;

    static async Task Main(string[] args)
    {
         try
        {
            KillProcessByName("ffmpeg");
            string videoUrl = await GetTikTokVideoUrl();
            if (!string.IsNullOrEmpty(videoUrl))
            {
                await DownloadVideo(videoUrl, videoDownloadPath);
                Thread.Sleep(3000);
                KillProcessByName("ffmpeg");
                Thread.Sleep(2000);

                string ffmpegPath = @"C:\Users\diogo.DIOGOMP84.001\Downloads\ffmpeg-master-latest-win64-gpl-shared\ffmpeg-master-latest-win64-gpl-shared\bin\ffmpeg.exe";
                string inputVideo = @"C:\Users\diogo.DIOGOMP84.001\source\repos\TikTokToFacebookSolution\TikTokToFacebook\bin\Debug\net8.0\testvideo.mp4";
                string outputVideo = "output_video.mp4";
                string overlayText = "https\\://www.facebook.com/misteriosinexplicaveisoficial";

                if (File.Exists(outputVideo))
                {
                    File.Delete(outputVideo);
                }

                RunFfmpeg(ffmpegPath, inputVideo, outputVideo, overlayText);
                Thread.Sleep(1000);
                KillProcessByName("ffmpeg");
                title = RemoveWordsStartingWithHash(desc);

                await UploadVideoToFacebook(outputVideo, desc, title);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static void KillProcessByName(string processName)
    {
        var processes = Process.GetProcessesByName(processName);

        if (processes.Length == 0)
        {
            Console.WriteLine($"No running process found with name: {processName}");
            return;
        }

        foreach (var process in processes)
        {
            try
            {
                Console.WriteLine($"Killing process {process.ProcessName} (PID: {process.Id})");
                process.Kill();
                process.WaitForExit(); // Ensures the process is terminated
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error killing process {process.ProcessName}: {ex.Message}");
            }
        }
    }

    static void RunFfmpeg(string ffmpegPath, string inputVideo, string outputVideo, string overlayText)
    {
        string arguments = $"-i \"{inputVideo}\" -vf \"drawtext=text='{overlayText}':fontcolor=yellow:fontsize=16:x=10:y=10\" -codec:a copy \"{outputVideo}\"";

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

    public static string RemoveWordsStartingWithHash(string input)
    {
        // Split the input string into an array of words
        string[] words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Filter out words that start with '#'
        var filteredWords = words.Where(word => !word.StartsWith("#"));

        // Join the filtered words back into a single string
        string result = string.Join(" ", filteredWords);

        return result;
    }

    private static async Task<string> GetTikTokVideoUrl()
    {
        using (var client = new HttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://tiktok-download-video-no-watermark.p.rapidapi.com/tiktok/info?url={tiktokVideoUrl}");
            request.Headers.Add("x-rapidapi-key", rapidApiKey);
            request.Headers.Add("x-rapidapi-host", "tiktok-download-video-no-watermark.p.rapidapi.com");

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(body);

                desc = jsonResponse["data"]["desc"].ToString();

                return jsonResponse["data"]["video_link_nwm"].ToString();
            }
        }
    }

    private static async Task DownloadVideo(string videoUrl, string outputPath)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(videoUrl);
            response.EnsureSuccessStatusCode();

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = File.Create(outputPath))
            {
                await stream.CopyToAsync(fileStream);
            }
        }
        Console.WriteLine("Video downloaded successfully: " + outputPath);
    }

    private static async Task UploadVideoToFacebook(string filePath, string description, string title)
    {
        using (var client = new HttpClient())
        using (var form = new MultipartFormDataContent())
        using (var fileStream = File.OpenRead(filePath))
        using (var content = new StreamContent(fileStream))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
            form.Add(content, "source", Path.GetFileName(filePath));

            form.Add(new StringContent(description), "description");
            form.Add(new StringContent(title), "title");

            var requestUri = $"https://graph-video.facebook.com/v22.0/{facebookPageId}/videos?access_token={facebookAccessToken}";
            var response = await client.PostAsync(requestUri, form);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Video uploaded successfully to Facebook!");
            }
            else
            {
                Console.WriteLine($"Facebook Upload Error: {responseString}");
            }
        }
    }
}