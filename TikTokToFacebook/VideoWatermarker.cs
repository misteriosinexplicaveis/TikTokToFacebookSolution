using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TikTokToFacebook
{
    class VideoWatermarker
    {
        /// <summary>
        /// Adds a text watermark to a video using FFmpeg.
        /// </summary>
        /// <param name="inputVideoPath">Path to the input video file.</param>
        /// <param name="outputVideoPath">Path to save the watermarked video.</param>
        /// <param name="watermarkText">Text to use as the watermark.</param>
        /// <param name="fontSize">Font size of the watermark text.</param>
        /// <param name="fontColor">Color of the watermark text (e.g., white, black, red).</param>
        /// <param name="positionX">X position of the watermark (e.g., 10 for left, W-w-10 for right).</param>
        /// <param name="positionY">Y position of the watermark (e.g., 10 for top, H-h-10 for bottom).</param>
        /// <param name="ffmpegPath">Path to the FFmpeg executable (default is "ffmpeg.exe").</param>
        /// <returns>True if the watermark was added successfully; otherwise, false.</returns>
        public static bool AddTextWatermark(
            string inputVideoPath,
            string outputVideoPath,
            string watermarkText,
            int fontSize = 24,
            string fontColor = "white",
            string positionX = "10",
            string positionY = "10",
            string ffmpegPath = "ffmpeg.exe")
        {
            try
            {
                // FFmpeg command to add text watermark
                string ffmpegCommand = $"-i \"{inputVideoPath}\" -vf \"drawtext=text='{watermarkText}':fontcolor={fontColor}:fontsize={fontSize}:x={positionX}:y={positionY}\" -codec:a copy \"{outputVideoPath}\"";

                // Create a process to run FFmpeg
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = ffmpegCommand,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    process.Start();

                    // Read the output (optional, for debugging)
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine("Watermark added successfully!");
                        Console.WriteLine($"Output video: {outputVideoPath}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Error adding watermark:");
                        Console.WriteLine(error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }
    }
}
