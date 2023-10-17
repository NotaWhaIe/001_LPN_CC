// <copyright file="CreateHoleTasks.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
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
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (var gt = new TransactionGroup(doc, "HoleTasks"))
            {
                gt.Start();

                foreach (RevitLinkInstance linkInstance in LinkInstanseCollections.RevitLinks(doc))
                {
                    Document linkDoc = linkInstance.GetLinkDocument();

                    // Взять стены и перекрытия
                    IEnumerable<Element> allIntersectingElements = CollectionsOfIntersectingElements.AllIntersectingElements(linkDoc);

                    IEnumerable<Element> mepElements = MepElementCollections.AllMepElements(doc);
                    foreach (Element intersectingElement in allIntersectingElements)
                    {
                        Solid floorSolid = intersectingElement.GetSolidWithoutHoles(linkInstance);
                        foreach (Element mepElement in mepElements)
                        {
                            Curve mepCurve = (mepElement.Location as LocationCurve).Curve;
                            SolidCurveIntersectionOptions defOptions = new ();
                            SolidCurveIntersection solidCurve = floorSolid.IntersectWithCurve(mepCurve, defOptions);

                            if (solidCurve != null && solidCurve.SegmentCount > 0)
                            {
                                Curve intersectionCurve = solidCurve.GetCurveSegment(0);
                                double mepDiameter = mepElement.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM)?.AsDouble() ?? 0;
                                double mepHeight;
                                double mepWidth;
                                if (mepDiameter > 0)
                                {
                                    mepHeight = mepDiameter;
                                    mepWidth = mepDiameter;
                                }
                                else
                                {
                                    mepHeight = mepElement.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM)?.AsDouble() ?? 0;
                                    mepWidth = mepElement.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM)?.AsDouble() ?? 0;
                                }

                                ///Разместить семейство в месте пересечения. Угол поворота объекта, вокруг которого строиться, не учитывается
                                HoleTaskCreator holeTaskCreator = new(doc);
                                holeTaskCreator.PlaceHoleTaskFamilyInstance(mepElement, solidCurve, intersectingElement, linkDoc, 200 / 304.8, 10);

                                /////test Разместить свефу в месте пересечения
                                //SolidCreater.CreateSphereByPoint(doc, intersectionCurve.GetEndPoint(0));
                            }
                        }
                    }
                }

                gt.Assimilate();
            }

            //stopwatch.Stop();
            //TimeSpan elapsedTime = stopwatch.Elapsed;
            //TaskDialog.Show("Время работы", elapsedTime.TotalSeconds.ToString() + " сек.");

            return Result.Succeeded;
        }
    }
}
