// <copyright file="IntersectingElementExtension.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Strana.Revit.HoleTask.ElementCollections;
using Strana.Revit.HoleTask.Extension.RevitElement;
using Strana.Revit.HoleTask.Utils;

namespace Strana.Revit.HoleTask.Extensions
{
    public static class IntersectingElementExtension
    {
        public static List<FamilyInstance> CreateHoleTasksByIntersectedElements(RevitLinkInstance linkInstance)
        {
            List<FamilyInstance> intersectedItemHoleTasks = new List<FamilyInstance>();
            Document doc = linkInstance.Document;
            Document linkDoc = linkInstance.GetLinkDocument();

            IEnumerable<Element> mepElements = MepElementSelector.GetSelectedOrAllMepElements();
            var carrentLinkBBoxes = GetBoundingBoxFromLinkedElement.AllCarrentLinksBBox(linkInstance);

            foreach (Element mepElement in mepElements)
            {
                var wallAndFloorsInMepBBox = WallFloorLinkElementCollections.AllElementsByMepBBox(mepElement, carrentLinkBBoxes);
                foreach (Element intersectingElement in wallAndFloorsInMepBBox)
                {
                    if (intersectingElement.GetSolidWithoutHoles(linkInstance) is { } floorWallSolid &&
                        mepElement.Location as LocationCurve is { Curve: { } mepCurve })
                    {
                        SolidCurveIntersectionOptions defOptions = new SolidCurveIntersectionOptions();
                        SolidCurveIntersection solidCurve = floorWallSolid.IntersectWithCurve(mepCurve, defOptions);

                        if (solidCurve?.SegmentCount > 0)
                        {
                            HoleTaskCreator holeTaskCreator = new HoleTaskCreator(doc);
                            FamilyInstance createdHoleTask = holeTaskCreator.PlaceHoleTaskFamilyInstance(mepElement, solidCurve, intersectingElement, linkDoc, linkInstance);
                            if (createdHoleTask != null)
                            {
                                intersectedItemHoleTasks.Add(createdHoleTask);
                            }
                        }
                    }
                }
            }
            return intersectedItemHoleTasks;
        }
    }
}
