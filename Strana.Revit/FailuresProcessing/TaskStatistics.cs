using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strana.Revit.HoleTask.FailuresProcessing
{
    using Autodesk.Revit.UI; // Убедитесь, что добавили этот using, если работаете с Revit API
    using Strana.Revit.HoleTask.Utils;
    using System;

    public class TaskStatistics
    {
        // Метод для отображения статистики заданий
        public void ShowTaskStatistics(TimeSpan elapsedTime)
        {
            int createdTasksWall =0;
            int createdTasksFloor =0;
            int deletedTasks =0;

            // Формируем сообщение с использованием интерполяции строк
            string message = $"Создано новых заданий (перекрытия): {createdTasksWall}\n" +
                             $"Создано новых заданий (стены): {createdTasksFloor}\n" +
                             $"Всего заданий было (перекрытия): {GlobalParameters.OldTasksFloor}\n" +
                             $"Всего заданий было (стены): {GlobalParameters.OldTasksWall}\n" +
                             $"Удаленных заданий в ходе объединения: {deletedTasks}\n" +
                             $"\nВремя работы: {elapsedTime.Seconds.ToString()} сек.";

            // Отображаем TaskDialog с сформированным сообщением
            TaskDialog.Show("Результат работы", message);
        }
    }
}
