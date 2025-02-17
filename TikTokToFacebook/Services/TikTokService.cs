using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using TikTokToFacebook.Model;

namespace TikTokToFacebook.Services
{
    public class TikTokService : ITikTokService
    {
        private readonly HttpClient _client;
        private readonly string _apiKey;
        private readonly string _apiHost;

        public TikTokService(HttpClient client, string apiKey, string apiHost)
        {
            _client = client;
            _apiKey = apiKey;
            _apiHost = apiHost;
        }
        public async Task<string> GetUsersSecUidAsync(string user)
        {
            string sanitizedUser = user.Replace("@", "");
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://tiktok-api23.p.rapidapi.com/api/user/info?uniqueId={sanitizedUser}");
            request.Headers.Add("x-rapidapi-key", _apiKey);
            request.Headers.Add("x-rapidapi-host", _apiHost);

            using var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(body);
            return jsonResponse["userInfo"]["user"]["secUid"].ToString();
        }

        public async Task<ItemListResponse> GetUserPostsAsync(string secUid, string qty)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://tiktok-api23.p.rapidapi.com/api/user/posts?secUid={secUid}&count={qty}&cursor=0");
            request.Headers.Add("x-rapidapi-key", _apiKey);
            request.Headers.Add("x-rapidapi-host", _apiHost);

            using var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ItemListResponse>(body);
        }

        public async Task<TikTokItemListResponse> GetTikTokVideoUrl(string user, string videoId)
        {
            string tiktokVideoUrl = $"https://www.tiktok.com/{user}/video/{videoId}";
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://tiktok-download-video-no-watermark.p.rapidapi.com/tiktok/info?url={tiktokVideoUrl}");
            request.Headers.Add("x-rapidapi-key", _apiKey);
            request.Headers.Add("x-rapidapi-host", "tiktok-download-video-no-watermark.p.rapidapi.com");

            using var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TikTokItemListResponse>(body);
        }
    }
}
