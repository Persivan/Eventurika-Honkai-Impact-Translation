using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using USM_builder;  // Для получения доступа к VideoInfo
using static System.Net.Mime.MediaTypeNames;

Main();

int Main()
{
    // Смена стандартной кодировки, чтобы на англоязычных системах выводились кириллица (иначе будет "???? ?? ??" вместо "ужас да да")
    Console.OutputEncoding = System.Text.Encoding.UTF8;

    // Проверка параметров запуска
    if ((args.Length < 5 || args.Length > 6) || args.Contains("-h") || args.Contains("-help") || args.Contains(String.Empty))
    {
        Console.WriteLine("Usage: USM_builder <input_directory> <ffmpeg_path> <ffprobe_path> <encoder_path> <output_directory> (-doNotUseSubtitles)");
        Console.WriteLine("Example: USM_builder \"input\" \"distr/ffmpeg/bin/ffmpeg.exe\" \"distr/ffmpeg/bin/ffprobe.exe\" \"distr/Scaleform VideoEncoder/medianocheH264.exe\" \"output\" -doNotUseSubtitles");
        Console.WriteLine("");
        Console.WriteLine("Parameteres:");
        Console.WriteLine("  \"-doNotUseSubtitles\" is optional parameter for ignoring .txt files.");
        return -1;
    }

#if DEBUG
    Environment.CurrentDirectory = "..\\..\\..\\..\\..\\";
    Console.WriteLine("Рабочая директория: \n" + Environment.CurrentDirectory);
#endif

    IOStore.Initialize(args);

    Builder builder = new();
    if (builder.ReadFileNames(IOStore.inputFolder) == 0)          // Считываем названия файлов
    {
        Console.WriteLine("Файлы не найдены");
        return 0;
    }

    if (!IOStore.doNotUseSubtitles)
    {
        builder.CreatePlaceHolderSubtitleFile();            // Создание файла если не существует
    }
    builder.CreateFolder("usm_builder_temp/");              // Создание папки если не существует
    builder.CreateFolder(IOStore.outputFolder);                   // Создание папки если не существует
    builder.EncodeAll();                                    // Собираем в usm
    Console.WriteLine("Папку \"usm_builder_temp\" можете удалять.");

    return 0;
}
