using Autodesk.Revit.DB;
using Strana.Revit.HoleTask.Extensions.RevitElement;
using Strana.Revit.HoleTask.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strana.Revit.HoleTask.Utils
{
    internal class GridRoundUpDimension
    {
        private static bool areRoundHoleTaskInPlane => Confing.Default.areRoundHoleTaskInPlane;
        //private static int roundHoleTaskInPlane => Confing.Default.roundHoleTaskInPlane;

        internal static HoleTaskGridDelta DeltaHoleTaskToGrids(Document doc, XYZ intersectionCenter, double thickness, double width, double angle)
        {
            if (!areRoundHoleTaskInPlane)
            {
                return new HoleTaskGridDelta(0, 0, 0);
            }
            double roundHoleTaskInPlane =WpfSettings.RoundHoleTaskInPlane;

            double toGrid1 = MeasureDistanceToGrid(doc, intersectionCenter, "1");
            double toGridA = MeasureDistanceToGrid(doc, intersectionCenter, "А");

            double thickness0 = ExchangeParametersAngles(angle, thickness, width);
            double width0 = ExchangeParametersAngles(angle, width, thickness);

            double thickness00 = UnitUtils.ConvertFromInternalUnits(thickness, UnitTypeId.Millimeters);
            double width00 = UnitUtils.ConvertFromInternalUnits(width, UnitTypeId.Millimeters);

            ///насколько нужно сместить семейство
            double delta1 = Math.Ceiling((toGrid1 - width00 / 2) / roundHoleTaskInPlane) * roundHoleTaskInPlane - (toGrid1 - width00 / 2);
            double deltaA = Math.Ceiling((toGridA - thickness00 / 2) / roundHoleTaskInPlane) * roundHoleTaskInPlane - (toGridA - thickness00 / 2);

            ///насколько нужно увеличить семейство
            double max = 2 * (Math.Max(delta1, deltaA));

            return new HoleTaskGridDelta(delta1, deltaA, max);
        }
        private static double ExchangeParametersAngles(double angle, double depth, double width)
        {
            const double epsilon = 1e-6; // Малая погрешность для сравнения чисел с плавающей точкой

            // Функция для проверки равенства с учетом погрешности
            bool AreEqual(double a, double b)
            {
                return Math.Abs(a - b) < epsilon;
            }

            if (AreEqual(angle, -Math.PI) || AreEqual(angle, Math.PI) || AreEqual(angle, 0))
            {
                return depth;
            }
            else if (AreEqual(angle, -Math.PI / 2) || AreEqual(angle, Math.PI / 2) || AreEqual(angle, 2 * Math.PI / 3) || AreEqual(angle, -2 * Math.PI / 3))
            {
                depth = width;
                return depth;
            }
            else
            {
                return depth;
            }
        }

        public static double MeasureDistanceToGrid(Document doc, XYZ intersectionCurveCenter, string gridName)
        {

            // Найти ось с заданным именем
            Grid grid = new FilteredElementCollector(doc)
                            .OfClass(typeof(Grid))
                            .Cast<Grid>()
                            .FirstOrDefault(g => g.Name.Equals(gridName, StringComparison.OrdinalIgnoreCase));

            if (grid == null || !(grid.Curve is Line gridLine))
            {
                return 0;
            }

            // Проекция точки на линию сетки в плоскости XY
            XYZ pointInXYPlane = new XYZ(intersectionCurveCenter.X, intersectionCurveCenter.Y, gridLine.GetEndPoint(0).Z);
            //SphereByPoint.CreateSphereByPoint(pointInXYPlane);
            //SphereByPoint.CreateSphereByPoint(gridLine.GetEndPoint(0));
            //SphereByPoint.CreateSphereByPoint(gridLine.GetEndPoint(1));

            var distance = gridLine.Distance(pointInXYPlane);
            // Вычисление горизонтального расстояния и конвертация из футов в миллиметры
            return UnitUtils.ConvertFromInternalUnits(distance, UnitTypeId.Millimeters);
            //return pointInXYPlane.DistanceTo(closestPoint);
        }
    }
}

