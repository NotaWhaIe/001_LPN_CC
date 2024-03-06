using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Strana.Revit.HoleTask.ElementCollections;
using Strana.Revit.HoleTask.Extension.RevitElement;
using Strana.Revit.HoleTask.Extensions;
using Strana.Revit.HoleTask.Extensions.RevitElement;
using Strana.Revit.HoleTask.FailuresProcessing;
using Strana.Revit.HoleTask.Utils;
using Strana.Revit.HoleTask.ViewModel;

namespace Strana.Revit.HoleTask.RevitCommands
{
    [Transaction(TransactionMode.Manual)]
    public class CreateHoleTasks : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            GlobalParameters.ResetParameters();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            docsaver.doc = doc;
            string userName = commandData.Application.Application.Username.ToString();
            GlobalParameters.UserName = userName;

            HoleTaskView taskView = new(doc);
            taskView.ShowDialog();

            List<FamilyInstance> startHoleTask = new();
            List<FamilyInstance> intersectionRectangularWall = new();
            List<FamilyInstance> intersectionRectangularFloor = new();
            HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Стены_Прямоугольное", intersectionRectangularWall);
            HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Перекрытия_Прямоугольное", intersectionRectangularFloor);

            
            GlobalParameters.ЕxistingTaskWall = intersectionRectangularWall;
            GlobalParameters.ЕxistingTaskFloor = intersectionRectangularFloor;
            GlobalParameters.OldTasksWall = intersectionRectangularWall.Count.ToString();
            GlobalParameters.OldTasksFloor = intersectionRectangularFloor.Count.ToString();
            
            startHoleTask.AddRange(intersectionRectangularWall.Concat(intersectionRectangularFloor));
            List<ElementId> startHoleTaskID = GetFamilyInstanceIds(startHoleTask);
            //Log.SaveListToFile(@"C:\Users\nikiforov.n\Desktop\ids0.txt", startHoleTaskID);

            // Проверка состояния выполнения после закрытия окна
            if (taskView.ShouldExecuteProgram)
            {
                using (var gt = new TransactionGroup(doc, "HoleTasks"))
                {

                    gt.Start();

                    SabFamilyInstenceCollections.GetFamilyInstenceCollections(doc);

                    // Получаем ViewModel из DataContext
                    var viewModel = taskView.DataContext as HoleTaskViewModel;
                    if (viewModel != null)
                    {
                        // Получаем имена выбранных связей
                        var selectedLinkNames = viewModel.GetSelectedLinks()
                            .Select(link =>
                            {
                                var index = link.Name.LastIndexOf('.'); // Находим индекс последней точки
                                return index == -1 ? link.Name : link.Name.Substring(0, index); // Обрезаем суффикс, если точка найдена
                            })
                            .ToList();

                        // Итерация по всем экземплярам RevitLinkInstance в документе
                        var linkInstances = new FilteredElementCollector(doc)
                            .OfClass(typeof(RevitLinkInstance))
                            .Cast<RevitLinkInstance>();

                        foreach (RevitLinkInstance linkInstance in linkInstances)
                        {
                            Document linkDoc = linkInstance.GetLinkDocument();
                            // Проверяем, загружена ли связь и совпадает ли её документ с одним из выбранных
                            if (linkDoc != null && selectedLinkNames.Contains(linkDoc.Title)) // Условие включено
                            {
                                // Обработка только для выбранных связей
                                linkInstance.CreateHoleTasksByCurrentLink();
                            }
                        }

                    }
                    gt.Assimilate();
                }
            }

            List<FamilyInstance> finishHoleTask = new();
            List<FamilyInstance> intersectionWallRectangularCombineList01 = new();
            List<FamilyInstance> intersectionFloorRectangularCombineList02 = new();
            HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Стены_Прямоугольное", intersectionWallRectangularCombineList01);
            HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Перекрытия_Прямоугольное", intersectionFloorRectangularCombineList02);
            GlobalParameters.СreatedTasksWall = (intersectionWallRectangularCombineList01.Count - intersectionRectangularWall.Count).ToString();
            GlobalParameters.СreatedTasksFloor = (intersectionFloorRectangularCombineList02.Count - intersectionRectangularFloor.Count).ToString();
            finishHoleTask.AddRange(intersectionWallRectangularCombineList01.Concat(intersectionFloorRectangularCombineList02));

            List<ElementId> finishHoleTaskID = GetFamilyInstanceIds(finishHoleTask);
            //Log.SaveListToFile(@"C:\Users\nikiforov.n\Desktop\ids1.txt", finishHoleTaskID);

            List<ElementId> resultIds = CompareAndFilterIds(startHoleTaskID, finishHoleTaskID);
            //Log.SaveListToFile(@"C:\Users\nikiforov.n\Desktop\resultIds.txt", resultIds);


            List<ElementId> result = CompareAndFilterIds(startHoleTaskID, finishHoleTaskID);

            int matchCount = TaskProcessor.CountNonMatchingInsertionPoints(GlobalParameters.ЕxistingTask,finishHoleTask,resultIds);
            GlobalParameters.DeletedTasks = matchCount.ToString();


            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            var taskStatistics = new TaskStatistics();
            taskStatistics.ShowTaskStatistics(elapsedTime);

            return Result.Succeeded;
        }
        public static List<ElementId> GetFamilyInstanceIds(List<FamilyInstance> rectangularCombineList)
        {
            // Используем LINQ для выборки Id из каждого FamilyInstance и сортировки их по числовому значению
            List<ElementId> sortedIds = rectangularCombineList
                .Select(instance => instance.Id)
                .OrderBy(id => id.IntegerValue) // Сортируем Id по их числовому значению
                .ToList();

            return sortedIds;
        }
        public static List<ElementId> CompareAndFilterIds(List<ElementId> ids0, List<ElementId> ids1)
        {
            if (ids0.Count == 0)
            {
                return new List<ElementId>(); // Возвращаем пустой список, если ids0 пуст
            }

            // Находим максимальное числовое значение среди ElementId в ids0
            int maxIdValue = ids0.Max(id => id.IntegerValue);
            ElementId maxIdInIds0 = new ElementId(maxIdValue);

            // Удаляем из ids1 все элементы, чьё числовое значение больше максимального значения в ids0
            ids1 = ids1.Where(id => id.IntegerValue <= maxIdInIds0.IntegerValue).ToList();

            // Возвращаем элементы, которые есть в ids0, но отсутствуют в ids1
            // Здесь нет необходимости изменять логику, так как Except использует Equals и GetHashCode методы ElementId,
            // которые должны корректно обрабатываться даже без прямого сравнения IntegerValue
            return ids0.Except(ids1).ToList();
        }
    }

    public static class docsaver
    {
        public static Document doc { get; set; }
    }
}