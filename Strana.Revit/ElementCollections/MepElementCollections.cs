// <copyright file="MepElementCollections.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Strana.Revit.HoleTask.Utils;

namespace Strana.Revit.HoleTask.ElementCollections
{
    /// <summary>
    /// Class contains all mep elements in revit model.
    /// </summary>
    public static class MepElementCollections
    {/// <summary>
     /// Gets all cable trays to check intersecting + fastFilterBB.
     /// </summary>
     /// <param name="doc"><seealso cref="Document"/></param>
     /// <param name="intersectedElement">wallFloor</param>
     /// <param name="transform"></param>
     /// <returns></returns>
        public static IEnumerable<Element> AllMepElementsByBBox(Document doc, Element intersectedElement, Transform transform)
        {
            BoundingBoxXYZ iEBB = intersectedElement.get_BoundingBox(null);
            BoundingBoxXYZ transformedBoundingBox = intersectedElement.get_BoundingBox(null);

            XYZ poinFirst = transform.OfPoint(iEBB.Min);
            XYZ poinLast = transform.OfPoint(iEBB.Max);

            double pointFirstX = poinFirst.X < poinLast.X ? poinFirst.X : poinLast.X;
            double pointFirstY = poinFirst.Y < poinLast.Y ? poinFirst.Y : poinLast.Y;
            double pointFirstZ = poinFirst.Z < poinLast.Z ? poinFirst.Z : poinLast.Z;
            double poinLastX = poinFirst.X > poinLast.X ? poinFirst.X : poinLast.X;
            double poinLastY = poinFirst.Y > poinLast.Y ? poinFirst.Y : poinLast.Y;
            double poinLastZ = poinFirst.Z > poinLast.Z ? poinFirst.Z : poinLast.Z;
            XYZ transformedMin = new XYZ(pointFirstX, pointFirstY, pointFirstZ);
            XYZ transformedMax = new XYZ(poinLastX, poinLastY, poinLastZ);

            transformedBoundingBox.Min = transformedMin;
            transformedBoundingBox.Max = transformedMax;
            List<XYZ> allBoundingBoxPoints =
            [
                transform.OfPoint(new XYZ(iEBB.Min.X, iEBB.Min.Y, iEBB.Max.Z)),
                transform.OfPoint(new XYZ(iEBB.Min.X, iEBB.Max.Y, iEBB.Max.Z)),
                transform.OfPoint(new XYZ(iEBB.Max.X, iEBB.Max.Y, iEBB.Min.Z)),
                transform.OfPoint(new XYZ(iEBB.Max.X, iEBB.Min.Y, iEBB.Min.Z)),
            ];

            transformedBoundingBox.ExpandToContain(allBoundingBoxPoints);

            Outline outline = new(transformedBoundingBox.Min, transformedBoundingBox.Max);

            BoundingBoxIntersectsFilter filter = new(outline);

            var d = new FilteredElementCollector(doc)
                          .OfCategory(BuiltInCategory.OST_DuctCurves)
                          .OfClass(typeof(Duct))
                          .WhereElementIsNotElementType()
                          .WherePasses(filter);

            var p = new FilteredElementCollector(doc)
                         .OfCategory(BuiltInCategory.OST_PipeCurves)
                         .OfClass(typeof(Pipe))
                         .WhereElementIsNotElementType()
                         .WherePasses(filter);

            var c = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_CableTray)
                        .OfClass(typeof(CableTray))
                        .WhereElementIsNotElementType()
                        .WherePasses(filter);

            return d.Concat(p).Concat(c);
        }

        public static IEnumerable<Element> AllElementsByMepBBox(this Element mepElement, Document doc, Transform transform, Document linkDoc)
        {
            BoundingBoxXYZ iEBB = mepElement.get_BoundingBox(null);
            BoundingBoxXYZ transformedBoundingBox = mepElement.get_BoundingBox(null);

            XYZ poinFirst = transform.OfPoint(iEBB.Min);
            XYZ poinLast = transform.OfPoint(iEBB.Max);

            double pointFirstX = poinFirst.X < poinLast.X ? poinFirst.X : poinLast.X;
            double pointFirstY = poinFirst.Y < poinLast.Y ? poinFirst.Y : poinLast.Y;
            double pointFirstZ = poinFirst.Z < poinLast.Z ? poinFirst.Z : poinLast.Z;
            double poinLastX = poinFirst.X > poinLast.X ? poinFirst.X : poinLast.X;
            double poinLastY = poinFirst.Y > poinLast.Y ? poinFirst.Y : poinLast.Y;
            double poinLastZ = poinFirst.Z > poinLast.Z ? poinFirst.Z : poinLast.Z;
            XYZ transformedMin = new XYZ(pointFirstX, pointFirstY, pointFirstZ);
            XYZ transformedMax = new XYZ(poinLastX, poinLastY, poinLastZ);

            transformedBoundingBox.Min = transformedMin;
            transformedBoundingBox.Max = transformedMax;
            List<XYZ> allBoundingBoxPoints =
            [
                transform.OfPoint(new XYZ(iEBB.Min.X, iEBB.Min.Y, iEBB.Max.Z)),
                transform.OfPoint(new XYZ(iEBB.Min.X, iEBB.Max.Y, iEBB.Max.Z)),
                transform.OfPoint(new XYZ(iEBB.Max.X, iEBB.Max.Y, iEBB.Min.Z)),
                transform.OfPoint(new XYZ(iEBB.Max.X, iEBB.Min.Y, iEBB.Min.Z)),
            ];

            transformedBoundingBox.ExpandToContain(allBoundingBoxPoints);


            Outline outline = new(transformedBoundingBox.Min, transformedBoundingBox.Max);

            SphereByPoint.CreateSphereByPoint(transformedBoundingBox.Min, doc);
            SphereByPoint.CreateSphereByPoint(transformedBoundingBox.Max, doc);

            BoundingBoxIntersectsFilter filter = new(outline);
            //var c = new FilteredElementCollector(linkDoc)
            //           .OfCategory(BuiltInCategory.OST_Walls)
            //           .OfClass(typeof(Wall))
            //           .WhereElementIsNotElementType()
            //           .WherePasses(filter);
            // Создаем правило фильтра, исключающее витражные стены
            ParameterValueProvider provider = new ParameterValueProvider(wallTypeId);
            FilterNumericRuleEvaluator evaluator = new FilterNumericEquals();
            FilterRule rule = new FilterElementIdRule(provider, evaluator, WallKind.Curtain);
            ElementParameterFilter curtainWallFilter = new ElementParameterFilter(rule, true); // true для инверсии правила

            // Фильтр для получения стен, исключая витражные
            var c = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .WherePasses(filter) // Пользовательский фильтр
                .WherePasses(curtainWallFilter); // Исключение витражных стен

            var c2 = new FilteredElementCollector(linkDoc)
            .OfCategory(BuiltInCategory.OST_Floors)
            .OfClass(typeof(Floor))
            .WhereElementIsNotElementType()
            .WherePasses(filter);

            //return c.Concat(c2);
            return new FilteredElementCollector(linkDoc)
                        .OfCategory(BuiltInCategory.OST_Walls)
                        .OfClass(typeof(Wall))
                        .WhereElementIsNotElementType();
        }

        /// <summary>
        ///     Expand the given bounding box to include
        ///     and contain the given point.
        /// </summary>
        public static void ExpandToContain(this BoundingBoxXYZ bb, XYZ p)
        {
            bb.Min = new XYZ(Math.Min(bb.Min.X, p.X),
                Math.Min(bb.Min.Y, p.Y),
                Math.Min(bb.Min.Z, p.Z));

            bb.Max = new XYZ(Math.Max(bb.Max.X, p.X),
                Math.Max(bb.Max.Y, p.Y),
                Math.Max(bb.Max.Z, p.Z));
        }

        /// <summary>
        ///     Expand the given bounding box to include
        ///     and contain the given points.
        /// </summary>
        public static void ExpandToContain(this BoundingBoxXYZ bb, IEnumerable<XYZ> pts)
        {
            bb.ExpandToContain(new XYZ(
                pts.Min<XYZ, double>(p => p.X),
                pts.Min<XYZ, double>(p => p.Y),
                pts.Min<XYZ, double>(p => p.Z)));

            bb.ExpandToContain(new XYZ(
                pts.Max<XYZ, double>(p => p.X),
                pts.Max<XYZ, double>(p => p.Y),
                pts.Max<XYZ, double>(p => p.Z)));
        }
    }
}
