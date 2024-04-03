// <copyright file="MepElementCollections.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Strana.Revit.HoleTask.Utils;

namespace Strana.Revit.HoleTask.ElementCollections
{
    /// <summary> Class contains all mep elements in revit model. </summary>
    public static class WallFloorLinkElementCollections
    {
        /// <summary> Gets all cable trays to check intersecting + fastFilterBB. </summary>
         /// <param name="doc"><seealso cref="Document"/></param>
         /// <param name="transform"></param>
         /// <returns></returns>
        public static IEnumerable<Element> AllElementsByMepBBox(this Element mepElement, Document doc, Transform transform, Document linkDoc)
        {
            BoundingBoxXYZ mepBoundingBox = mepElement.get_BoundingBox(null);
            if (mepBoundingBox == null)
            {
                return Enumerable.Empty<Element>();
            }
            XYZ transformedMin = transform.Inverse.OfPoint(mepBoundingBox.Min);
            XYZ transformedMax = transform.Inverse.OfPoint(mepBoundingBox.Max);

            XYZ transformedMintest = transform.Inverse.OfPoint(mepBoundingBox.Min);
            XYZ transformedMaxtest = transform.Inverse.OfPoint(mepBoundingBox.Max);
            SphereByPoint.CreateSphereByPoint(transformedMintest, doc);
            SphereByPoint.CreateSphereByPoint(transformedMaxtest, doc);

            Outline mepOutline = new Outline(transformedMin, transformedMax);
            BoundingBoxIntersectsFilter mepFilter = new BoundingBoxIntersectsFilter(mepOutline);

            var c1 = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                //.WherePasses(mepFilterInvert)
                .WherePasses(mepFilter)
                .Cast<Wall>()
                .Where(w => w.WallType.Kind != WallKind.Curtain)
                .Cast<Element>();

            var c2 = new FilteredElementCollector(linkDoc)
                  .OfCategory(BuiltInCategory.OST_Floors)
                  .OfClass(typeof(Floor))
                  .WhereElementIsNotElementType()
                  //.WherePasses(mepFilterInvert)
                  .WherePasses(mepFilter)
                  .Cast<Element>();

            int debag = c1.Count();
            if (debag <= 0)
            {
                c1 = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .Cast<Wall>()
                .Where(w => w.WallType.Kind != WallKind.Curtain)
                .Cast<Element>();
            }

            int debagа = c2.Count();
            if (debagа <= 0)
            {
                c2 = new FilteredElementCollector(linkDoc)
                  .OfCategory(BuiltInCategory.OST_Floors)
                  .OfClass(typeof(Floor))
                  .WhereElementIsNotElementType()
                  .Cast<Element>();
            }
            // Получаем первый элемент или null, если коллекция пуста
            Element firstFloor = c2.FirstOrDefault();

            if (firstFloor != null)
            {
                // Получаем BoundingBox первого элемента, если он не null
                BoundingBoxXYZ boundingBox = firstFloor.get_BoundingBox(null);

                // Только если boundingBox не null, создаем сферы
                if (boundingBox != null)
                {
                    SphereByPoint.CreateSphereByPoint(boundingBox.Max, doc);
                    SphereByPoint.CreateSphereByPoint(boundingBox.Min, doc);
                }
            }

            int debag0 = c2.Count();
            return c1.Concat(c2);
        }
    }
}

