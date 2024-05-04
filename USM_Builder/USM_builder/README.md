# Запуск
1. Указать пути в `start.bat`. Пример:
`./USM_Builder.exe "usm_builder_input" "distr/ffmpeg/bin/ffmpeg.exe" "distr/ffmpeg/bin/ffprobe.exe" "distr/Scaleform VideoEncoder/medianocheH264.exe" "usm_builder_output" "distr/USM_subs_toolbox.exe"`
* "usm_builder_input" - путь до папки с файлами .avi (видео), .txt (субтитры), .wav (аудио)
* "distr/ffmpeg/bin/ffmpeg.exe" - путь до ffmpeg.exe
* "distr/ffmpeg/bin/ffprobe.exe" - путь до ffprobe.exe
* "distr/Scaleform VideoEncoder/medianocheH264.exe" - путь до энкодера
* "usm_builder_output" - путь до папки с результатами (будет создана если не существует)
* "distr/USM_subs_toolbox.exe" - путь патчера для сабов
2. Запускаем `./start.bat`


# ПАПКА INPUT!
Для генерации одного .usm файла в папке "input" должно быть 3 файла
1. FILENAME.avi - видео
2. FILENAME.wav - аудио
3. FILENAME_en.txt/FILENAME.txt - сабы
