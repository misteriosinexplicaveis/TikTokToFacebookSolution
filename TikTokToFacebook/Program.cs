using System.Diagnostics;
using System.Net.Http.Headers;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TikTokToFacebook.Model;
using MySql.Data.MySqlClient;
using System;

class Program
{
    //mysql
    private const string connectionString = "server=localhost;database=tiktok;user=tiktok;password=Jid4N7!SL80pEd;";

    //tiktok
    private static string tiktokuser = "@momentocuriosos";

    //tiktokapi
    private const string apiKey = "a06d1bb238mshcfa03782743f37bp1d64c0jsn0703764c6935";
    private const string apiHost = "tiktok-api23.p.rapidapi.com";

    //facebook
    private const string facebookPageId = "2789468501067417";
    private const string facebookAccessToken = "EAAJCfA3UHIQBOzyHSVZCOwBC1TAtaAcDEX4zEthBZBHrygETLMGblyAxDAWX2arhnt5V37n4MgdmPgzaJKXgSfmL4VYjINeicqQqdismhCuXE4Fyksj27i0hudF94WPdiaXj9j9lSAWVg2UCqSluxr65OHSTbEcXWwQMX2aBAgaGZAZCxvm74Lwv3q8uyAZDZD";

    //rapidapi
    private const string rapidApiKey = "a06d1bb238mshcfa03782743f37bp1d64c0jsn0703764c6935";
    private const string videoDownloadPath = "testvideo.mp4";

    //mpeg
    private const string ffmpegPath = @"C:\ffmpeg-master-latest-win64-gpl-shared\bin\ffmpeg.exe";
    private const string inputVideo = @"C:\Users\diogo.DIOGOMP84.001\source\repos\TikTokToFacebookSolution\TikTokToFacebook\bin\Debug\net8.0\testvideo.mp4";
    private const string overlayText = "https\\://www.facebook.com/misteriosinexplicaveisoficial";
    private const string outputVideo = "output_video.mp4";
    private static string? desc;
    private static string? title;

    static async Task Main(string[] args)
    {
        string userSecUid = await GetUsersSecUidAsync(tiktokuser);
        var itemList = await GetUserPostsAsync(userSecUid, "14");

        // Create a List of strings
        List<Item> newVideos = new List<Item>();

        foreach (Item item in itemList.Data.ItemList)
        {
            var id = item.Id;
 
            if (!RecordExistsInMySQL(connectionString, id, tiktokuser))
            {
                newVideos.Add(item);
            }
        }

        if(newVideos.Count == 0){
            Console.WriteLine($"No new videos to push!");
            newVideos.Clear();
            return;
        }
        
        foreach (Item item in newVideos)
        { 
            try
            {
                var videoId = item.Id;
                var createTime = item.CreateTime;
                KillProcessByName("ffmpeg");
                string videoUrl = await GetTikTokVideoUrl(tiktokuser, videoId);
                if (!string.IsNullOrEmpty(videoUrl))
                {
                    await DownloadVideo(videoUrl, videoDownloadPath);
                    Thread.Sleep(3000);
                    KillProcessByName("ffmpeg");
                    Thread.Sleep(2000);

                    if (File.Exists(outputVideo))
                    {
                        File.Delete(outputVideo);
                    }

                    RunFfmpeg(ffmpegPath, inputVideo, outputVideo, overlayText);
                    Thread.Sleep(1000);
                    KillProcessByName("ffmpeg");
                    title = RemoveWordsStartingWithHash(desc);

                    bool isVideoUploadedWithoutErros = await UploadVideoToFacebook(outputVideo, desc, title);
                    if (isVideoUploadedWithoutErros)
                    {
                        InsertRecord(videoId, createTime, tiktokuser);
                    }
                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        newVideos.Clear();
    }

    private static bool RecordExistsInMySQL(string connectionString, string id, string user)
    {
        bool exists = false;
        string query = "SELECT COUNT(*) FROM TIKTOKTOFACEBOOK WHERE Id = @Id AND User = @User";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@User", user);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    exists = count > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database Error: " + ex.Message);
            }
        }
        return exists;
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

    public static string RemoveWordsStartingWithHash(string? input)
    {
        // Split the input string into an array of words
        string[] words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Filter out words that start with '#'
        var filteredWords = words.Where(word => !word.StartsWith("#"));

        // Join the filtered words back into a single string
        string result = string.Join(" ", filteredWords);

        return result;
    }

    private static async Task<string> GetTikTokVideoUrl(string user, string videoId)
    {
        using (var client = new HttpClient())
        {
            string tiktokVideoUrl = "https://www.tiktok.com/" + user + "/video/" + videoId;
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://tiktok-download-video-no-watermark.p.rapidapi.com/tiktok/info?url={tiktokVideoUrl}");
            request.Headers.Add("x-rapidapi-key", rapidApiKey);
            request.Headers.Add("x-rapidapi-host", "tiktok-download-video-no-watermark.p.rapidapi.com");

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(body);

                desc = jsonResponse["data"]?["desc"]?.ToString();

             
                return jsonResponse["data"]["video_link_nwm"].ToString();
            }
        }
    }

    private static async Task<ItemListResponse> GetUserPostsAsync(string secUid, string qty)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://tiktok-api23.p.rapidapi.com/api/user/posts?secUid=" + secUid + "&count=" + qty + "&cursor=0"),
            Headers =
    {
        { "x-rapidapi-key", apiKey },
        { "x-rapidapi-host", apiHost },
    },
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            ItemListResponse itemListResponse = JsonConvert.DeserializeObject<ItemListResponse>(body);

            //var jsonResponse = JObject.Parse(body);

            return itemListResponse;
        }
    }

    private static async Task<string> GetUsersSecUidAsync(string user)
    {
        string thistiktokuser = user.Replace("@", ""); // Remove the '@' symbol

        using (var client = new HttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://tiktok-api23.p.rapidapi.com/api/user/info?uniqueId={thistiktokuser}");
            request.Headers.Add("x-rapidapi-key", apiKey);
            request.Headers.Add("x-rapidapi-host", apiHost);

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(body);
                string secUid = jsonResponse["userInfo"]["user"]["secUid"].ToString();
                return secUid;
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

    private static async Task<Boolean> UploadVideoToFacebook(string filePath, string description, string title)
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
                return true;
            }
            else
            {
                Console.WriteLine($"Facebook Upload Error: {responseString}");
                return false;
            }
        }
    }

    public static void InsertRecord(string id, long createTime, string user)
    {
        bool success = false;
        string query = "INSERT INTO TIKTOKTOFACEBOOK (Id, CreateTime, User) VALUES (@Id, @CreateTime, @User)";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@CreateTime", createTime);
                    cmd.Parameters.AddWithValue("@User", user);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    success = rowsAffected > 0;
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"MySQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
            }
        }
    }
}