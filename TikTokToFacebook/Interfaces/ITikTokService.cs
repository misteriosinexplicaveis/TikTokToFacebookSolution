using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TikTokToFacebook.Model;

namespace TikTokToFacebook.Services
{
    public interface ITikTokService
    {
        Task<string> GetUsersSecUidAsync(string user);
        Task<ItemListResponse> GetUserPostsAsync(string secUid, string qty);
        Task<TikTokItemListResponse> GetTikTokVideoUrl(string user, string videoId);
    }
}
