@echo off
title .usm_srt_extract_en
setlocal enabledelayedexpansion

::set "folder_name=Story05_CG01"
set /p "folder_name=Enter file name (ex: Story05_CG01): "
set "srt_folder=srt/%folder_name%"
set "log_folder=logs/%folder_name%"

mkdir "!srt_folder!"
mkdir "!log_folder!"

set "usm_video=usm_videos/%folder_name%.usm"

set "lang_index=1"
"distr/USM_subs_toolbox.exe" -extractSbt "!usm_video!" "!srt_folder!/lang_!lang_index!.srt" -l !lang_index! > "!log_folder!/extract_lang_!lang_index!.log"

endlocal