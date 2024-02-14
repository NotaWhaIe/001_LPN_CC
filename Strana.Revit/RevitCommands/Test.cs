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

            var nestedFamilyNames = new List<string>
    {
        "(Отв_Задание)_Стены_Прямоугольное",
        "(Отв_Задание)_Перекрытия_Прямоугольное"
    };

            using (Transaction trans = new Transaction(doc, "Копирование вложенных семейств"))
            {
                trans.Start();

                var allInstances = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .ToList();

                foreach (var instance in allInstances)
                {
                    var dependentIds = instance.GetSubComponentIds();
                    foreach (ElementId id in dependentIds)
                    {
                        FamilyInstance nestedInstance = doc.GetElement(id) as FamilyInstance;
                        if (nestedInstance != null && nestedFamilyNames.Contains(nestedInstance.Symbol.FamilyName))
                        {
                            // Активируем символ семейства, если он не активен
                            if (!nestedInstance.Symbol.IsActive)
                            {
                                nestedInstance.Symbol.Activate();
                                doc.Regenerate();
                            }

                            LocationPoint locationPoint = nestedInstance.Location as LocationPoint;
                            if (locationPoint != null)
                            {
                                // Создание копии вложенного семейства в той же точке
                                doc.Create.NewFamilyInstance(locationPoint.Point, nestedInstance.Symbol, StructuralType.NonStructural);
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

