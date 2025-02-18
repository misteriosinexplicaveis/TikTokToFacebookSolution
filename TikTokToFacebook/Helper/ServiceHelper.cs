using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TikTokToFacebook.Services;
using TikTokToFacebook.Utils;

namespace TikTokToFacebook.Helper
{
    public static class ServiceHelper
    {
        public static HttpClient CreateHttpClient() => new HttpClient();

        public static ITikTokService CreateTikTokService(HttpClient client) =>
            new TikTokService(client, AppConfig.ApiKey, AppConfig.ApiHost);

        public static IVideoService CreateVideoService() => new VideoService();

        public static IFacebookService CreateFacebookService() =>
            new FacebookService(AppConfig.FacebookPageId, AppConfig.FacebookAccessToken);

        public static IDatabaseService CreateDatabaseService() =>
            new DatabaseService(AppConfig.ConnectionString);
    }
}
