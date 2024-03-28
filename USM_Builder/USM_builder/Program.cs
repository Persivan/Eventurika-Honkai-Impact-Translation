using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

main();

int main()
{
    Console.WriteLine(args.Length);
    // Проверка параметров запуска
    if (args.Length != 5 || args.Contains("help"))
    {
        Console.WriteLine("Usage: USM_builder <input_directory> <ffmpeg_path> <encoder_path> <output_directory> <usm_subtitle_toolbox_path>");
        Console.WriteLine("Example: USM_builder \"input\" \"distr/ffmpeg\" \"distr/Scaleform VideoEncoder\" \"output\" \"USM_subs_toolbox.exe\"");
        return -1;
    }

    Builder builder = new Builder();
    builder.readFileNames();                                // Считываем названия файлов
    VideoInfo videoInfo = builder.convertToAviAndWav();     // Пересобираем файлы с формат пригодный для энкодера
    builder.encode(videoInfo);                              // Собираем в usm

    return 0;
}

class IOStore
{
    public static string input = Environment.GetCommandLineArgs()[1];
    public static string ffmpegPath = Environment.GetCommandLineArgs()[2];
    public static string encoderPath = Environment.GetCommandLineArgs()[3];
    public static string output = Environment.GetCommandLineArgs()[4];
    public static string usmSubtitleToolbox = Environment.GetCommandLineArgs()[5];
}

public struct VideoInfo
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

class Builder
{
    private string _currentDirectory = Directory.GetCurrentDirectory();
    private List<string> _fileNames = new List<string>();

    public void readFileNames()
    {
        // Get all .mp4 files
        var mp4Files = Directory.GetFiles(IOStore.input, "*.mp4");

        // Get all .txt files
        var txtFiles = Directory.GetFiles(IOStore.input, "*.txt");

        // Extract base names without extensions
        var mp4BaseNames = mp4Files.Select(Path.GetFileNameWithoutExtension).ToList();
        var txtBaseNames = txtFiles.Select(Path.GetFileNameWithoutExtension).ToList();

        // Check for each .mp4 file if there is a corresponding .txt file and vice versa
        foreach (var mp4BaseName in mp4BaseNames)
        {
            // mp4BaseName не может быть null, но вдруг кто-то словит
            if (mp4BaseName == null)
            {
                Console.WriteLine($"Ошибка код 01, сообщите разработчикам");
                return;
            }
            // Check if there exists a corresponding .txt file
            if (txtBaseNames.Contains(mp4BaseName + "_en"))
            {
                _fileNames.Add(mp4BaseName);
            }
            else
            {
                Console.WriteLine($"Файл субтитров для '{IOStore.input}/{mp4BaseName}.mp4' не найден");
            }
        }
        // Check result and show is a there missed .mp4 files
        foreach (var txtBaseName in txtBaseNames)
        {
            if (!_fileNames.Contains(txtBaseName.Replace("_en", "")))
            {
                Console.WriteLine($"Файл медиа для '{IOStore.input}/{txtBaseName}.txt' не найден");
            }
        }

        // Результат выполнения
        Console.WriteLine($"Найдено {_fileNames.Count * 2} файла(ов). Можно сгенерировать {_fileNames.Count} .usm файла(ов)");
    }

    public VideoInfo convertToAviAndWav() {
        if (!Directory.Exists(IOStore.output)) Directory.CreateDirectory(IOStore.output);

        const string mp4File = "C:/Users/server/source/repos/USM_builder/USM_builder/bin/Debug/net8.0/input/Story05_CG01.mp4";

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

    public void encode(VideoInfo videoInfo)
    {
        Console.WriteLine(DateTime.Now + " - Передаем файл в Scaleform...");

        string[] hardcode = [
            "C:/Users/server/source/repos/USM_builder/USM_builder/bin/Debug/net8.0/input/Story05_CG01.avi",
            "C:/Users/server/source/repos/USM_builder/USM_builder/bin/Debug/net8.0/input/Story05_CG01.wav",
            "C:/Users/server/source/repos/USM_builder/USM_builder/bin/Debug/net8.0/input/Story05_CG01.txt",
            "C:/Users/server/source/repos/USM_builder/USM_builder/bin/Debug/net8.0/input/Story05_CG01.usm"
            ];

        // .avi, .wav, .txt, {VideoBitrate 885833, AudioBitrate 129498}
        convertInVideoEncoder(hardcode[0], hardcode[1], hardcode[2], videoInfo);
        fixSubtitles(hardcode[3]);
    }

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


public class FilePair
{
    public string media { get; set; }
    public string txt { get; set; }
    public string audio { get; set; }
}