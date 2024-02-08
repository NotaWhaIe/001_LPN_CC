using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Strana.Revit.HoleTask.ElementCollections;
using Strana.Revit.HoleTask.Extension.RevitElement;
using Strana.Revit.HoleTask.Extensions;
using Strana.Revit.HoleTask.Utils;
using Strana.Revit.HoleTask.ViewModel;

namespace Strana.Revit.HoleTask.RevitCommands
{
    [Transaction(TransactionMode.Manual)]
    public class CreateHoleTasks : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            docsaver.doc = doc;

            HoleTaskView taskView = new(doc);
            taskView.ShowDialog();

            // Проверка состояния выполнения после закрытия окна
            if (taskView.ShouldExecuteProgram )
            {
                using (var gt = new TransactionGroup(doc, "HoleTasks"))
                {
                    gt.Start();
                    
                    
                    ///сюда закинукинуть линк по которой строить зно
                    // Здесь выполняется основная логика вашего плагина
                    foreach (RevitLinkInstance linkInstance in LinkInstanseCollections.RevitLinks(doc))
                    {
                        linkInstance.CreateHoleTasksByCurrentLink();
                    }

                    gt.Assimilate();
                }
            }

            //stopwatch.Stop();
            //TimeSpan elapsedTime = stopwatch.Elapsed;
            //TaskDialog.Show("Время работы", elapsedTime.TotalSeconds.ToString() + " сек.");

            return Result.Succeeded;
        }
    }

    public static class docsaver
    {
        public static Document doc { get; set; }
    }
}