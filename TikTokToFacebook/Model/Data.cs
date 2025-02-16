using Newtonsoft.Json;

namespace TikTokToFacebook.Model
{
    public class Data
    {
        [JsonProperty("itemList")]
        public List<Item> ItemList { get; set; }
    }
}