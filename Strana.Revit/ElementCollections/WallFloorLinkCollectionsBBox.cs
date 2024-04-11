﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Strana.Revit.HoleTask.Extension.RevitElement;

namespace Strana.Revit.HoleTask.ElementCollections
{
    public static class WallFloorLinkCollectionsBBox
    {
        public static (Dictionary<Solid, Element> SolidElementMap, Dictionary<BoundingBoxXYZ, Element> BBoxElementMap) AllElementsByMepBBox(Element mepElement, RevitLinkInstance linkInstance)
        {
            Document linkDoc = linkInstance.GetLinkDocument();
            Transform transform = linkInstance.GetTransform();

            BoundingBoxXYZ mepBoundingBox = mepElement.get_BoundingBox(null);
            if (mepBoundingBox == null)
            {
                return (new Dictionary<Solid, Element>(), new Dictionary<BoundingBoxXYZ, Element>());
            }

            IEnumerable<Element> elements = new FilteredElementCollector(linkDoc)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementMulticlassFilter(new List<Type> { typeof(Wall), typeof(Floor) }))
                .ToList();

            IEnumerable<Element> walls = elements.OfType<Wall>().Where(w => w.WallType.Kind != WallKind.Curtain);
            IEnumerable<Element> floors = elements.OfType<Floor>();
            IEnumerable<Element> combinedElements = walls.Concat(floors);

            var solidElementMap = GetTransformedSolidsFromElements(combinedElements, transform);
            var bboxElementMap = TransformSolidsToBoundingBoxes(solidElementMap);

            return (solidElementMap, bboxElementMap);
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
                kvp =>
                {
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
    }
}
