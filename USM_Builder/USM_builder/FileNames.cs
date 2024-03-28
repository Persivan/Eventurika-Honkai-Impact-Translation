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
        public string media { get; set; }
        public string txt { get; set; }
        public string audio { get; set; }
    }
}
