using System;

namespace TikTokToFacebook.Utils
{
    public static class AppConfig
    {
        public static string Watermark => GetEnvironmentVariable("WATERMARK", "https://www.facebook.com/misteriosinexplicaveisoficial");
        public static string User => GetEnvironmentVariable("USER", "@momentocuriosos");
        public static string VideoQuantity => GetEnvironmentVariable("VIDEO_QUANTITY", "3");
        public static string ApiKey => GetEnvironmentVariable("API_KEY", "default-api-key");
        public static string ApiHost => GetEnvironmentVariable("API_HOST", "tiktok-api23.p.rapidapi.com");
        public static string FacebookPageId => GetEnvironmentVariable("FACEBOOK_PAGE_ID", "2789468501067417");
        public static string FacebookAccessToken => GetEnvironmentVariable("FACEBOOK_ACCESS_TOKEN", "default-facebook-token");
        public static string ConnectionString => GetEnvironmentVariable("CONNECTION_STRING", "server=localhost;database=tiktok;user=tiktok;password=default-password;");

        private static string GetEnvironmentVariable(string key, string defaultValue)
        {
            return Environment.GetEnvironmentVariable(key) ?? defaultValue;
        }
    }
}