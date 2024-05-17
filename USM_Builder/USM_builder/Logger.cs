using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USM_builder
{
    internal class Logger
    {
        /// <summary>
        /// Функция для вывода логов
        /// В ней происходит добавление всех нужных полей. Например, текущее время
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLine(String message)
        {
            List<String> messageContainer = new List<string>() { DateAndTime.Now.ToString() };

            // Выход если отсутствует сообщение
            if (String.IsNullOrEmpty(message))
            {
                messageContainer.Add("Ошибка - код 03, сообщите разработчикам");
                Console.WriteLine(String.Join(" - ", messageContainer));
                return;
            }

            // Добавление блоков с текстом
            messageContainer.Add(message);

            // Вывод сообщения
            Console.WriteLine(String.Join(" - ", messageContainer));
        }
    }
}
