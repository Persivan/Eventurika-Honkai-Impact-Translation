using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USM_builder
{
    /// <summary>
    /// Хранилище для всех констант
    /// </summary>
    internal class IOStore
    {
        public static string input;
        public static string ffmpegPath;
        public static string ffprobePath;
        public static string encoderPath;
        public static string output;
        public static string tempFolder = "usm_builder_temp";
        public static bool doNotUseSubtitles = false;

        public static void Initialize(string[] args)
        {
            if (args.Length >= 5)
            {
                input = Path.GetFullPath(args[0]);
                ffmpegPath = args[1];
                ffprobePath = args[2];
                encoderPath = args[3];
                output = Path.GetFullPath(args[4]);

                input = Path.TrimEndingDirectorySeparator(input);
                output = Path.TrimEndingDirectorySeparator(output);

                if (args.Length == 6 && args[5] == "-doNotUseSubtitles")
                {
                    doNotUseSubtitles = true;
                }
            }
            else
            {
                throw new ArgumentException("Not enough arguments provided.");
            }

            tempFolder = Path.GetFullPath(tempFolder);
        }
    }
}