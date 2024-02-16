using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using FirstRevitPlugin.FailuresProcessing;
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

            using (Transaction trans = new Transaction(doc, "Копирование вложенных семейств с параметрами"))
            {
                trans.Start();
                TransactionHandler.SetWarningResolver(trans);

                var levels = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .OrderBy(l => l.Elevation)
                    .ToList();

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
                            LocationPoint locationPoint = nestedInstance.Location as LocationPoint;
                            if (locationPoint != null)
                            {
                                Level chosenLevel = ChooseLevel(levels, locationPoint.Point.Z);

                                if (chosenLevel != null)
                                {
                                    double offset = locationPoint.Point.Z - chosenLevel.Elevation;
                                    FamilyInstance newInstance = CreateFamilyInstanceWithLevel(doc, nestedInstance, locationPoint.Point, chosenLevel, offset);

                                    // Копирование значений параметров
                                    CopyParameters(nestedInstance, newInstance);
                                }
                            }
                        }
                    }
                }

                trans.Commit();
            }

            return Result.Succeeded;
        }

        private Level ChooseLevel(List<Level> levels, double zPoint)
        {
            Level closestLevelBelow = levels.LastOrDefault(l => l.Elevation < zPoint);
            Level closestLevelAbove = levels.FirstOrDefault(l => l.Elevation > zPoint);
            return closestLevelBelow ?? closestLevelAbove;
        }

        private FamilyInstance CreateFamilyInstanceWithLevel(Document doc, FamilyInstance originalInstance, XYZ point, Level level, double offset)
        {
            FamilySymbol symbol = originalInstance.Symbol;
            FamilyInstance newInstance = doc.Create.NewFamilyInstance(point, symbol, level, StructuralType.NonStructural);
            Parameter elevationParam = newInstance.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
            if (elevationParam != null && elevationParam.IsReadOnly == false)
            {
                elevationParam.Set(offset);
            }
            return newInstance;
        }

        private void CopyParameters(FamilyInstance originalInstance, FamilyInstance newInstance)
        {
            var parameterNames = new List<string> { "Глубина", "Ширина", "Высота" };

            foreach (string paramName in parameterNames)
            {
                Parameter originalParam = originalInstance.LookupParameter(paramName);
                Parameter newParam = newInstance.LookupParameter(paramName);

                // Проверяем, существуют ли параметры и не является ли параметр нового экземпляра только для чтения
                if (originalParam != null && newParam != null && !newParam.IsReadOnly)
                {
                    // Копируем значение из оригинального экземпляра в новый
                    double value = originalParam.AsDouble();
                    newParam.Set(value);
                }
            }
        }
    }
}

