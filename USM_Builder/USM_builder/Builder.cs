using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USM_builder
{
    internal class Builder
    {
        private string _currentDirectory = Directory.GetCurrentDirectory();
        private List<string> _fileNames = new List<string>();   // @todo переписать под использованием типа FileNames
                                                                // @todo чтобы была возможность использовать разные имена файлов

        /// <summary>
        /// Функция берет все .mp4, .txt файлы из папки. 
        /// По названию mp4 файла ищет соответствующий .txt файл 
        /// Если есть файл FILE_NAME.mp4, то соответсововать ему должен файл FILE_NAME_en.txt
        /// Если такой файл найден, то название файла будет добавлено в массив _fileNames
        /// Если такой файл НЕ найден, то об этом будет сообщено пользователю
        /// </summary>
        /// <param name="filePath"></param>
        public void readFileNames(string filePath)
        {
            // Получаем список .mp4 файлов
            var mp4Files = Directory.GetFiles(filePath, "*.mp4");

            // Получаем список .txt файлов
            var txtFiles = Directory.GetFiles(filePath, "*.txt");

            // Получаем списки имен файлов без расширения
            var mp4BaseNames = mp4Files.Select(Path.GetFileNameWithoutExtension).ToList();
            var txtBaseNames = txtFiles.Select(Path.GetFileNameWithoutExtension).ToList();

            // Проверяем для каждого файла .mp4 наличие соответствующего файла .txt и наоборот
            foreach (var mp4BaseName in mp4BaseNames)
            {
                // mp4BaseName не может быть null, но вдруг кто-то словит
                if (mp4BaseName == null)
                {
                    Console.WriteLine($"Ошибка - код 01, сообщите разработчикам");
                    return;
                }
                // Проверяем, существует ли соответствующий файл .txt, при успехе добавляем в список
                // при неудаче, сообщаем пользователю. Уже пользователь решает, что делать с этим файлом
                if (txtBaseNames.Contains(mp4BaseName + "_en"))
                {
                    _fileNames.Add(mp4BaseName);
                }
                else
                {
                    Console.WriteLine($"Файл субтитров для '{filePath}/{mp4BaseName}.mp4' не найден");
                }
            }

            // Результат выполнения
            Console.WriteLine($"Найдено {_fileNames.Count * 2} файла(ов). Можно сгенерировать {_fileNames.Count} .usm файла(ов)");
        }


        /// <summary>
        /// Обработка avi + wav + txt файлов в .usm пригодного для игры
        /// @todo убрать хардкод и сделать обработку _fileNames
        /// </summary>
        /// <param name="videoInfo"></param>
        public void encodeAll()
        {
            Console.WriteLine(DateTime.Now + " - Передаем файл в Scaleform...");

            string[] hardcode = [
                "usm_builder_input/Story05_CG01.avi",
                "usm_builder_input/Story05_CG01.wav",
                "usm_builder_input/Story05_CG01_en.txt",
                "usm_builder_output/Story05_CG01.usm"
            ];

            // Create an instance of FfmpegHelper
            FfmpegHelper ffmpegHelper = new FfmpegHelper(IOStore.ffmpegPath, IOStore.ffprobePath);

            // Get the bitrate of the AVI video file
            int videoBitrate = ffmpegHelper.GetVideoBitrate(hardcode[0]);
            Console.WriteLine($"Video Bitrate: {videoBitrate} b/s");

            // Get the bitrate of the WAV audio file
            int audioBitrate = ffmpegHelper.GetAudioBitrate(hardcode[1]);
            Console.WriteLine($"Audio Bitrate: {audioBitrate} b/s");

            // Get the bitrate of the WAV audio file
            float frameRate = ffmpegHelper.GetVideoFrameRate(hardcode[0]);
            Console.WriteLine($"Video Frameate: {frameRate} fps");

            // .avi, .wav, .txt, 885833, 129498, 24
            convertInVideoEncoder(hardcode[0], hardcode[1], hardcode[2], videoBitrate, audioBitrate, frameRate);
        }

        /// <summary>
        /// Сборка avi + wav + txt файлов в .usm
        /// @todo проверить почему энкодеру не нравится формат сабов
        /// </summary>
        /// <param name="videoFileName"></param>
        /// <param name="audioFileName"></param>
        /// <param name="subtitleFileName"></param>
        /// <param name="videoInfo"></param>
        private void convertInVideoEncoder(String videoFileName, String audioFileName, String subtitleFileName, int VideoBitrate, int AudioBitrate, float Framerate)
        {
            Console.WriteLine(DateTime.Now + " - Scaleform - внесение параметров...");
            var processStartInfo = new ProcessStartInfo();
            var outputFile = IOStore.output + '/' + Path.GetFileNameWithoutExtension(videoFileName) + ".usm";
            processStartInfo.FileName = IOStore.encoderPath + "/medianocheH264.exe";
            processStartInfo.Arguments = String.Format(
                "-target=xboxone -h264_profile=high -hca=on -hca_quality=5 -video00=\"{0}\" -output=\"{1}\" -bitrate={2} {3} -framerate={4} -subtitle00=\"{6}\" -subtitle01=\"{5}\"", // -subtitle00 и -subtitle01 дублируются т.к. Scaleform video encoder не умеет записывать строго в 1 дорожку сабы, он пишет сначала в 0 потом в 1
                videoFileName,
                outputFile,
                VideoBitrate,
                audioFileName != null ? $"-audio00=\"{audioFileName}\"" : "",
                Framerate,
                subtitleFileName,
                "assets/placeholder_subtitles.txt"
                );
            processStartInfo.UseShellExecute = false;

            Process videoEncoderProcess = new Process() { StartInfo = processStartInfo };
            videoEncoderProcess.Start();

            Console.WriteLine(DateTime.Now + " - Scaleform - начало конвертации.");
            Console.WriteLine(processStartInfo.Arguments);
            videoEncoderProcess.WaitForExit();

            if (!File.Exists(outputFile))
            {
                Console.WriteLine(DateTime.Now + " - Scaleform - ошибочка вышла! Конвертация провалилась, смотрите лог medianocheH264.log");
            }
            else Console.WriteLine(DateTime.Now + " - Scaleform - конец конвертации. Файл .usm сгенерирован");
        }
    }
}
