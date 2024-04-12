// <copyright file="MepElementCollections.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Strana.Revit.HoleTask.Utils;
using Autodesk.Revit.Attributes;
using Strana.Revit.HoleTask.Extension.RevitElement;
using System.Xml.Linq;

namespace Strana.Revit.HoleTask.ElementCollections
{
    public static class WallFloorLinkElementCollections
    {
        public static IEnumerable<Element> AllElementsByMepBBox(
                Element mepElement,
                RevitLinkInstance linkInstance,
                Dictionary<BoundingBoxXYZ, Element> bboxElementMap)
        {
            Document linkDoc = linkInstance.GetLinkDocument();
            Transform linkTransform = linkInstance.GetTransform();

            BoundingBoxXYZ mepBoundingBox = mepElement.get_BoundingBox(null);
            if (mepBoundingBox == null)
            {
                return Enumerable.Empty<Element>();
            }
            return FindIntersectingElements(mepBoundingBox,bboxElementMap, 0);
        }

        public static IEnumerable<Element> FindIntersectingElements(BoundingBoxXYZ box1, Dictionary<BoundingBoxXYZ, Element> bboxElementMap, double tolerance)
        {
            Outline outline1 = new Outline(box1.Min, box1.Max);
            return bboxElementMap
                .Where(kvp => new Outline(kvp.Key.Min, kvp.Key.Max).Intersects(outline1, tolerance))
                .Select(kvp => kvp.Value);
        }
    }
}

