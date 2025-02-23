using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TikTokToFacebook.Model
{
    public class TikTokItemListResponse
    {
        [JsonProperty("data")]
        public TiktokData Data { get; set; }
    }
}
