using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TikTokToFacebook.Utils
{
    public static class Utils
    {
        /// <summary>
        /// Ensures the file is accessible by forcing garbage collection and retries
        /// </summary>
        public static void EnsureFileIsAccessible(string filePath)
        {
            if (IsFileLocked(filePath)){
                KillProcessesUsingFile(filePath);

                int retries = 5;
                while (IsFileLocked(filePath) && retries > 0)
                {
                    Console.WriteLine("File is still locked, retrying...");
                    Thread.Sleep(1000);
                    retries--;
                }

                if (IsFileLocked(filePath))
                {
                    Console.WriteLine("Forcing garbage collection to release file...");
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

        }

        /// <summary>
        /// Checks if a file is locked by another process
        /// </summary>
        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }

        /// <summary>
        /// Kills any processes that are using the specified file
        /// </summary>
        private static void KillProcessesUsingFile(string filePath)
        {
            try
            {
                var processes = Process.GetProcesses();

                foreach (var process in processes)
                {
                    try
                    {
                        foreach (ProcessModule module in process.Modules)
                        {
                            if (module.FileName.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"Killing process: {process.ProcessName} (PID: {process.Id})");
                                process.Kill();
                                process.WaitForExit();
                            }
                        }
                    }
                    catch (Exception) { } // Ignore processes we can't access
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while unlocking file: {ex.Message}");
            }
        }

        public static string TruncateString(string input, int maxLength)
        {
            if (input.Length <= maxLength)
                return input;

            int lastSpaceIndex = input.LastIndexOf(' ', maxLength);
            return lastSpaceIndex > 0 ? input.Substring(0, lastSpaceIndex) : input.Substring(0, maxLength);
        }

        public static string RemoveWordsWithStartWithHashes(string? input)
        {
            // Split the input string into an array of words
            string[] words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Filter out words that start with '#'
            var filteredWords = words.Where(word => !word.StartsWith("#"));

            // Join the filtered words back into a single string
            string result = string.Join(" ", filteredWords);

            return TruncateString(input, 255);
        }

        public static void KillProcessByName(string processName)
        {
            var processes = Process.GetProcessesByName(processName);

            if (processes.Length == 0)
            {
                Console.WriteLine($"No running process found with name: {processName}");
                return;
            }

            foreach (var process in processes)
            {
                try
                {
                    Console.WriteLine($"Killing process {process.ProcessName} (PID: {process.Id})");
                    process.Kill();
                    process.WaitForExit(); // Ensures the process is terminated
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error killing process {process.ProcessName}: {ex.Message}");
                }
            }
            Thread.Sleep(3000);
        }
    }
}
