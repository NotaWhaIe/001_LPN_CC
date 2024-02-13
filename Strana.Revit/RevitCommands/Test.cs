using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Strana.Revit.HoleTask.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
//using Autodesk.DesignScript.Geometry;

namespace Strana.Revit.HoleTask.RevitCommands
{
    [Autodesk.Revit.Attributes.TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Test : IExternalCommand
    {
        static AddInId addinId = new AddInId(new Guid("f64706dd-e8f6-4cbe-9cc6-a2910be5ad5a"));
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Autodesk.Revit.DB.Document doc = commandData.Application.ActiveUIDocument.Document;

            // Список имен вложенных семейств
            var nestedFamilyNames = new List<string>
        {
            "(Отв_Задание)_Стены_Прямоугольное",
            "(Отв_Задание)_Перекрытия_Прямоугольное"
        };

            // Найти все экземпляры вложенных семейств в проекте
            var nestedInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(fi => nestedFamilyNames.Contains(fi.Symbol.FamilyName))
                .ToList();

            using (Transaction trans = new Transaction(doc, "Размещение копий вложенных семейств"))
            {
                trans.Start();

                foreach (var nestedInstance in nestedInstances)
                {
                    // Получаем местоположение и уровень для каждого экземпляра вложенного семейства
                    LocationPoint locationPoint = nestedInstance.Location as LocationPoint;
                    Level level = doc.GetElement(nestedInstance.LevelId) as Level;

                    if (locationPoint != null && level != null)
                    {
                        // Активируем тип семейства, если он не активен
                        if (!nestedInstance.Symbol.IsActive)
                            nestedInstance.Symbol.Activate();

                        // Создаем новый экземпляр семейства в том же месте и на том же уровне
                        doc.Create.NewFamilyInstance(locationPoint.Point, nestedInstance.Symbol, level, StructuralType.NonStructural);
                    }
                }

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}
