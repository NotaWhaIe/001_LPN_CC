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
            var c0 = new FilteredElementCollector(linkDoc)
                   .OfCategory(BuiltInCategory.OST_Walls)
                   .OfClass(typeof(Wall))
                   .WhereElementIsNotElementType()
                   .Cast<Wall>();

            Wall firstWall = c0.FirstOrDefault();
            BoundingBoxXYZ boundingBox = firstWall?.get_BoundingBox(null);
            Outline outline = boundingBox != null ? new Outline(boundingBox.Min, boundingBox.Max) : null;
            BoundingBoxIntersectsFilter filter = new(outline);

            var c1 = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                 .WherePasses(filter)
                .Cast<Wall>()
                .Where(w => w.WallType.Kind != WallKind.Curtain);

            var c2 = new FilteredElementCollector(linkDoc)
            .OfCategory(BuiltInCategory.OST_Floors)
            .OfClass(typeof(Floor))
            .WhereElementIsNotElementType()
            .WherePasses(filter);

            return c1.Concat(c2);
        }
    }
}
