using Autodesk.Revit.DB;
using Strana.Revit.HoleTask.Extensions.RevitElement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strana.Revit.HoleTask.Utils
{
    internal class HoleTasksRoundUpDimension
    {
        private static bool areRoundHoleTaskDimensions => Confing.Default.areRoundHoleTaskDimensions;

        private static int roundHoleTaskDimensions => Confing.Default.roundHoleTaskDimensions;

        internal void RoundUpAllHoleTask(RevitLinkInstance linkInstance)
        {
            if (areRoundHoleTaskDimensions)
            {
                Document doc = linkInstance.Document;
                List<FamilyInstance> intersectionWallRectangularCombineList = new();
                double roundTo = roundHoleTaskDimensions / 304.8;

                ///Добавляю в список уже существующие задания на отверстия:
                HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Стены_Прямоугольное", intersectionWallRectangularCombineList);
                HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Перекрытия_Прямоугольное", intersectionWallRectangularCombineList);

                RoundUpFamilyInstanceParameters(intersectionWallRectangularCombineList, roundTo);
            }
        }
        public static void RoundUpFamilyInstanceParameters(List<FamilyInstance> familyInstances, double roundTo)
        {
            foreach (var fi in familyInstances)
            {
                RoundUpParameter(fi, "Ширина", roundTo);
                RoundUpParameter(fi, "Высота", roundTo);
                RoundUpParameter(fi, "Глубина", roundTo);
            }
        }

        public static void RoundUpParameter(FamilyInstance fi, string parameterName, double roundTo)
        {
            Parameter param = fi.LookupParameter(parameterName);
            if (param != null && param.StorageType == StorageType.Double)
            {
                double originalValue = param.AsDouble();
                double roundedValue = Math.Ceiling((originalValue / roundTo) * roundTo);
                param.Set(roundedValue);
            }
        }
    }
}
