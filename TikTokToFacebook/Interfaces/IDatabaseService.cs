using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TikTokToFacebook.Services
{
    public interface IDatabaseService
    {
        bool RecordExists(string id, string user);
        void InsertRecord(string id, long createTime, string user);
    }
}
