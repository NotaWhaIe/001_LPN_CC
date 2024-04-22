﻿using Autodesk.Revit.DB;
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

        internal static HoleTaskGridDelta DeltaHoleTaskToGrids(Document doc, XYZ intersectionCenter, double thickness, double width, double angle)
        {
            if (!areRoundHoleTaskInPlane)
            {
                return new HoleTaskGridDelta(0, 0, 0);
            }
            double roundHoleTaskInPlane = WpfSettings.RoundHoleTaskInPlane;

            XYZ leftBottomIntersection = FindLeftBottomIntersection(doc);

            double toGrid1 = UnitUtils.ConvertFromInternalUnits(intersectionCenter.X - leftBottomIntersection.X, UnitTypeId.Millimeters);
            double toGridA = UnitUtils.ConvertFromInternalUnits(intersectionCenter.Y - leftBottomIntersection.Y, UnitTypeId.Millimeters);

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

        public static XYZ FindLeftBottomIntersection(Document doc)
        {
            var grids = new FilteredElementCollector(doc)
                        .OfClass(typeof(Grid))
                        .Cast<Grid>()
                        .ToList();

            List<XYZ> intersectionPoints = new List<XYZ>();

            for (int i = 0; i < grids.Count; i++)
            {
                var curve1 = grids[i].Curve;

                for (int j = i + 1; j < grids.Count; j++)
                {
                    var curve2 = grids[j].Curve;

                    IntersectionResultArray results;
                    SetComparisonResult result = curve1.Intersect(curve2, out results);

                    if (result == SetComparisonResult.Overlap && results != null)
                    {
                        foreach (IntersectionResult ir in results)
                        {
                            intersectionPoints.Add(ir.XYZPoint);
                        }
                    }
                }
            }

            // Находим точку с минимальными X и Y среди всех точек пересечения
            if (intersectionPoints.Count == 0)
            {
                return null; // Нет пересечений
            }

            return intersectionPoints.OrderBy(p => p.X).ThenBy(p => p.Y).FirstOrDefault();
        }

    }
}

