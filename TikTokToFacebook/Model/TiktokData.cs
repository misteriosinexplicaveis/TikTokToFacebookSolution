using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TikTokToFacebook.Model
{
    public class TiktokData
    {
        [JsonProperty("desc")]
        public string Desc { get; set; }
        [JsonProperty("video_link_nwm")]
        public string Url { get; set; }
    }
}
