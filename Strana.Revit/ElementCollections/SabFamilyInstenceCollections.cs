using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FirstRevitPlugin.FailuresProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strana.Revit.HoleTask.ElementCollections
{
    public class SabFamilyInstenceCollections
    {
        public static void GetFamilyInstenceCollections(Document doc)
        {
            //Document doc = commandData.Application.ActiveUIDocument.Document;

            var nestedFamilyNames = new List<string>
            {
                "(Отв_Задание)_Стены_Прямоугольное",
                "(Отв_Задание)_Перекрытия_Прямоугольное"
            };

            using (Transaction trans = new Transaction(doc, "Копирование вложенных семейств с параметрами"))
            {
                TransactionHandler.SetWarningResolver(trans);
                trans.Start();

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


                                    if (!DoesParentFamilyInstanceExistAtLocation(doc,locationPoint.Point, nestedFamilyNames))
                                    {
                                        FamilyInstance newInstance = CreateFamilyInstanceWithLevel(doc, nestedInstance, locationPoint.Point, chosenLevel, offset);
                                        // Копирование значений параметров
                                        CopyParameters(nestedInstance, newInstance);
                                    }
                                }
                            }
                        }
                    }
                }

                trans.Commit();
            }

        }
        public static Level ChooseLevel(List<Level> levels, double zPoint)
        {
            Level closestLevelBelow = levels.LastOrDefault(l => l.Elevation < zPoint);
            Level closestLevelAbove = levels.FirstOrDefault(l => l.Elevation > zPoint);
            return closestLevelBelow ?? closestLevelAbove;
        }

        public static FamilyInstance CreateFamilyInstanceWithLevel(Document doc, FamilyInstance originalInstance, XYZ point, Level level, double offset)
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

        private static bool DoesParentFamilyInstanceExistAtLocation(Document doc, XYZ location, List<string> familyNames)
        {
            const double tolerance = 0.01; // Небольшой допуск для сравнения координат, около 3 мм

            foreach (FamilyInstance fi in new FilteredElementCollector(doc)
                                                .OfClass(typeof(FamilyInstance))
                                                .WhereElementIsNotElementType()
                                                .Cast<FamilyInstance>()
                                                .Where(fi => familyNames.Contains(fi.Symbol.Family.Name)))
            {
                XYZ existingLocation = (fi.Location as LocationPoint)?.Point;

                if (existingLocation != null && existingLocation.IsAlmostEqualTo(location, tolerance))
                {
                    return true; // Найден существующий экземпляр родительского семейства в заданных координатах
                }
            }

            return false; // Родительское семейство в заданных координатах не найдено
        }

        public static void CopyParameters(FamilyInstance originalInstance, FamilyInstance newInstance)
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

