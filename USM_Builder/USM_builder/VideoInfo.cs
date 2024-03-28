using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USM_builder
{
    /// <summary>
    /// Класс для получения информации о потоке
    /// </summary>
    internal class VideoInfo
    {
        public readonly String VideoBitrate;
        public readonly String AudioBitrate;
        public readonly int Framerate;

        public VideoInfo(string videoBitrate, string audioBitrate, int fps)
        {
            VideoBitrate = videoBitrate;
            AudioBitrate = audioBitrate;
            Framerate = fps;
        }

        public VideoInfo(String ffmpegString)
        {
            Framerate = 0;
            AudioBitrate = null;
            String[] splitByLine = ffmpegString.Split('\n');

            String[] second = splitByLine[1].Split('=');
            String[] fpsSplit = second[1].Replace("\r", "").Split('/');
            int fps1 = Convert.ToInt32(fpsSplit[0]);
            int fps2 = Convert.ToInt32(fpsSplit[1]);
            float floatFps = (float)fps1 / (float)fps2;
            Framerate = (int)Math.Ceiling(floatFps);

            // получим третью строчку, и после = захватим битрейт
            String[] third = splitByLine[2].Split('=');
            VideoBitrate = third[1].Replace("\r", "");
            if (splitByLine.Length > 4 && splitByLine[4].Length > 0)
            {
                String[] forth = splitByLine[6].Split('=');
                AudioBitrate = forth[1].Replace("\r", "");
            } // теперь по идее нам нужно найти файл с аудио битрейтом
        }

        public override string ToString() // это просто строковое представление, для дебага или чтения
        {
            return String.Format("VideoBitrate {0}, AudioBitrate {1}", VideoBitrate, AudioBitrate != null ? AudioBitrate : "NULL");
        }
    }
}
