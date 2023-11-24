// <copyright file="HoleTaskCreator.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

namespace Strana.Revit.HoleTask.Utils
{

    /// <summary>
    /// This class contains metod set up a HoleTasks familySybol.
    /// </summary>
    internal class HoleTaskCreator
    {
        private readonly Document doc;
        private readonly List<FamilyInstance> intersectionFloorRectangularCombineList = new List<FamilyInstance>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HoleTaskCreator"/> class.
        /// Create families into a Revit document.
        /// </summary>
        /// <param name="doc"><seealso cref="Document"/></param>
        public HoleTaskCreator(Document doc)
        {
            this.doc = doc;
        }

        /// <summary>
        /// Create DirectShape sphere For test.
        /// </summary>
        /// <param name="doc"><seealso cref="Document"/></param>
        /// <param name="center"><seealso cref="XYZ"/></param>
        public static void CreateSphereByPoint(Document doc, XYZ center/*, double diameter*/)
        {
            List<Curve> profile = [];

            // first create sphere with 2' radius
            //diameter = 0.5;
            //double radius = diameter/2;
            double radius = 0.2;
            XYZ profilePlus = center + new XYZ(0, radius, 0);
            XYZ profileMinus = center - new XYZ(0, radius, 0);

            profile.Add(Line.CreateBound(profilePlus, profileMinus));
            profile.Add(Arc.Create(profileMinus, profilePlus, center + new XYZ(radius, 0, 0)));

            CurveLoop curveLoop = CurveLoop.Create(profile);
            SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

            Frame frame = new Frame(center, XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);
            if (Frame.CanDefineRevitGeometry(frame))
            {
                Solid sphere = GeometryCreationUtilities.CreateRevolvedGeometry(frame, new CurveLoop[] { curveLoop }, 0, 2 * Math.PI, options);
                //using (Transaction t = new(doc, "create SphereByPoint"))
                //{
                //t.Start();
                DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Furniture));
                ds.ApplicationId = "Application id";
                ds.ApplicationDataId = "Geometry object id";
                ds.SetShape(new GeometryObject[] { sphere });
                //t.Commit();
                //}
            }
        }

        /// <summary>
        /// Set up a HoleTasks familySybol into a middle intersection meps element.
        /// </summary>
        /// <param name="mepElement">The MEP element representing pipes or ducts to create a hole for.</param>
        /// <param name="intersection">The intersection geometry with the MEP element.</param>
        /// <param name="intersectedElement">The floor element where the hole is created.</param>
        /// <param name="linkDoc">The linked document where the floor element resides.</param>
        /// <param name="linkInstance">The link instance in current project.</param>
        /// <param name="clearance">Clearance value for the top and bottom of the MEP element.</param>
        /// <param name="roundHoleSizesUpIncrement">The increment used for rounding up hole sizes.</param>
        /// <param name="floorHoleFamilySymbol">The family symbol for creating the hole in the floor.</param>
        /// <returns>
        /// The created FamilyInstance representing the hole task.
        /// </returns>
        public FamilyInstance PlaceHoleTaskFamilyInstance(
            Element mepElement,
            SolidCurveIntersection intersection,
            Element intersectedElement,
            Document linkDoc,
            RevitLinkInstance linkInstance,
            double clearance,
            double roundHoleSizesUpIncrement)
        {
            OrientaionType orientation = this.GetElementOrientationType(mepElement);
            FamilySymbol holeFamilySymbol;
            HoleTaskFamilyLoader familyLoader = new(this.doc);
            Wall wall = intersectedElement as Wall;
            if (intersectedElement != wall && orientation == OrientaionType.Vertical)
            {
                holeFamilySymbol = familyLoader.FloorFamilySymbol;
            }
            else if (intersectedElement != wall && orientation == OrientaionType.Horizontal)// Horizontal == Inclined
            {
                holeFamilySymbol = familyLoader.FloorFamilySymbol;
            }
            else
            {
                holeFamilySymbol = familyLoader.WallFamilySymbol;
            }

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

            FamilyInstance holeTask;
            IEnumerable<Level> docLvlList = this.GetDocumentLevels(this.doc);

            double holeTaskWidth = this.RoundUpToIncrement(mepHeight + clearance, roundHoleSizesUpIncrement);
            double holeTaskThickness = this.RoundUpToIncrement(this.CalculatedWidth(mepWidth, intersectedElement, intersection) + clearance, roundHoleSizesUpIncrement);
            double holeTaskHeight = this.GetInterctedElementThickness(intersectedElement) + (60 / 304.8);

            Level lvl = GetClosestFloorLevel(docLvlList, linkDoc, intersectedElement);
            XYZ intersectionCurveCenter = this.GetIntersectionCurveCenter(intersection);
            intersectionCurveCenter = new XYZ(intersectionCurveCenter.X, intersectionCurveCenter.Y, intersectionCurveCenter.Z - lvl.ProjectElevation);

            holeTask = this.doc.Create.NewFamilyInstance(
                intersectionCurveCenter,
                holeFamilySymbol,
                lvl,
                StructuralType.NonStructural);
            this.intersectionFloorRectangularCombineList.Add(holeTask);

            holeTask.LookupParameter("Глубина").Set(holeTaskThickness);
            holeTask.LookupParameter("Ширина").Set(this.ExchangeParameters(orientation, holeTaskWidth, holeTaskHeight));// Width
            holeTask.LookupParameter("Высота").Set(this.ExchangeParameters(orientation, holeTaskHeight, holeTaskWidth));// Height

            this.RotateHoleTask(mepElement, orientation, holeTask, intersection, intersectedElement, lvl, linkInstance);
            return holeTask;
        }

        /// <summary>
        /// Gets the closest floor level from the list of document levels based on the elevation of the linked floor.
        /// </summary>
        /// <param name="docLvlList">The list of document levels</param>
        /// <param name="linkDoc">The linked document.</param>
        /// <param name="floor">The linked floor element.</param>
        /// <returns>The closest floor level.</returns>
        private static Level GetClosestFloorLevel(IEnumerable<Level> docLvlList, Document linkDoc, Element floor)
        {
            Level lvl = null;
            double linkFloorLevelElevation = (linkDoc.GetElement(floor.LevelId) as Level).Elevation;
            double heightDifference = 10000000000;
            foreach (Level docLvl in docLvlList)
            {
                double tmpHeightDifference = Math.Abs(Math.Round(linkFloorLevelElevation, 6) - Math.Round(docLvl.Elevation, 6));
                if (tmpHeightDifference < heightDifference)
                {
                    heightDifference = tmpHeightDifference;
                    lvl = docLvl;
                }
            }

            return lvl;
        }

        /// <summary>
        /// RoundUp Height and Width familys parameter in project.
        /// </summary>
        /// <param name="heightWidth">Element size in model.</param>
        /// <param name="roundingValue">Величина округления.</param>
        /// <returns>
        /// The rounded-up value of the height or width, ensuring it is a multiple of the specified increment.
        /// </returns>
        private double RoundUpToIncrement(double heightWidth, double roundingValue)
        {
            return (((int)Math.Ceiling(heightWidth * 304.8 / roundingValue)) * roundingValue) / 304.8;
        }

        /// <summary>
        /// Get a collection of levels from the carrent Revit document.
        /// </summary>
        /// <param name="document">The Revit document to retrieve levels from.</param>
        /// <returns>
        /// List collection of Level objects representing the levels in the document.
        /// </returns>
        private IEnumerable<Level> GetDocumentLevels(Document document)
        {
            return new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .ToList();
        }

        /// <summary>
        /// MEP element orienteering type.
        /// </summary>
        /// <param name="wallОrFloor">Intersected host element.</param>
        /// <returns> Vertical/Horizontal.</returns>
        private OrientaionType GetElementOrientationType(Element mepElement)
        {
            LocationCurve mepElementCurve = mepElement.Location as LocationCurve;
            XYZ startPoint = mepElementCurve.Curve.GetEndPoint(0);
            XYZ endPoint = mepElementCurve.Curve.GetEndPoint(1);
            double tolerance = 0.3; // Tolerance=0.1 == 1 degree, tolerance=0.2 == 3 degree, tolerance=0.3 == 4 degree.
            if (Math.Abs(startPoint.X - endPoint.X) <= tolerance)
            {
                return OrientaionType.Vertical;
            }

            return OrientaionType.Horizontal;
        }

        private XYZ GetIntersectionCurveCenter(SolidCurveIntersection intersection)
        {
            XYZ intersectionCurveStartPoint = intersection.GetCurveSegment(0).GetEndPoint(0);
            XYZ intersectionCurveEndPoint = intersection.GetCurveSegment(0).GetEndPoint(1);
            XYZ intersectionCurveCenter = (intersectionCurveStartPoint + intersectionCurveEndPoint) / 2;
            return intersectionCurveCenter;
        }

        private double GetInterctedElementThickness(Element intersectedElement)
        {
            if (intersectedElement.GetType() == typeof(Wall))
            {
                Wall wall = intersectedElement as Wall;
                double wallThickness = wall.Width;
                return wallThickness;
            }
            else
            {
                double floorThickness = intersectedElement.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();
                return floorThickness;
            }
        }

        private double ExchangeParameters(OrientaionType orientaionType, double holeTaskHeight, double holeTaskWidth)
        {
            if (orientaionType == OrientaionType.Vertical)
            {
                return holeTaskHeight;
            }
            else
            {
                holeTaskHeight = holeTaskWidth;
                return holeTaskHeight;
            }
            //мб тут надо будет добавить определения для еще одного кейса, как и для всех ориентаций
            //intersectedElement != wall && orientation == OrientaionType.Horizontal
        }

        private double CalculatedWidth(double mepWidth, Element intersectedElement, SolidCurveIntersection intersection)
        {
            if (intersectedElement.GetType() == typeof(Floor))
            {
                return mepWidth;
            }
            else
            {
                XYZ intersectionCurveStartPoint = intersection.GetCurveSegment(0).GetEndPoint(0);
                XYZ intersectionCurveEndPoint = intersection.GetCurveSegment(0).GetEndPoint(1);

                double legTriangle = this.GetInterctedElementThickness(intersectedElement);
                double hypotenuseTriangle = intersectionCurveStartPoint.DistanceTo(intersectionCurveEndPoint);

                if (legTriangle == hypotenuseTriangle)
                {
                    return mepWidth;
                }
                else
                {
                    double calculatedWidth = Math.Sqrt(Math.Abs((hypotenuseTriangle * hypotenuseTriangle) - (legTriangle * legTriangle)));
                    mepWidth = mepWidth + (2 * calculatedWidth);
                    return mepWidth;
                }
            }
        }

        private double GetAngleFromMEPCurve(MEPCurve curve)
        {
            foreach (Connector c in curve.ConnectorManager.Connectors)
            {
                double rotationAngle;
                if (c.CoordinateSystem.BasisY.AngleOnPlaneTo(XYZ.BasisY, XYZ.BasisZ) < Math.PI)
                {
                    rotationAngle = c.CoordinateSystem.BasisY.AngleTo(XYZ.BasisY) + Math.PI / 2;
                }
                else
                {
                    rotationAngle = -c.CoordinateSystem.BasisY.AngleTo(XYZ.BasisY) + Math.PI / 2;
                }
                return rotationAngle;
            }
            return 0;
        }

        private void RotateHoleTask(Element mepElement, OrientaionType orientaionType, FamilyInstance holeTask, SolidCurveIntersection intersection, Element intersectedElement, Level lvl, RevitLinkInstance linkInstance)
        {
            {
                Wall wall = intersectedElement as Wall;
                MEPCurve curve = mepElement as MEPCurve;
                Transform transform = linkInstance.GetTotalTransform();
                XYZ xAxis = transform.BasisX;
                double linkRotation = Math.Atan2(xAxis.Y, xAxis.X);
                if (intersectedElement != wall && orientaionType == OrientaionType.Vertical)
                {
                    double rotationAngle = GetAngleFromMEPCurve(curve);
                    XYZ point1 = intersection.GetCurveSegment(0).GetEndPoint(0);
                    XYZ point2 = intersection.GetCurveSegment(0).GetEndPoint(1);
                    Line axis = Line.CreateBound(point1, point2);
                    ElementTransformUtils.RotateElement(this.doc, holeTask.Id, axis, rotationAngle);
                }
                else if (intersectedElement != wall && orientaionType == OrientaionType.Horizontal) // Horizontal == Inclined
                {
                    double rotationAngle = GetAngleFromMEPCurve(curve);
                    XYZ pointCenter = GetIntersectionCurveCenter(intersection);
                    Line axis = Line.CreateBound(pointCenter, pointCenter + XYZ.BasisZ);
                    ElementTransformUtils.RotateElement(this.doc, holeTask.Id, axis, rotationAngle + linkRotation);
                }
                else // Wall
                {
                    XYZ wallOrientation = wall.Orientation;
                    XYZ pointCenter = GetIntersectionCurveCenter(intersection);
                    pointCenter = new XYZ(pointCenter.X, pointCenter.Y, pointCenter.Z - lvl.Elevation);
                    Line axis = Line.CreateBound(pointCenter, pointCenter + XYZ.BasisZ);
                    double rotationAngle = wallOrientation.AngleTo(holeTask.FacingOrientation);
                    if (wallOrientation.AngleOnPlaneTo(holeTask.FacingOrientation, XYZ.BasisZ) < Math.PI)
                    {
                        rotationAngle *= -1;
                    }
                    if (rotationAngle != 0)
                    {
                        ElementTransformUtils.RotateElement(this.doc, holeTask.Id, axis, (rotationAngle + linkRotation) - (Math.PI / 2));
                    }
                }
            }
        }
    }
}
