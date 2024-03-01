using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strana.Revit.HoleTask.FailuresProcessing
{
    public class ListProcessor
    {
        // Метод принимает два списка FamilyInstance и возвращает строку с информацией
        public string ProcessLists(List<FamilyInstance> list1, List<FamilyInstance> list2)
        {
            // Вычисляем общее количество элементов в обоих списках
            int totalCount = list1.Count + list2.Count;

            // Собираем информацию о каждом списке
            string list1Info = $"Первый список содержит {list1.Count} элементов.";
            string list2Info = $"Второй список содержит {list2.Count} элементов.";

            // Собираем и возвращаем общую информацию
            string result = $"Общая информация:\n{list1Info}\n{list2Info}\nВсего элементов: {totalCount}.";
            return result;
        }
    }
}
