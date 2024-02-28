using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strana.Revit.HoleTask.FailuresProcessing
{
    using Autodesk.Revit.UI; // Убедитесь, что добавили этот using, если работаете с Revit API
    using System;

    public class TaskStatistics
    {
        // Метод для отображения статистики заданий
        public void ShowTaskStatistics(TimeSpan elapsedTime, int createdTasks, int totalTasks, int deletedTasks)
        {
            // Формируем сообщение с использованием интерполяции строк
            string message = $"Время работы: {elapsedTime.TotalSeconds.ToString()} сек.\n" +
                             $"Создано новых заданий: {createdTasks}\n" +
                             $"Всего заданий в проекте было: {totalTasks}\n" +
                             $"Удаленных заданий в ходе объединения: {deletedTasks}";

            // Отображаем TaskDialog с сформированным сообщением
            TaskDialog.Show("Результат работы", message);
        }
    }
}
