using System;

namespace TikTokToFacebook.Utils
{
    public static class AppConfig
    {
        public static string Watermark => GetEnvironmentVariable("WATERMARK", "Choose Watermark");
        public static string User => GetEnvironmentVariable("USER", "user");
        public static string VideoQuantity => GetEnvironmentVariable("VIDEO_QUANTITY", "3");
        public static string ApiKey => GetEnvironmentVariable("API_KEY", "default-api-key");
        public static string ApiHost => GetEnvironmentVariable("API_HOST", "tiktok-api23.p.rapidapi.com");
        public static string FacebookPageId => GetEnvironmentVariable("FACEBOOK_PAGE_ID", "facebook_page_id");
        public static string FacebookAccessToken => GetEnvironmentVariable("FACEBOOK_ACCESS_TOKEN", "default-facebook-token");
        public static string ConnectionString => GetEnvironmentVariable("CONNECTION_STRING", "server=localhost;database=tiktok;user=tiktok;password=default-password;");

        private static string GetEnvironmentVariable(string key, string defaultValue)
        {
            return Environment.GetEnvironmentVariable(key) ?? defaultValue;
        }
    }
}