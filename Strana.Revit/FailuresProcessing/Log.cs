using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strana.Revit.HoleTask.FailuresProcessing
{
    public class Log
    {
        public static void SaveListToFile<T>(string filePath, List<T> list)
        {
            // Используем using, чтобы автоматически закрыть файл после записи
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (T item in list)
                {
                    // Преобразуем каждый элемент списка в строку и записываем в файл
                    writer.WriteLine(item.ToString());
                }
            }
        }
    }
}
