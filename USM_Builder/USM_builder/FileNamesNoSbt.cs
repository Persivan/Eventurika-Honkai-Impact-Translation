using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USM_builder
{
    /// <summary>
    /// Класс для хранения названий файлов без субтитров
    /// Полностью заполненный обьект класса, означает, что мы можем собрать .usm файл
    /// </summary>
    internal class FileNamesNoSbt
    {
        public FileNamesNoSbt(string filename, string media, string audio)
        {
            this.filename = filename;
            this.media = media;
            this.audio = audio;
        }
        public string filename { get; set; } // без расширения
        public string media { get; set; }
        public string audio { get; set; }
    }
}
