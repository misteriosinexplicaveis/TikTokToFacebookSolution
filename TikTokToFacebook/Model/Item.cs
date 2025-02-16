using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TikTokToFacebook.Model
{
    public class Item
    {
        [JsonProperty("author")]
        public Author Author { get; set; }
        [JsonProperty("createTime")]
        public long CreateTime { get; set; }
    }
}
