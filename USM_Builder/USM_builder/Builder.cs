using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USM_builder
{
    internal class Builder
    {
        private List<FileNames> _fileNames = [];      
        private List<FileNamesNoSbt> _fileNamesNoSbt = [];                  // @todo чтобы была возможность использовать разные имена файлов

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
#if DEBUG
            Console.WriteLine("Do Not Use Subtitles: " + IOStore.doNotUseSubtitles);
#endif
            // Получаем список .mp4 файлов
            string[] mp4Files = Directory.GetFiles(filePath, "*.m2v");

            string[] txtFiles = [""];
            if (!IOStore.doNotUseSubtitles)
            {
                // Получаем список .txt файлов
                txtFiles = Directory.GetFiles(filePath, "*.txt");
            }

            // Получаем списки имен файлов без расширения
            List<string?> mp4BaseNames = mp4Files.Select(Path.GetFileNameWithoutExtension).ToList();
            List<string?> txtBaseNames = [];
            if (!IOStore.doNotUseSubtitles)
            {
                txtBaseNames = txtFiles.Select(Path.GetFileNameWithoutExtension).ToList();
            }

            // Проверяем для каждого файла .mp4 наличие соответствующего файла .txt и наоборот
            foreach (string? mp4BaseName in mp4BaseNames)
            {
                // mp4BaseName не может быть null, но вдруг кто-то словит
                if (mp4BaseName == null)
                {
                    Console.WriteLine($"Ошибка - код 01, сообщите разработчикам");
                    return 0;
                }
                // Проверяем, существует ли соответствующий файл .txt, при успехе добавляем в список
                // при неудаче, сообщаем пользователю. Уже пользователь решает, что делать с этим файлом
                if (!IOStore.doNotUseSubtitles)
                {
                    if (txtBaseNames.Contains(mp4BaseName + "_en"))
                    {
                        _fileNames.Add(new FileNames(mp4BaseName, mp4BaseName + ".avi", mp4BaseName + "_en.txt", mp4BaseName + ".wav"));
                    }
                    else if (txtBaseNames.Contains(mp4BaseName))
                    {
                        _fileNames.Add(new FileNames(mp4BaseName, mp4BaseName + ".avi", mp4BaseName + ".txt", mp4BaseName + ".wav"));
                    }
                }
                else
                {
                    Console.WriteLine($"Файл субтитров для '{filePath}/{mp4BaseName}.avi' не найден");
                }
                if (IOStore.doNotUseSubtitles)
                {
                    // без субтитров
                    _fileNamesNoSbt.Add(new FileNamesNoSbt(mp4BaseName, mp4BaseName + ".avi", mp4BaseName + ".wav"));
                }
            }

            // Результат выполнения
            if (IOStore.doNotUseSubtitles)
            {
                Console.WriteLine($"Найдено {_fileNamesNoSbt.Count * 2} файла(ов). Можно сгенерировать {_fileNamesNoSbt.Count} .usm файла(ов)");
                return _fileNamesNoSbt.Count;
            }
            else
            {
                Console.WriteLine($"Найдено {_fileNames.Count * 2} файла(ов). Можно сгенерировать {_fileNames.Count} .usm файла(ов)");
                return _fileNames.Count;
            }
        }


        /// <summary>
        /// Обработка avi + wav + txt файлов в .usm пригодного для игры
        /// @todo убрать хардкод и сделать обработку _fileNames
        /// </summary>
        /// <param name="videoInfo"></param>
        public void encodeAll()
        {
            if ((_fileNames.Count == 0 && !IOStore.doNotUseSubtitles) || (_fileNamesNoSbt.Count == 0 && IOStore.doNotUseSubtitles))
            {
                Logger.WriteLine("encodeAll - файлы не найдены");
                return;
            }

            Logger.WriteLine("Подготавливаем файлы для Scaleform...");

            int counter = 0;
            if (!IOStore.doNotUseSubtitles)
            {
                foreach (FileNames file in _fileNames)
                {
                    Logger.WriteLine($"файл номер: {counter++}, {file.filename}");
                    // Create an instance of FfmpegHelper
                    FfmpegHelper ffmpegHelper = new(IOStore.ffmpegPath, IOStore.ffprobePath);

                    // Получаем битрейт m2v файла
                    float frameRatem2v = ffmpegHelper.GetVideoFrameRate($"{IOStore.tempFolder}/{file.media}");
                    Logger.WriteLine($"Video Frameate (m2v): {frameRatem2v} fps");

                    // Конвертируем m2v -> avi, hca -> wav
                    ffmpegHelper.ConvertInFfmpeg($"{IOStore.input}/{file.filename}.m2v", $"{IOStore.input}/{file.filename}.hca", $"{IOStore.tempFolder}/{file.media}", $"{IOStore.tempFolder}/{file.audio}", frameRatem2v);
                    Logger.WriteLine($"m2v -> avi, hca -> wav completed");

                    // Get the bitrate of the AVI video file
                    int videoBitrate = ffmpegHelper.GetVideoBitrate($"{IOStore.tempFolder}/{file.filename}.avi");
                    Logger.WriteLine($"Video Bitrate (converted avi): {videoBitrate} b/s");

                    // Get the bitrate of the AVI video file
                    float frameRate = ffmpegHelper.GetVideoFrameRate($"{IOStore.tempFolder}/{file.filename}.avi");
                    Logger.WriteLine($"Video Frameate: {frameRate} fps");

                    // Get the bitrate of the WAV audio file
                    int audioBitrate = ffmpegHelper.GetAudioBitrate($"{IOStore.tempFolder}/{file.filename}.wav");
                    Logger.WriteLine($"Audio Bitrate: {audioBitrate} b/s");

                    // fix txt file (remove null characters)
                    removeNullCharacters(file.txt);

                    // .avi, .wav, .txt, 885833, 129498, 24
                    convertInVideoEncoder($"{IOStore.tempFolder}/{file.filename}.avi", $"{IOStore.tempFolder}/{file.filename}.wav", file.txt, videoBitrate, audioBitrate, frameRate);

                    // Deleting unnecessary files
                    //removeTempFiles($"{file.filename}.avi", $"{file.filename}.wav", file.txt);
                }
            }
            else
            {
                foreach (FileNamesNoSbt file in _fileNamesNoSbt)
                {
                    Logger.WriteLine($"файл номер: {counter++}, {file.filename}");
                    // Create an instance of FfmpegHelper
                    FfmpegHelper ffmpegHelper = new(IOStore.ffmpegPath, IOStore.ffprobePath);

                    // Получаем битрейт m2v файла
                    float frameRatem2v = ffmpegHelper.GetVideoFrameRate($"{IOStore.tempFolder}/{file.media}");
                    Logger.WriteLine($"Video Frameate (m2v): {frameRatem2v} fps");

                    ffmpegHelper.ConvertInFfmpeg($"{IOStore.input}/{file.filename}.m2v", $"{IOStore.input}/{file.filename}.hca", $"{IOStore.tempFolder}/{file.media}", $"{IOStore.tempFolder}/{file.audio}", frameRatem2v);

                    // Get the bitrate of the AVI video file
                    int videoBitrate = ffmpegHelper.GetVideoBitrate($"{IOStore.tempFolder}/{file.filename}.avi");
                    Logger.WriteLine($"Video Bitrate: {videoBitrate} b/s");

                    // Get the bitrate of the AVI video file
                    float frameRate = ffmpegHelper.GetVideoFrameRate($"{IOStore.tempFolder}/{file.filename}.avi"); // появляется красный текст: At least one output file must be specified
                    Logger.WriteLine($"Video Frameate: {frameRate} fps");

                    // Get the bitrate of the WAV audio file
                    int audioBitrate = ffmpegHelper.GetAudioBitrate($"{IOStore.tempFolder}/{file.filename}.wav");
                    Logger.WriteLine($"Audio Bitrate: {audioBitrate} b/s");

                    // .avi, .wav, "", 885833, 129498, 24
                    convertInVideoEncoder($"{IOStore.tempFolder}/{file.filename}.avi", $"{IOStore.tempFolder}/{file.filename}.wav", "", videoBitrate, audioBitrate, frameRate);

                    // Deleting unnecessary files
                    removeTempFiles($"{file.filename}.avi", $"{file.filename}.wav", "");
                }
            }
        }

        public void removeNullCharacters(String subtitleFileName)
        {
            string txtFilePath = $"{IOStore.input}/{subtitleFileName}";
            string txtFile = File.ReadAllText(txtFilePath).Replace("\0", "");
            string outputFilePath = $"{IOStore.tempFolder}/{subtitleFileName}";
            File.WriteAllTextAsync(outputFilePath, txtFile);
        }

        public void createPlaceHolderSubtitleFile()
        {
            // Specify the file path
            string filePath = $"{IOStore.tempFolder}/placeholder_subtitles.txt";

            // Check if the directory exists, if not, create it
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Check if the file exists, if not, create it and write content
            if (!File.Exists(filePath))
            {
                // Не трогать две строки ниже!
                string content = @"1000
0000, 0001, .";

                File.WriteAllText(filePath, content);
            }
        }

        /// <summary>
        /// Создает папку если она не существует
        /// </summary>
        /// <param name="filePath"></param>
        public void createFolder(string filePath)
        {
            // Check if the directory exists, if not, create it
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public void removeTempFiles(string video, string audio, string txt)
        {
            File.Delete($"{IOStore.tempFolder}/{video}");
            File.Delete($"{IOStore.tempFolder}/{audio}");
            if (txt != "")
                File.Delete($"{IOStore.tempFolder}/{txt}");
            Logger.WriteLine("удалены временные файлы для " + $"{Path.GetFileNameWithoutExtension(video)}");
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
            Logger.WriteLine("Scaleform - внесение параметров для конвертации...");
            ProcessStartInfo processStartInfo = new();
            string outputFile = $"{IOStore.output}/{Path.GetFileNameWithoutExtension(videoFileName)}.usm";
            processStartInfo.FileName = IOStore.encoderPath;
            if (!IOStore.doNotUseSubtitles)
            {
                processStartInfo.Arguments = String.Format(
                "-target=xboxone -h264_profile=high -hca=on -hca_quality=5 -video00=\"{0}\" -output=\"{1}\" -bitrate={2} {3} -framerate={4} -subtitle00=\"{6}\" -subtitle01=\"{5}\"", // -subtitle00 и -subtitle01 дублируются т.к. Scaleform video encoder не умеет записывать строго в 1 дорожку сабы, он пишет сначала в 0 потом в 1
                videoFileName,
                outputFile,
                VideoBitrate,
                audioFileName != null ? $"-audio00=\"{audioFileName}\"" : "",
                Framerate,
                $"{IOStore.tempFolder}/{subtitleFileName}",
                $"{IOStore.tempFolder}/placeholder_subtitles.txt"
                );
            }
            else
            {
                processStartInfo.Arguments = String.Format(
                    "-target=xboxone -h264_profile=high -hca=on -hca_quality=5 -video00=\"{0}\" -output=\"{1}\" -bitrate={2} {3} -framerate={4}",
                    videoFileName,
                    outputFile,
                    VideoBitrate,
                    audioFileName != null ? $"-audio00=\"{audioFileName}\"" : "",
                    Framerate
                    );
            }
            processStartInfo.UseShellExecute = false;

            Process videoEncoderProcess = new Process() { StartInfo = processStartInfo };
            videoEncoderProcess.Start();

            Logger.WriteLine("Scaleform - начало конвертации.");
            Console.WriteLine(processStartInfo.Arguments);
            videoEncoderProcess.WaitForExit();

            if (!File.Exists(outputFile))
            {
                Logger.WriteLine("Scaleform - ошибочка вышла! Конвертация провалилась, смотрите лог medianocheH264.log");
            }
            else Logger.WriteLine("Scaleform - конец конвертации. Файл .usm сгенерирован");
        }
    }
}
