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
            MepElementSelector.UIDocument = uidoc;
            Document doc = uidoc.Document;
            docsaver.doc = doc;
            string userName = commandData.Application.Application.Username.ToString();
            GlobalParameters.UserName = userName;

            HoleTaskView taskView = new(doc);
            taskView.ShowDialog();

            List<FamilyInstance> startHoleTask = new();
            List<FamilyInstance> intersectionRectangularWall = new();
            List<FamilyInstance> intersectionRectangularFloor = new();

            ///заменить на один коллектор
            //HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Стены_Прямоугольное", intersectionRectangularWall);
            //HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Перекрытия_Прямоугольное", intersectionRectangularFloor);
            HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Стены_Прямоугольное", intersectionRectangularWall, "(Отв_Задание)_Перекрытия_Прямоугольное", intersectionRectangularFloor);


            GlobalParameters.ЕxistingTaskWall = intersectionRectangularWall;
            GlobalParameters.ЕxistingTaskFloor = intersectionRectangularFloor;
            GlobalParameters.OldTasksWall = intersectionRectangularWall.Count.ToString();
            GlobalParameters.OldTasksFloor = intersectionRectangularFloor.Count.ToString();

           /// moved collectors from cycle


            ///

            if (taskView.ShouldExecuteProgram)
            {
                using (var gt = new TransactionGroup(doc, "HoleTasks"))
                {
                    gt.Start();
                    SabFamilyInstenceCollections.GetFamilyInstenceCollections(doc);
                    var viewModel = taskView.DataContext as HoleTaskViewModel;
                    if (viewModel != null)
                    {
                        var selectedLinkNames = viewModel.GetSelectedLinks()
                            .Select(link =>
                            {
                                var index = link.Name.LastIndexOf('.');
                                return index == -1 ? link.Name : link.Name.Substring(0, index);
                            })
                            .ToList();

                        var linkInstances = new FilteredElementCollector(doc)
                            .OfClass(typeof(RevitLinkInstance))
                            .Cast<RevitLinkInstance>();

                        foreach (RevitLinkInstance linkInstance in linkInstances)
                        {
                            Document linkDoc = linkInstance.GetLinkDocument();
                            if (linkDoc != null && selectedLinkNames.Contains(linkDoc.Title))
                            {
                                linkInstance.CreateHoleTasksByCurrentLink();
                            }
                        }
                    }
                    gt.Assimilate();
                }
            }

            List<FamilyInstance> intersectionWallRectangularCombineList01 = new();
            List<FamilyInstance> intersectionFloorRectangularCombineList02 = new();
            HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Стены_Прямоугольное", intersectionWallRectangularCombineList01);
            HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Перекрытия_Прямоугольное", intersectionFloorRectangularCombineList02);
            double createdTasksWall = intersectionWallRectangularCombineList01.Count - intersectionRectangularWall.Count;
            GlobalParameters.СreatedTasksWall = (createdTasksWall > 0 ? createdTasksWall : 0).ToString();
            double createdTasksFloor = intersectionFloorRectangularCombineList02.Count - intersectionRectangularFloor.Count;
            GlobalParameters.СreatedTasksFloor = (createdTasksFloor > 0 ? createdTasksFloor : 0).ToString();
            double deletedTasks = -((createdTasksFloor < 0 ? createdTasksFloor : 0) + (createdTasksWall < 0 ? createdTasksWall : 0));
            GlobalParameters.DeletedTasks = deletedTasks.ToString();

            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            var taskStatistics = new TaskStatistics();
            taskStatistics.ShowTaskStatistics(elapsedTime);

            return Result.Succeeded;
        }
        public static List<ElementId> GetFamilyInstanceIds(List<FamilyInstance> rectangularCombineList)
        {
            List<ElementId> sortedIds = rectangularCombineList
                .Select(instance => instance.Id)
                .OrderBy(id => id.IntegerValue)
                .ToList();

            return sortedIds;
        }
    }

    public static class docsaver
    {
        public static Document doc { get; set; }
    }
}