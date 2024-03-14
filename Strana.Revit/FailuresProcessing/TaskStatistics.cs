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
            string message = $"Новых заданий (перекрытия): {GlobalParameters.СreatedTasksFloor}\n" +
                             $"Новых заданий (стены): {GlobalParameters.СreatedTasksWall}\n" +
                             $"Было заданий (перекрытия): {GlobalParameters.OldTasksFloor}\n" +
                             $"Было заданий (стены): {GlobalParameters.OldTasksWall}\n" +
                             $"Удалено (после объединения): {GlobalParameters.DeletedTasks}\n" +
                             $"\nВремя работы: {elapsedTime.Seconds.ToString()} сек.";

            // Отображаем TaskDialog с сформированным сообщением
            TaskDialog.Show("Результат работы", message);
        }
    }
}
