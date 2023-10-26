// <copyright file="IntersectingElementExtension.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;
using Strana.Revit.HoleTask.ElementCollections;
using Strana.Revit.HoleTask.Extension.RevitElement;
using Strana.Revit.HoleTask.Utils;
using System.Collections.Generic;

namespace Strana.Revit.HoleTask.Extensions
{
    public static class IntersectingElementExtension
    {
        public static void CreateHoleTasksByIntersectedElements(this Element intersectingElement, RevitLinkInstance linkInstance)
        {
            if (intersectingElement.AreElementsHaveFaces())
            {
                Document doc = linkInstance.Document;
                Document linkDoc = linkInstance.GetLinkDocument();

                IEnumerable<Element> mepElements = MepElementCollections.AllMepElementsByBBox(doc, intersectingElement, linkInstance.GetTotalTransform());

                Solid floorSolid = intersectingElement.GetSolidWithoutHoles(linkInstance);
                foreach (Element mepElement in mepElements)
                {
                    Curve mepCurve = (mepElement.Location as LocationCurve).Curve;
                    SolidCurveIntersectionOptions defOptions = new();
                    SolidCurveIntersection solidCurve = floorSolid.IntersectWithCurve(mepCurve, defOptions);

                    if (solidCurve != null && solidCurve.SegmentCount > 0)
                    {
                        HoleTaskCreator holeTaskCreator = new(doc);
                        holeTaskCreator.PlaceHoleTaskFamilyInstance(mepElement, solidCurve, intersectingElement, linkDoc,linkInstance, 200 / 304.8, 10);
                    }
                }
            }

        }

    }

}
