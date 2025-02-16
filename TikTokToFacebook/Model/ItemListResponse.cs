using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TikTokToFacebook.Model
{
    public class ItemListResponse
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}
