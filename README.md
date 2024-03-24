# Описание
**English soon, btw just use translator.**
Этот репозиторий содержит все для работы с .usm синематиками игры [Honkai Impact 3](https://honkaiimpact3.hoyoverse.com/global/en-us/home).
Перечисленные здесь инструменты позволят вам открыть файлы с синематиками расположенными `\Games\BH3_Data\StreamingAssets\Video`.
Получить видео дорожку. Получить аудио дорожку. Получить несколько дорожек с субтитрами. Заменить видео, аудио, субтитры.
Каждый синематик - это .usm файл.
Все синематики можно просмотреть в игре "Телефон" -> "Collection" -> "Cutscenes". 

# Программы
**Экспортировать сабы из USM / Конвертировать сабы для будущей упаковки** - USM_subs_toolbox.exe - прога, использую [билд с гита](https://github.com/Devyatyi9/HI3rd_usm_sbt_patcher/releases/tag/test2). Размещена в корне проекта.

**Конвертировать медиа после VGMToolBox** - ffmpeg, использую [билд отсюда](https://www.gyan.dev/ffmpeg/builds/). Размещена, где угодно. В скрипте указывается путь до нее.

**Экспортировать медиа и аудио из usm** - VGMToolBox, использую [билд отсюда](https://sourceforge.net/projects/vgmtoolbox/), подробнее `vgmtoolbox_output/readme.md`. Размещена, где угодно. В скриптах не используется.

**Упаковщик USM** - USM_builder - запаковывает файлы обратно в формат, пригодный игре.

# Как заменить субтитры
0. Из игры не требуется выходить. Все послед. действия можно выполнять с запущенной игрой.
1. Помещаем интересующий нас файл из `BH3_Data/StreamingAssets/Video` в `usm_videos`
Например `Story05_CG01.usm`
`cp "C:\Program Files\Honkai Impact 3rd glb\Games\BH3_Data\StreamingAssets\Video\Story05_CG01.usm" "C:\Files\Projects\Mihoyo depacker\patcher\unpack\usm_videos\Story05_CG01.usm"`
2. В файле `.usm_extract.bat` указываем файл, без расширения
`set "folder_name=Story05_CG01"`
3. Запускаем экспорт субтитров `.usm_extract.bat`
При выполнение будут созданы папки `logs/Story05_CG01` и `srt/Story05_CG01`. В `srt/Story05_CG01` будут храниться все хранимые в файле субтитры.
Если каких-то не хватает, можно проверить `logs/Story05_CG01`. Надпись `Subtitles not found in this file` означает, что по указанному номеру нет субтитров.
Это первый этап на котором вы можете переводить текст в файле с англ субтитрами `file1.srt`. Если файла `file1.srt` нет, то англ. субтитров нет для этого файла. Для такого файла русские субтитры создать не получится.
Сейчас мы способны изменить только англ дорожку субтитров. Это задевает все синематики "Дни рожденья" т.к. там видеоряд - это письмо на англ языке. Напишите в раздел **Issues**, если найдете еще.  
4. В файле `.usm_convert.bat` указываем файл, без расширения
`set "folder_name=Story05_CG01"`
5. Запускаем конвертацию файла субтитров
`.usm_convert.bat`
При выполнение будут созданы папки `logs/Story05_CG01` и `srt/Story05_CG01` (Если вы не пропускали прошлые пункты, то должно выдать ошибку `A subdirectory or file srt/Story05_CG01 already exists.` и `A subdirectory or file log/Story05_CG01 already exists.`).
6. В `srt/Story05_CG01` будут храниться конвертированные файлы субтитров.
Конвертирование нужна для будущей сборки.
Это второй этап, последний этап, на котором вы можете переводить текст в файле с англ субтитрами `converted1.srt`.
6. Переведенный файл кладем в папку `usm_builder_input` и называем его `Story05_CG01_en.txt`
7. Открываем VGMToolBox. В интерфейсы программы: Misc. Tools -> Stream Tools -> Video Demultiplexer -> Формат "USM (CRI Movie 2)"
8. Закидываем файл из `usm_videos` (Story05_CG01.usm)
В этой же папке будет создана два файла: `Story05_CG01_40534656.m2v` и `Story05_CG01_40534641.hca`. m2v - видео-дорожка. hca - аудио-дорожка.
9. Переносим эти файлы (`Story05_CG01_40534656.m2v` и `Story05_CG01_40534641.hca`) в папку `vgmtoolbox_output`.
10. В файле `.ffmpeg_convert_to_mp4.bat` меняем
ffmpeg_path - путь да исполняемого файла ffmpeg
m2v_file - путь до видео-дорожки
hca_file - путь до аудио-дорожки
output_file - названия для будущего mp4 файла 
```
set ffmpeg_path="C:\Files\Projects\Mihoyo depacker\patcher\distr\ffmpeg\bin\ffmpeg.exe"
set m2v_file="vgmtoolbox_output\Story05_CG01"
set hca_file="vgmtoolbox_output\Story05_CG01"
set output_file="ffmpeg_output\Story05_CG01"
```
11. Запускаем сборку mp4 файла `.ffmpeg_convert_to_mp4.bat`
Это создаст папку `ffmpeg_output` и в ней mp4 файл с видео и аудио `Story05_CG01.mp4`
12. Собранный файл кладем в папку `usm_builder_input`.
На данном этапе в папке должно быть два файла: `Story05_CG01.mp4` и `Story05_CG01_en.txt`
13. Открываем USM_builder (для его работы требуется net framework).
В нем указываем пути до ffmpeg. Теперь не до исполняемого файла, а до папки bin
Путь до Scaleform VideoEncoder
14. Тыкаем Старт
Прога подглючивает, начинает не с первого раза. Успешный старт - если открылось еще одно окно и посыпались строчки с процентами прогресса.
Процесс занимает 2-3 минуты И ЗАВЕРШАЕТСЯ КРАШЕМ. Однако, файл сгенерирован и храниться в `usm_builder_output`
15. Переносим файл обратно в папку с игрой `BH3_Data/StreamingAssets/Video` с заменой файла. Старый файл мы сохранили в `usm_videos`
16. Провеярем результат через катсцены во внутриигровой коллекции. Повторюсь, перезаходить в игру не требуется. 

# Обьяснения форматов файлов
* **txt** - что угодно
* **srt** - субтитры
* **usm** - видеоролики (медиа, аудио, субтитры в одном файле)
* **m2v** - медиа файл после vgmtoolbox (без аудио и сабов)
* **hca** - аудио файл после vgmtoolbox (без аудио и сабов)
* **mp4** - видеоролик (медиа, аудио, сабы в зависимости от сборки)
* **bat** - самодельные скрипты
* **md** - файлы документации
* **exe** - файлы дистрибутив

# Скрипты
Написал скрипт `extract.bat` - достает из usm файла сабы
1. Помещаем файл из `BH3_Data\StreamingAssets\Video` в `./usm_videos/`
2. В строке `set "folder_name=6.5_Birthday_Mei_efbc04aec45adb226958ad3f7582d70a"` изменяем название файла
3. Запускаем скрипт
Достанет 0-7 дорожки сабов в папку `./srt/6.5_Birthday_Mei_efbc04aec45adb226958ad3f7582d70a/`, лог в `./logs/6.5_Birthday_Mei_efbc04aec45adb226958ad3f7582d70a/`

Написал скрипт `convert.bat` - конвертирует формат сабов. Перед запуском требуется запустить extract.bat, чтобы он создал файлы .srt
1. В строке `set "folder_name=6.5_Birthday_Mei_efbc04aec45adb226958ad3f7582d70a"` изменяем название файла
2. Запускаем скрипт

Написал скрипт `ffmpeg_convert_m2v+hca_to_mp4.bat` - конвертирует m2v+hca в mp4 (с аудиодорожкой)
1. Указать путь до ffmpeg `set ffmpeg_path="C:\Files\Projects\Mihoyo depacker\patcher\distr\ffmpeg\bin\ffmpeg.exe"`
2. Указать путь до m2v файла `set m2v_file="vgmtoolbox_output\6.5_Birthday_Mei_efbc04aec45adb226958ad3f7582d70a_40534656"`
3. Указать путь до hca файла `set hca_file="vgmtoolbox_output\6.5_Birthday_Mei_efbc04aec45adb226958ad3f7582d70a_40534641"`
4. Указать путь для выходного видео файла `set output_file="ffmpeg_output\output_m2v_to_mp4"`
5. Запускаем скрипт

# Особенности
## Хранение файлов
Все usm файлы надо хранить в `./usm_videos`. Запуск в таком случае `Test.exe -extractSbt "usm_videos/kiana.usm" -l 4`. файл с сабами будет лежать в usm_videos/kiana.srt. Иначе будет ошибка: `Error : [file_open,null/test.srt]`
## Индекс сабов
В некоторых файлах нет англ сабов (1 индекс), т.к. на экране буквально англ текст. После сборки подобных файлов мы все равно не получим сабы на экране
## Сборка USM файла
Выкидывает исключение, но **один** файл собирается успешно

# Языки
Файл `usm_videos/7.1_Birthday_Kiana_21f3d1f775e2938ac9c205e53d88bd82.usm`

0. Китайский (упрощенный)
1. **НЕТ ДОРОЖКИ!?** (видимо тут должен быть английский, но т.к. на видеоряде англ буковки, то решили ничего не писать)
2. Вьетнамский
3. Тайский
4. Французский
5. Немецкий
6. Индонезийский

Файл `usm_videos/2.6_CG111_mux.usm`

0. Китайский
1. Английский
2. ...

Файл `6.5_Birthday_Mei_efbc04aec45adb226958ad3f7582d70a.usm`

0. Китайский (упрощенный)
1. **НЕТ ДОРОЖКИ!?** (видимо тут должен быть английский, но т.к. на видеоряде англ буковки, то решили ничего не писать)
2. Вьетнамский
3. Тайский
4. Французский
5. Немецкий
6. Индонезийский

# Зависимости
[Devyatyi9/HI3rd_usm_sbt_patcher](https://github.com/Devyatyi9/HI3rd_usm_sbt_patcher/releases/tag/test2)
[Программа для сборки USM]()