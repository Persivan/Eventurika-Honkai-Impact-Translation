@echo off
title .usm_all_srt_convert
setlocal enabledelayedexpansion

set "srt_folder=srt"
set "txt_folder=!srt_folder!/converted"

mkdir "!txt_folder!"

"distr/USM_subs_toolbox.exe" -srt-convert -multiple "!srt_folder!" "!txt_folder!"

endlocal