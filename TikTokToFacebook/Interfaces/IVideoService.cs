using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TikTokToFacebook.Services
{
    public interface IVideoService
    {
        Task DownloadVideoAsync(string videoUrl, string outputPath);
        void RunFfmpeg(string inputVideo, string outputVideo, string overlayText);
    }
}
