using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Strana.Revit.HoleTask.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Document doc = commandData.Application.ActiveUIDocument.Document;

            // Имена вложенных семейств
            var nestedFamilyNames = new List<string>
    {
        "(Отв_Задание)_Стены_Прямоугольное",
        "(Отв_Задание)_Перекрытия_Прямоугольное"
    };

            using (Transaction trans = new Transaction(doc, "Копирование вложенных семейств"))
            {
                trans.Start();

                // Поиск всех экземпляров семейств в проекте
                var allInstances = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .ToList();

                foreach (var instance in allInstances)
                {
                    // Получение вложенных семейств для каждого экземпляра
                    var nestedFamilies = instance.GetSubComponentIds().Select(id => doc.GetElement(id) as FamilyInstance);

                    foreach (var nestedInstance in nestedFamilies)
                    {
                        // Проверка, соответствует ли вложенное семейство заданным именам
                        if (nestedInstance != null && nestedFamilyNames.Contains(nestedInstance.Symbol.FamilyName))
                        {
                            // Копирование вложенного семейства
                            LocationPoint locationPoint = nestedInstance.Location as LocationPoint;
                            if (locationPoint != null)
                            {
                                ElementId levelId = nestedInstance.LevelId;
                                FamilySymbol symbol = nestedInstance.Symbol;

                                // Создание копии вложенного семейства
                                doc.Create.NewFamilyInstance(locationPoint.Point, symbol, doc.GetElement(levelId) as Level, StructuralType.NonStructural);
                            }
                        }
                    }
                }

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}

