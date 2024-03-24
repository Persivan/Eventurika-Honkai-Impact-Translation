setlocal enabledelayedexpansion

set "folder_name=Story05_CG01"
set "srt_folder=srt/%folder_name%"
set "log_folder=logs/%folder_name%"

mkdir "!srt_folder!"
mkdir "!log_folder!"

set "usm_video=usm_videos/%folder_name%.usm"

for /l %%i in (0,1,7) do (
    set "file_index=%%i"
    USM_subs_toolbox.exe -srt-convert -single "!srt_folder!/file!file_index!.srt" "!srt_folder!/converted!file_index!.txt" > "!log_folder!/convert_!file_index!.log"
)

endlocal