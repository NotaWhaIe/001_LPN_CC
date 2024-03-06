using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strana.Revit.HoleTask.FailuresProcessing
{
    public static class TaskProcessor
    {
        public static int CountNonMatchingInsertionPoints(
    List<FamilyInstance> startHoleTask,
    List<FamilyInstance> finishHoleTask,
    List<ElementId> result)
        {
            int nonMatchCount = 0;

            // Преобразуем finishHoleTask в словарь для ускорения поиска
            Dictionary<ElementId, XYZ> finishHoleTaskLocations = finishHoleTask.ToDictionary(
                fi => fi.Id,
                fi => (fi.Location as LocationPoint)?.Point);

            // Словарь для хранения локаций startHoleTask для ускорения проверки совпадений
            var startHoleTaskLocations = startHoleTask
                .Select(fi => (fi.Location as LocationPoint)?.Point)
                .Where(loc => loc != null)
                .ToHashSet(XYZEqualityComparer.Instance);

            foreach (var id in result)
            {
                // Поиск координаты точки вставки в finishHoleTask по ElementId
                if (finishHoleTaskLocations.TryGetValue(id, out XYZ finishLocation) && finishLocation != null)
                {
                    // Если не найдено совпадение в startHoleTask, увеличиваем счётчик несовпадений
                    if (!startHoleTaskLocations.Any(startLocation => startLocation.IsAlmostEqualTo(finishLocation)))
                    {
                        nonMatchCount++;
                    }
                }
            }

            return nonMatchCount;
        }

        // Класс для сравнения XYZ с учетом допуска
        class XYZEqualityComparer : IEqualityComparer<XYZ>
        {
            public static readonly XYZEqualityComparer Instance = new XYZEqualityComparer();
            private const double Tolerance = 0.01;

            public bool Equals(XYZ x, XYZ y)
            {
                return x.IsAlmostEqualTo(y, Tolerance);
            }

            public int GetHashCode(XYZ obj)
            {
                // Достаточно простой реализации для наших целей
                return obj.GetHashCode();
            }
        }
    }
}
