// <copyright file="CreateHoleTasks.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Strana.Revit.HoleTask.ElementCollections;
using Strana.Revit.HoleTask.Extension.RevitElement;
using Strana.Revit.HoleTask.Utils;

namespace Strana.Revit.HoleTask.RevitCommands
{
    /// <summary>
    /// Start Up HoleTask Plugin.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class CreateHoleTasks : IExternalCommand
    {
        /// <summary>
        /// Executed when buttoon clicked.
        /// </summary>
        /// <param name="commandData"><seealso cref="ExternalCommandData"/></param>
        /// <param name="message">revit message.</param>
        /// <param name="elements">revit elements set.</param>
        /// <returns>voiding.</returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            foreach (RevitLinkInstance linkInstance in LinkInstanseCollections.RevitLinks(doc))
            {
                Document linkDoc = linkInstance.GetLinkDocument();
                IEnumerable<Element> floors = CollectionsOfIntersectingElements.AllFloors(linkDoc);
                IEnumerable<Element> mepElements = MepElementCollections.AllMepElements(doc);
                foreach (Element floor in floors)
                {
                    Solid floorSolid = floor.GetSolidWithoutHoles(linkInstance);
                    foreach (Element mepElement in mepElements)
                    {
                        Curve mepCurve = (mepElement.Location as LocationCurve).Curve;
                        SolidCurveIntersectionOptions defOptions = new ();
                        SolidCurveIntersection solidCurve = floorSolid.IntersectWithCurve(mepCurve, defOptions);

                        if (solidCurve != null && solidCurve.SegmentCount > 0)
                        {
                            Curve intersectionCurve = solidCurve.GetCurveSegment(0);
                            double mepDiameter = mepElement.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM)?.AsDouble() ?? 0.0;
                            double mepHeight;
                            double mepWidth;
                            if (mepDiameter != 0.0)
                            {
                                mepHeight = mepDiameter;
                                mepWidth = mepDiameter;
                            }
                            else
                            {
                                mepHeight = mepElement.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM)?.AsDouble() ?? 0.0;
                                mepWidth = mepElement.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM)?.AsDouble() ?? 0.0;
                            }

                            SolidCreater.CreateSphereByPoint(doc, intersectionCurve.GetEndPoint(0));
                        }
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
