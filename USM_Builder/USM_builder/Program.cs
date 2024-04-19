using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using USM_builder;  // Для получения доступа к VideoInfo
using static System.Net.Mime.MediaTypeNames;

main();

int main()
{
    Console.OutputEncoding = System.Text.Encoding.UTF8;
    // Проверка параметров запуска
    if (args.Length != 5 || args.Contains("-h") || args.Contains("-help"))
    {
        Console.WriteLine("Usage: USM_builder <input_directory> <ffmpeg_path> <ffprobe_path> <encoder_path> <output_directory>");
        Console.WriteLine("Example: USM_builder \"input\" \"distr/ffmpeg/bin/ffmpeg.exe\" \"distr/ffmpeg/bin/ffprobe.exe\" \"distr/Scaleform VideoEncoder\" \"output\"");
        return -1;
    }

#if DEBUG
    Environment.CurrentDirectory = "..\\..\\..\\..\\..\\";
    Console.WriteLine("Рабочая директория: \n" + Environment.CurrentDirectory);
#endif

    Builder builder = new Builder();
    if (builder.readFileNames(IOStore.input) == 0)          // Считываем названия файлов
    {
        Console.WriteLine("Файлы не найдены");
        return 0;                   
    }
    builder.createPlaceHolderSubtitleFile();                // Создание файла если не существует
    builder.createTempFolder();                             // Создание папки если не существует
    builder.encodeAll();                                    // Собираем в usm
    Console.WriteLine("Папку \"usm_builder_temp\" можете удалять.");

    return 0;
}
