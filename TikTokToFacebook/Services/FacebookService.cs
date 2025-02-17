using System.Net.Http.Headers;

namespace TikTokToFacebook.Services
{
    public class FacebookService : IFacebookService
    {
        private readonly string _facebookPageId;
        private readonly string _accessToken;

        public FacebookService(string facebookPageId, string accessToken)
        {
            _facebookPageId = facebookPageId;
            _accessToken = accessToken;
        }

        public async Task<bool> UploadVideoAsync(string filePath, string description, string title)
        {
            using var client = new HttpClient();
            using var form = new MultipartFormDataContent();
            await using var fileStream = File.OpenRead(filePath);
            using var content = new StreamContent(fileStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
            form.Add(content, "source", Path.GetFileName(filePath));
            form.Add(new StringContent(description), "description");
            form.Add(new StringContent(title), "title");

            var requestUri = $"https://graph-video.facebook.com/v22.0/{_facebookPageId}/videos?access_token={_accessToken}";
            var response = await client.PostAsync(requestUri, form);
            return response.IsSuccessStatusCode;
        }
    }
}
