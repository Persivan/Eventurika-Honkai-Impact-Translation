@echo off
title .usm_srt_extract
setlocal enabledelayedexpansion

::set "folder_name=Story05_CG01"
set /p "folder_name=Enter file name (ex: Story05_CG01): "
set "srt_folder=srt/%folder_name%"
set "log_folder=logs/%folder_name%"

mkdir "!srt_folder!"
mkdir "!log_folder!"

set "usm_video=usm_videos/%folder_name%.usm"

for /l %%i in (0,1,7) do (
    set "file_index=%%i"
    "distr/USM_subs_toolbox.exe" -extractSbt "!usm_video!" "!srt_folder!/lang_!file_index!.srt" -l !file_index! > "!log_folder!/extract_lang_!file_index!.log"
)

endlocal