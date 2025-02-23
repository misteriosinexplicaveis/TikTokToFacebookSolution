using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TikTokToFacebook.Utils
{
    using System;
    using System.IO;
    using System.Threading;

    public static class FileUtils
    {
        /// <summary>
        /// Waits until a file exists, checking every 500ms for a maximum of 5 seconds.
        /// </summary>
        /// <param name="filePath">The full path of the file to wait for.</param>
        /// <param name="timeoutSeconds">The maximum wait time in seconds (default is 5 seconds).</param>
        /// <returns>True if the file exists within the timeout, otherwise false.</returns>
        public static bool WaitForFile(string filePath, int timeoutSeconds = 5)
        {
            int elapsedMilliseconds = 0;
            int pollingInterval = 500; // Check every 500ms

            while (elapsedMilliseconds < timeoutSeconds * 1000)
            {
                if (File.Exists(filePath))
                {
                    return true; // File found
                }

                Thread.Sleep(pollingInterval);
                elapsedMilliseconds += pollingInterval;
            }

            return false; // Timeout reached
        }
    }
}
