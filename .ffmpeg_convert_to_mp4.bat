set ffmpeg_path="C:\Files\Projects\Mihoyo depacker\patcher\distr\ffmpeg\bin\ffmpeg.exe"
set m2v_file="vgmtoolbox_output\Story05_CG01_40534656"
set hca_file="vgmtoolbox_output\Story05_CG01_40534641"
set output_file="ffmpeg_output\Story05_CG01"

mkdir ffmpeg_output

%ffmpeg_path% -i %m2v_file%.m2v -i %hca_file%.hca -c:v copy %output_file%.mp4