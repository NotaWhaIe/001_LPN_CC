﻿// <copyright file="MepElementCollections.cs" company="Strana">
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
        public static IEnumerable<Element> AllElementsByMepBBox(this Element mepElement, RevitLinkInstance linkInstance)
        {
            Document doc = linkInstance.Document;
            Document linkDoc = linkInstance.GetLinkDocument();
            Transform linkTransform = linkInstance.GetTransform();

            BoundingBoxXYZ mepBoundingBox = mepElement.get_BoundingBox(null);
            if (mepBoundingBox == null)
            {
                return Enumerable.Empty<Element>();
            }

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

            var solidElementMap = GetTransformedSolidsFromElements(elements, linkTransform);
            var bboxElementMap = TransformSolidsToBoundingBoxes(solidElementMap);
            var intersectingElements = FindIntersectingElements(mepBoundingBox, bboxElementMap, 0);

            double debag = intersectingElements.Count();

            return intersectingElements;
        }
        public static Dictionary<Solid, Element> GetTransformedSolidsFromElements(IEnumerable<Element> elements, Transform transform)
        {
            var solidElementMap = new Dictionary<Solid, Element>();
            foreach (Element element in elements)
            {
                Solid solidWithHoles = WallFloorSolidGetter.GetSolidWithHoles(element);
                if (solidWithHoles != null)
                {
                    Solid transformedSolid = SolidUtils.CreateTransformed(solidWithHoles, transform);
                    solidElementMap[transformedSolid] = element;
                }
            }
            return solidElementMap;
        }
        public static Dictionary<BoundingBoxXYZ, Element> TransformSolidsToBoundingBoxes(Dictionary<Solid, Element> solidElementMap)
        {
            var bboxElementMap = new Dictionary<BoundingBoxXYZ, Element>();
            foreach (var kvp in solidElementMap)
            {
                Solid solid = kvp.Key;
                Element element = kvp.Value;
                BoundingBoxXYZ bb = solid.GetBoundingBox();
                if (bb != null)
                {
                    XYZ transformedMin = bb.Transform.OfPoint(bb.Min);
                    XYZ transformedMax = bb.Transform.OfPoint(bb.Max);
                    BoundingBoxXYZ transformedBoundingBox = new BoundingBoxXYZ();
                    transformedBoundingBox.Min = transformedMin;
                    transformedBoundingBox.Max = transformedMax;
                    bboxElementMap[transformedBoundingBox] = element;
                }
            }
            return bboxElementMap;
        }
        public static IEnumerable<Element> FindIntersectingElements(BoundingBoxXYZ box1, Dictionary<BoundingBoxXYZ, Element> bboxElementMap, double tolerance)
        {
            Outline outline1 = new Outline(box1.Min, box1.Max);
            foreach (var kvp in bboxElementMap)
            {
                BoundingBoxXYZ box2 = kvp.Key;
                Element element = kvp.Value;
                Outline outline2 = new Outline(box2.Min, box2.Max);
                if (outline1.Intersects(outline2, tolerance))
                {
                    yield return element; 
                }
            }
        }
  
    }
}

