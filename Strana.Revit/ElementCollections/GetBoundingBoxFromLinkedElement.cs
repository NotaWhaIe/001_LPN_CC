using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Strana.Revit.HoleTask.Extension.RevitElement;

namespace Strana.Revit.HoleTask.ElementCollections
{
    public static class GetBoundingBoxFromLinkedElement
    {
        public static Dictionary<BoundingBoxXYZ, Element> AllCarrentLinksBBox(RevitLinkInstance linkInstance)
        {
            Document linkDoc = linkInstance.GetLinkDocument();
            Transform transform = linkInstance.GetTransform();


            IEnumerable<Element> elements = new FilteredElementCollector(linkDoc)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementMulticlassFilter(new List<Type> { typeof(Wall), typeof(Floor) }))
                .ToList();

            IEnumerable<Element> walls = elements.OfType<Wall>().Where(w => w.WallType.Kind != WallKind.Curtain);
            IEnumerable<Element> floors = elements.OfType<Floor>();
            IEnumerable<Element> combinedElements = walls.Concat(floors);

            Dictionary<Solid, Element> solidElementMap = GetTransformedSolidsFromElements(combinedElements, transform);
            Dictionary<BoundingBoxXYZ, Element> bboxElementMap = TransformSolidsToBoundingBoxes(solidElementMap);

            return  bboxElementMap;
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
