@echo off
title .usm_build
:: Использование: USM_builder <input_directory> <ffmpeg_path> <ffprobe_path> <encoder_path> <output_directory>
"./distr/USM_Builder/USM_builder.exe" "usm_builder_input" "distr/ffmpeg/bin/ffmpeg.exe" "distr/ffmpeg/bin/ffprobe.exe" "distr/Scaleform VideoEncoder/medianocheH264.exe" "usm_builder_output"
pause