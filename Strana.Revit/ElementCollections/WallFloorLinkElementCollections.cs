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
        /// <param name="mepElement"> This is a duct, cable tray or pipe.</param>
        /// <param name="doc"><seealso cref="Document"/></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IEnumerable<Element> AllElementsByMepBBox(this Element mepElement, RevitLinkInstance linkInstance)
        {
            Document doc = linkInstance.Document;
            Document linkDoc = linkInstance.GetLinkDocument();
            Transform transform = linkInstance.GetTransform();

            BoundingBoxXYZ mepBoundingBox = mepElement.get_BoundingBox(null);
            if (mepBoundingBox == null)
            {
                return Enumerable.Empty<Element>();
            }
            XYZ transformedMin = transform.Inverse.OfPoint(mepBoundingBox.Min);
            XYZ transformedMax = transform.Inverse.OfPoint(mepBoundingBox.Max);

            Outline mepOutline = new Outline(transformedMin, transformedMax);
            BoundingBoxIntersectsFilter mepFilter = new BoundingBoxIntersectsFilter(mepOutline);

            IEnumerable<Element> walls = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .WherePasses(mepFilter)
                .Cast<Wall>()
                .Where(w => w.WallType.Kind != WallKind.Curtain)
                .Cast<Element>();

            IEnumerable<Element> floors = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_Floors)
                .OfClass(typeof(Floor))
                .WhereElementIsNotElementType()
                .WherePasses(mepFilter)
                .Cast<Element>();

            return walls.Concat(floors);
        }
    }
}
