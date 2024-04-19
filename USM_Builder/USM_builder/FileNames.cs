using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USM_builder
{
    /// <summary>
    /// Класс для хранения названий файлов
    /// Полностью заполненный обьект класса, означает, что мы можем собрать .usm файл
    /// </summary>
    internal class FileNames
    {
        public FileNames(string filename, string media, string txt, string audio)
        {
            this.filename = filename;
            this.media = media;
            this.txt = txt;
            this.audio = audio;
        }
        public string filename { get; set; } // без расширения
        public string media { get; set; }
        public string txt { get; set; }
        public string audio { get; set; }
    }
}
