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
        public static string input = Environment.GetCommandLineArgs()[1];
        public static string ffmpegPath = Environment.GetCommandLineArgs()[2];
        public static string ffprobePath = Environment.GetCommandLineArgs()[3];
        public static string encoderPath = Environment.GetCommandLineArgs()[4];
        public static string output = Environment.GetCommandLineArgs()[5];
        public static string tempFolder = "usm_builder_temp";
        public static bool doNotUseSubtitles = false;
    }
}
