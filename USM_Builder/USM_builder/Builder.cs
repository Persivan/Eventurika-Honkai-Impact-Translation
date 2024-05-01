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
        private string _currentDirectory = Directory.GetCurrentDirectory(); // помоему это не используется
        private List<FileNames> _fileNames = new List<FileNames>();     // @todo переписать под использованием типа FileNames
                                                                        // @todo чтобы была возможность использовать разные имена файлов

        /// <summary>
        /// Функция берет все .mp4, .txt файлы из папки. 
        /// По названию mp4 файла ищет соответствующий .txt файл 
        /// Если есть файл FILE_NAME.mp4, то соответсововать ему должен файл FILE_NAME_en.txt
        /// Если такой файл найден, то название файла будет добавлено в массив _fileNames
        /// Если такой файл НЕ найден, то об этом будет сообщено пользователю
        /// </summary>
        /// <param name="filePath"></param>
        public int readFileNames(string filePath)
        {
            // Получаем список .mp4 файлов
            var mp4Files = Directory.GetFiles(filePath, "*.m2v");

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
                    return 0;
                }
                // Проверяем, существует ли соответствующий файл .txt, при успехе добавляем в список
                // при неудаче, сообщаем пользователю. Уже пользователь решает, что делать с этим файлом
                if (txtBaseNames.Contains(mp4BaseName + "_en"))
                {
                    // @todo убрать "_en.txt", как минимум стоит сделать выбор испольщовать приписку "_en" или нет
                    _fileNames.Add(new FileNames(mp4BaseName, mp4BaseName + ".avi", mp4BaseName + "_en.txt", mp4BaseName + ".wav"));
                } 
                else {
                    Console.WriteLine($"Файл субтитров для '{filePath}/{mp4BaseName}.avi' не найден");
                }
            }

            // Результат выполнения
            Console.WriteLine($"Найдено {_fileNames.Count * 2} файла(ов). Можно сгенерировать {_fileNames.Count} .usm файла(ов)");
            return _fileNames.Count;
        }


        /// <summary>
        /// Обработка avi + wav + txt файлов в .usm пригодного для игры
        /// @todo убрать хардкод и сделать обработку _fileNames
        /// </summary>
        /// <param name="videoInfo"></param>
        public void encodeAll()
        {
            if (_fileNames.Count == 0) {
                Console.WriteLine(DateTime.Now + " - encodeAll - файлы не найдены");
                return;
            }

            Console.WriteLine(DateTime.Now + " - Передаем файлы в Scaleform...");

            int counter = 0;
            foreach (var file in _fileNames)
            {
                Console.WriteLine(DateTime.Now + $" - файл номер: {counter++}, {file.filename}");
                // Create an instance of FfmpegHelper
                FfmpegHelper ffmpegHelper = new FfmpegHelper(IOStore.ffmpegPath, IOStore.ffprobePath);

                // Converting media and audio files @todo Привязать к нашей консольке, чтобы была возможность ввода Y/N когда спрашивает перезаписывать ли существующий файл
                ffmpegHelper.ConvertInFfmpeg($"{IOStore.input}/{file.filename}.m2v", $"{IOStore.input}/{file.filename}.hca", $"{IOStore.tempFolder}/{file.media}", $"{IOStore.tempFolder}/{file.audio}");

                // Get the bitrate of the AVI video file
                int videoBitrate = ffmpegHelper.GetVideoBitrate($"{IOStore.tempFolder}/{file.filename}.avi");
                Console.WriteLine(DateTime.Now + $"Video Bitrate: {videoBitrate} b/s");

                // Get the bitrate of the AVI video file
                float frameRate = ffmpegHelper.GetVideoFrameRate($"{IOStore.tempFolder}/{file.filename}.avi"); // появляется красный текст: At least one output file must be specified
                Console.WriteLine(DateTime.Now + $"Video Frameate: {frameRate} fps");

                // Get the bitrate of the WAV audio file
                int audioBitrate = ffmpegHelper.GetAudioBitrate($"{IOStore.tempFolder}/{file.filename}.wav");
                Console.WriteLine(DateTime.Now + $"Audio Bitrate: {audioBitrate} b/s");

                // .avi, .wav, .txt, 885833, 129498, 24
                convertInVideoEncoder($"{IOStore.tempFolder}/{file.filename}.avi", $"{IOStore.tempFolder}/{file.filename}.wav", file.txt, videoBitrate, audioBitrate, frameRate);
            }
        }

        public void createPlaceHolderSubtitleFile()
        {
            // Specify the file path
            string filePath = $"{IOStore.tempFolder}/placeholder_subtitles.txt";

            // Check if the directory exists, if not, create it
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Check if the file exists, if not, create it and write content
            if (!File.Exists(filePath))
            {
                string content = @"1000
0000, 0001, .";

                File.WriteAllText(filePath, content);
            }
        }

        public void createTempFolder()
        {
            // Specify the file path
            string filePath = "usm_builder_temp/";
            // Check if the directory exists, if not, create it
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Сборка avi + wav + txt файлов в .usm
        /// @todo проверить почему энкодеру не нравится формат сабов (возмонжо т.к. в конце строки есть NULLNULL)
        /// </summary>
        /// <param name="videoFileName"></param>
        /// <param name="audioFileName"></param>
        /// <param name="subtitleFileName"></param>
        /// <param name="videoInfo"></param>
        private void convertInVideoEncoder(String videoFileName, String audioFileName, String subtitleFileName, int VideoBitrate, int AudioBitrate, float Framerate)
        {
            Console.WriteLine(DateTime.Now + " - Scaleform - внесение параметров...");
            ProcessStartInfo processStartInfo = new();
            string outputFile = $"{IOStore.output}/{Path.GetFileNameWithoutExtension(videoFileName)}.usm";
            Directory.CreateDirectory(IOStore.output); // medianocheH264 выдаёт ошибку на японском если путь вывода не существует
            processStartInfo.FileName = IOStore.encoderPath;
            processStartInfo.Arguments = String.Format(
                "-target=xboxone -h264_profile=high -hca=on -hca_quality=5 -video00=\"{0}\" -output=\"{1}\" -bitrate={2} {3} -framerate={4} -subtitle00=\"{6}\" -subtitle01=\"{5}\"", // -subtitle00 и -subtitle01 дублируются т.к. Scaleform video encoder не умеет записывать строго в 1 дорожку сабы, он пишет сначала в 0 потом в 1
                videoFileName,
                outputFile,
                VideoBitrate,
                audioFileName != null ? $"-audio00=\"{audioFileName}\"" : "",
                Framerate,
                $"{IOStore.input}/{subtitleFileName}",
                $"{IOStore.tempFolder}/placeholder_subtitles.txt"
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
