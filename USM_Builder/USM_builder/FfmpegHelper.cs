using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace USM_builder
{
    public class FfmpegHelper
    {
        private readonly string ffmpegPath;
        private readonly string ffprobePath;

        public FfmpegHelper(string ffmpegPath, string ffprobePath)
        {
            this.ffmpegPath = ffmpegPath;
            this.ffprobePath = ffprobePath;
        }

        public int GetVideoBitrate(string videoFilePath)
        {
            string command = $"-v error -select_streams v:0 -show_entries stream=bit_rate -of default=noprint_wrappers=1:nokey=1 \"{videoFilePath}\"";
            string output = RunFFprobeCommand(command);
            // Parsing the output to find the bitrate
            int bitrate = ParseBitrate(output);
            return bitrate;
        }

        public int GetAudioBitrate(string audioFilePath)
        {
            string command = $"-v error -select_streams a:0 -show_entries stream=bit_rate -of default=noprint_wrappers=1:nokey=1 \"{audioFilePath}\"";
            string output = RunFFprobeCommand(command);
            // Parsing the output to find the bitrate
            int bitrate = ParseBitrate(output);
            return bitrate;
        }

        private string RunFFprobeCommand(string command)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(processStartInfo))
            {
                if (process != null)
                {
                    process.WaitForExit();
                    string output = process.StandardOutput.ReadToEnd();
                    return output;
                }
                else
                {
                    throw new Exception("Failed to start ffprobe process.");
                }
            }
        }

        private int ParseBitrate(string output)
        {
            int bitrate;
            if (int.TryParse(output, out bitrate))
            {
                return bitrate;
            }
            throw new Exception("Bitrate not found in ffprobe output.");
        }

        private void RunFFmpegCommand(string command)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = command,
                RedirectStandardOutput = false, // с включенным редиректом в вывод не попадает сообщение о перезаписи файла
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(processStartInfo))
            {
                if (process != null)
                {
                    process.WaitForExit();
                    //string output = process.StandardOutput.ReadToEnd();
                    //string output = "aboba"; // ffmpeg не работает корректно с включённым редиректом
                    //return output;
                }
                else
                {
                    throw new Exception("Failed to start ffmpeg process.");
                }
            }
        }

        public float GetVideoFrameRate(string videoFilePath)
        {// r_frame_rate,avg_frame_rate
            string command = $"-v error -select_streams v:0 -show_entries stream=r_frame_rate -of default=noprint_wrappers=1:nokey=1 \"{videoFilePath}\"";
            string output = RunFFprobeCommand(command);
            // Parsing the output to find the frame rate
            float frameRate = NewParseFrameRate(output);
            return frameRate;
        }

        public void ConvertInFfmpeg(string videoFilePath, string audioFilePath, string outputVideoFilePath, string outputAudioFilePath, float framerate)
        {
            string command = $"-i \"{videoFilePath}\" -i \"{audioFilePath}\" -c:v rawvideo -pix_fmt yuv420p -c:a copy -r {framerate} \"{outputVideoFilePath}\" \"{outputAudioFilePath}\" -y";
            RunFFmpegCommand(command);
            // @todo если output вернул ошибку, надо кидать исключение
            //string output = "pepega";
            //return output;
        }

        private float ParseFrameRate(string output)
        {
            // Define the regular expression pattern to extract the frame rate
            string pattern = @"Stream #\d:\d: Video:.+, (\d+) fps";

            // Create a regex object
            Regex regex = new Regex(pattern);

            // Match the pattern in the input string
            Match match = regex.Match(output);

            // Check if a match is found
            if (match.Success)
            {
                // Get the frame rate value
                string frameRateStr = match.Groups[1].Value;

                // Parse the frame rate value to double
                if (float.TryParse(frameRateStr, out float frameRate))
                {
                    return frameRate;
                }
            }

            // Return a default value if frame rate extraction fails
            return -1;
        }

        private float NewParseFrameRate(string output)
        {
            // Trim the string to remove any extraneous characters like \r and \n
            string cleanedOutput = output.Trim();

            // Split the cleaned string on the '/' character
            string[] parts = cleanedOutput.Split('/');

            // Convert the numerator and denominator to integers
            if (parts.Length == 2 && int.TryParse(parts[0], out int numerator) && int.TryParse(parts[1], out int denominator))
            {
                // Calculate the float frame rate
                return (float)numerator / denominator;
            }
            else
            {
                Logger.WriteLine("Ошибка - код 04, сообщите разработчикам");
                throw new FormatException("Invalid ffprobe output format");
            }
        }
    }

}
