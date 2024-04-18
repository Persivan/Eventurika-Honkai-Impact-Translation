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
        /// Функция конвертации mp4 файлов в avi и wav
        /// @todo нужно переписать. Нужно переводить вывод VGMToolBox (m2v, hca) сразу в avi и wav.
        /// </summary>
        /// <returns></returns>
        public VideoInfo convertToAviAndWav()
        {
            if (!Directory.Exists(IOStore.output)) Directory.CreateDirectory(IOStore.output);

            const string mp4File = "usm_builder_input/Story05_CG01.mp4";

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = IOStore.ffmpegPath + "/bin/ffprobe.exe";
            processStartInfo.Arguments = String.Format("{0} -show_entries stream=bit_rate,avg_frame_rate -hide_banner", mp4File);
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.CreateNoWindow = true;

            VideoInfo videoInfo;
            Process ffprobeProcess = new Process() { StartInfo = processStartInfo };
            ffprobeProcess.Start();

            StringBuilder fullOutput = new StringBuilder();
            while (!ffprobeProcess.StandardOutput.EndOfStream)
            {
                string line = ffprobeProcess.StandardOutput.ReadLine();
                fullOutput.AppendLine(line);
            }

            ffprobeProcess.WaitForExit();

            videoInfo = new VideoInfo(fullOutput.ToString());
            Console.WriteLine(videoInfo.ToString());


            processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = IOStore.ffmpegPath + "/bin/ffmpeg.exe";
            String newVideoFileName = mp4File.Remove(mp4File.Length - 3, 3) + "avi";
            String newAudioFileName = mp4File.Remove(mp4File.Length - 3, 3) + "wav";
            processStartInfo.Arguments = String.Format("-i {0} {1} -b {2} {3} {4}", mp4File,
                videoInfo.AudioBitrate != null ? "-ab " + videoInfo.AudioBitrate : "",
                videoInfo.VideoBitrate,
                newVideoFileName,
                videoInfo.AudioBitrate != null ? newAudioFileName : "");
            processStartInfo.UseShellExecute = false;

            Process ffMpegProcess = new Process() { StartInfo = processStartInfo };
            ffMpegProcess.Start();

            Console.WriteLine(DateTime.Now + " - ffMpeg - Начало конвертации...");
            ffMpegProcess.WaitForExit();
            Console.WriteLine(DateTime.Now + " - ffMpeg - Конец конвертации.");


            return videoInfo;
        }

        /// <summary>
        /// Обработка avi + wav + txt файлов в .usm пригодного для игры
        /// @todo убрать хардкод и сделать обработку _fileNames
        /// </summary>
        /// <param name="videoInfo"></param>
        public void encode(VideoInfo videoInfo)
        {
            Console.WriteLine(DateTime.Now + " - Передаем файл в Scaleform...");

            string[] hardcode = [
                "usm_builder_input/Story05_CG01.avi",
                "usm_builder_input/Story05_CG01.wav",
                "usm_builder_input/Story05_CG01_en.txt",
                "usm_builder_output/Story05_CG01.usm"
                ];

            // .avi, .wav, .txt, {VideoBitrate 885833, AudioBitrate 129498}
            convertInVideoEncoder(hardcode[0], hardcode[1], hardcode[2], videoInfo);
            //fixSubtitles(hardcode[3]);
        }

        /// <summary>
        /// Сборка avi + wav + txt файлов в .usm
        /// @todo проверить почему энкодеру не нравится формат сабов
        /// </summary>
        /// <param name="videoFileName"></param>
        /// <param name="audioFileName"></param>
        /// <param name="subtitleFileName"></param>
        /// <param name="videoInfo"></param>
        private void convertInVideoEncoder(String videoFileName, String audioFileName, String subtitleFileName, VideoInfo videoInfo)
        {
            var processStartInfo = new ProcessStartInfo();
            Console.WriteLine(DateTime.Now + " - Scaleform - внесение параметров...");
            var outputFile = "";
            processStartInfo.FileName = IOStore.encoderPath + "/medianocheH264.exe";
            processStartInfo.Arguments = String.Format(
                "-target=xboxone -h264_profile=high -video00=\"{0}\" -output=\"{1}\" -bitrate={2} {3} -framerate={4} -subtitle08=\"{5}\"",
                videoFileName,
                outputFile = IOStore.output + '/' + Path.GetFileNameWithoutExtension(videoFileName) + ".usm",
                videoInfo.VideoBitrate,
                audioFileName != null ? $"-audio00=\"{audioFileName}\"" : "",
                videoInfo.Framerate,
                subtitleFileName
                );
            //videoFileName = textBox4.Text + '/' + videoFileName.Remove(videoFileName.Length - 3, 3) + "usm",
            processStartInfo.UseShellExecute = false;
            // For silent mode
            //processStartInfo.CreateNoWindow = true;

            Process videoEncoderProcess = new Process() { StartInfo = processStartInfo };
            videoEncoderProcess.Start();

            Console.WriteLine(DateTime.Now + " - Scaleform - начало конвертации.");
            Console.WriteLine(processStartInfo.Arguments);
            videoEncoderProcess.WaitForExit();
            //outputFile
            //
            if (!File.Exists(outputFile))
            {
                Console.WriteLine(DateTime.Now + " - Scaleform - ошибочка вышла! Конвертация провалилась, смотрите лог medianocheH264.log");
            }
            else Console.WriteLine(DateTime.Now + " - Scaleform - конец конвертации. Файл .usm сгенерирован");
        }

        /// <summary>
        /// Выставление сабов с 0 дорожки на 1
        /// </summary>
        /// <param name="fileName"></param>
        private void fixSubtitles(string fileName)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = IOStore.usmSubtitleToolbox;
            processStartInfo.Arguments = String.Format("-fixSbt {0}", fileName);
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.CreateNoWindow = true;
            Process usmToolProcess = new Process() { StartInfo = processStartInfo };
            usmToolProcess.Start();
            Console.WriteLine(DateTime.Now + " - исправление language id.");
            usmToolProcess.WaitForExit();
            Console.WriteLine(DateTime.Now + " - конец работы с текущим файлом.");
        }
    }
}
