@echo off
title .usm_all_srt_extract
setlocal enabledelayedexpansion

set "srt_folder=srt"

mkdir "!srt_folder!"

set "usm_folder=usm_videos"

set "lang_index=1" :: lang 1 = english
"distr/USM_subs_toolbox.exe" -extractSbt "!usm_folder!" "!srt_folder!" -l !lang_index!

endlocal