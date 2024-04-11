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
        private static Dictionary<Document, Dictionary<BoundingBoxXYZ, Element>> cachedBBoxMap = new Dictionary<Document, Dictionary<BoundingBoxXYZ, Element>>();

        public static IEnumerable<Element> AllElementsByMepBBox(this Element mepElement, RevitLinkInstance linkInstance)
        {
            Document linkDoc = linkInstance.GetLinkDocument();
            Transform linkTransform = linkInstance.GetTransform();

            BoundingBoxXYZ mepBoundingBox = mepElement.get_BoundingBox(null);
            if (mepBoundingBox == null)
            {
                return Enumerable.Empty<Element>();
            }

            // Проверка наличия кэшированной карты для документа
            if (!cachedBBoxMap.ContainsKey(linkDoc) || cachedBBoxMap[linkDoc].Count == 0)
            {
                CacheElements(linkDoc, linkTransform);
            }

            var bboxElementMap = cachedBBoxMap[linkDoc];
            return FindIntersectingElements(mepBoundingBox, bboxElementMap, 0);
        }

        private static void CacheElements(Document linkDoc, Transform transform)
        {
            IEnumerable<Element> walls = new FilteredElementCollector(linkDoc)
                         .OfClass(typeof(Wall))
                         .WhereElementIsNotElementType()
                         .Cast<Wall>()
                         .Where(w => w.WallType.Kind != WallKind.Curtain)
                         .Cast<Element>();

            IEnumerable<Element> floors = new FilteredElementCollector(linkDoc)
                .OfClass(typeof(Floor))
                .WhereElementIsNotElementType()
                .Cast<Element>();

            IEnumerable<Element> elements = walls.Concat(floors);

            var solidElementMap = GetTransformedSolidsFromElements(elements, transform);
            var bboxElementMap = TransformSolidsToBoundingBoxes(solidElementMap);
            cachedBBoxMap[linkDoc] = bboxElementMap;
        }

        private static Dictionary<Solid, Element> GetTransformedSolidsFromElements(IEnumerable<Element> elements, Transform transform)
        {
            return elements.Select(element =>
            {
                Solid solidWithHoles = WallFloorSolidGetter.GetSolidWithHoles(element);
                return solidWithHoles != null ? new { Solid = SolidUtils.CreateTransformed(solidWithHoles, transform), Element = element } : null;
            })
            .Where(x => x != null)
            .ToDictionary(x => x.Solid, x => x.Element);
        }

        private static Dictionary<BoundingBoxXYZ, Element> TransformSolidsToBoundingBoxes(Dictionary<Solid, Element> solidElementMap)
        {
            return solidElementMap.ToDictionary(
                kvp => {
                    Solid solid = kvp.Key;
                    Element element = kvp.Value;
                    BoundingBoxXYZ bb = solid.GetBoundingBox();
                    XYZ transformedMin = bb.Transform.OfPoint(bb.Min);
                    XYZ transformedMax = bb.Transform.OfPoint(bb.Max);
                    return new BoundingBoxXYZ { Min = transformedMin, Max = transformedMax };
                },
                kvp => kvp.Value
            );
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

