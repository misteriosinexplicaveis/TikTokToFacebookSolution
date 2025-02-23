using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TikTokToFacebook.Services
{
    public interface IProcessService
    {
        void KillProcessByName(string processName);
    }
}
