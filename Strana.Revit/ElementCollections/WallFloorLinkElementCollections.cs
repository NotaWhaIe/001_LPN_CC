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
    public static class WallFloorLinkElementCollections
    {/// <summary>
     /// Gets all cable trays to check intersecting + fastFilterBB.
     /// </summary>
     /// <param name="doc"><seealso cref="Document"/></param>
     /// <param name="intersectedElement">wallFloor</param>
     /// <param name="transform"></param>
     /// <returns></returns>

        public static IEnumerable<Element> AllElementsByMepBBox(this Element mepElement, Document doc, Transform transform, Document linkDoc)
        {

            BoundingBoxXYZ mepBoundingBox = mepElement.get_BoundingBox(null);
            if (mepBoundingBox == null)
            {
                return Enumerable.Empty<Element>();
            }

            XYZ transformedMin = mepBoundingBox.Min ;
            XYZ transformedMax = mepBoundingBox.Max ;
            Outline mepOutline = new Outline(transformedMin, transformedMax);
            BoundingBoxIntersectsFilter mepFilter = new BoundingBoxIntersectsFilter(mepOutline);

            var c1 = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                 .WherePasses(mepFilter)
                .Cast<Wall>()
                .Where(w => w.WallType.Kind != WallKind.Curtain);

            var c2 = new FilteredElementCollector(linkDoc)
                  .OfCategory(BuiltInCategory.OST_Floors)
                  .OfClass(typeof(Floor))
                  .WhereElementIsNotElementType()
                  .WherePasses(mepFilter);
            int debag = c2.Count();
            return c1.Concat(c2);
        }
    }
}
