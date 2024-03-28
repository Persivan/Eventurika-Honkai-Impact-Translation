using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using USM_builder;  // Для получения доступа к VideoInfo
using static System.Net.Mime.MediaTypeNames;

main();

int main()
{
    // Проверка параметров запуска
    if (args.Length != 5 || args.Contains("-h") || args.Contains("-help"))
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
