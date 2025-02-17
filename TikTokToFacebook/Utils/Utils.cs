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
        public static string RemoveWordsStartingWithHash(string? input)
        {
            // Split the input string into an array of words
            string[] words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Filter out words that start with '#'
            var filteredWords = words.Where(word => !word.StartsWith("#"));

            // Join the filtered words back into a single string
            string result = string.Join(" ", filteredWords);

            return result;
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
        }
    }
}
