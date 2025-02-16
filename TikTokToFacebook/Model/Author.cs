using Newtonsoft.Json;

namespace TikTokToFacebook.Model
{
    public class Author
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}