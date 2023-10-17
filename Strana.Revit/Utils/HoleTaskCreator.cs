using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Strana.Revit.BaseElements;

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
        /// Set up a HoleTasks familySybol into a middle intersection meps element.
        /// </summary>
        /// <param name="mepElement">The MEP element representing pipes or ducts to create a hole for.</param>
        /// <param name="intersection">The intersection geometry with the MEP element.</param>
        /// <param name="intersectedElement">The floor element where the hole is created.</param>
        /// <param name="linkDoc">The linked document where the floor element resides.</param>
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
            double clearance,
            double roundHoleSizesUpIncrement)
        {
            OrientaionType orientation = this.GetElementOrientationType(intersectedElement);
            

            FamilySymbol holeFamilySymbol;
            HoleTaskFamilyLoader familyLoader = new(this.doc);
            if (orientation == OrientaionType.Vertical)
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

            FamilyInstance holeTask = null;

            IEnumerable<Level> docLvlList = this.GetDocumentLevels(this.doc);

            double holeTaskWidth = this.RoundUpToIncrement(mepHeight + clearance, roundHoleSizesUpIncrement);
            double holeTaskThickness = this.RoundUpToIncrement(this.CalculatedWidth(orientation, mepWidth, intersectedElement, intersection) + clearance, roundHoleSizesUpIncrement);
            double holeTaskHeight = this.GetInterctedElementThickness(intersectedElement) + (60 / 304.8);

            Level lvl = GetClosestFloorLevel(docLvlList, linkDoc, intersectedElement);
            XYZ intersectionCurveCenter = this.GetIntersectionCurveCenter(intersection);

            using (var t = new Transaction(this.doc, "Put the familySybol"))
            {
                t.Start();

                holeTask = this.doc.Create.NewFamilyInstance(
                    intersectionCurveCenter,
                    holeFamilySymbol,
                    lvl,
                    StructuralType.NonStructural);

                holeTask.LookupParameter("Глубина").Set(holeTaskThickness);
                ///upd01 Для тестирования скорости 
                holeTask.LookupParameter("Ширина").Set(this.ExchangeParameters(orientation, holeTaskWidth, holeTaskHeight));
                holeTask.LookupParameter("Высота").Set(this.ExchangeParameters(orientation, holeTaskHeight, holeTaskWidth));

                // UPD2
                double rotationOfMepElement = this.AngleToRotateHoleTask(mepElement, orientation, holeTask, intersection, intersectedElement, lvl);
                /*if (rotationOfMepElement != 0)
                {
                    XYZ point1 = intersection.GetCurveSegment(0).GetEndPoint(0);
                    XYZ point2 = intersection.GetCurveSegment(0).GetEndPoint(1);
                    Line axis = Line.CreateBound(point1, point2);
                    ElementTransformUtils.RotateElement(this.doc, holeTask.Id, axis, rotationOfMepElement);
                }*/

                //intersectionPoint.get_Parameter(this.heightOfBaseLevelGuid).Set((this.doc.GetElement(intersectionPoint.LevelId) as Level).Elevation);
                //intersectionPoint.get_Parameter(this.levelOffsetGuid).Set(intersectionPoint.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).AsDouble() - (50 / 304.8));
                this.intersectionFloorRectangularCombineList.Add(holeTask);

                t.Commit();
            }

            //intersectionPoint.get_Parameter(this.intersectionPointWidthGuid).Set(intersectionPointWidth);
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
        /// <returns></returns>
        private OrientaionType GetElementOrientationType(Element wallОrFloor)
        {
            if (wallОrFloor.GetType() == typeof(Wall))
            {
                return OrientaionType.Horizontal;
            }
            else
            {
                return OrientaionType.Vertical;
            }
        }

        private XYZ GetIntersectionCurveCenter(SolidCurveIntersection intersection)
        {
            ///Добавить перегрузку метода, которая возвращает длину линии пересечения
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

        ///upd01 Для тестирования скорости 
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
        }

        private double CalculatedWidth(
            OrientaionType orientaionType,
            double mepWidth,
            Element intersectedElement,
            SolidCurveIntersection intersection)
        {
            if (orientaionType == OrientaionType.Vertical)
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
                    double calculatedWidth = Math.Sqrt((hypotenuseTriangle * hypotenuseTriangle) - (legTriangle * legTriangle));
                    mepWidth = mepWidth + (2 * calculatedWidth);
                    return mepWidth;
                }
            }
        }

        private double AngleToRotateHoleTask(Element mepElement, OrientaionType orientaionType, FamilyInstance holeTask, SolidCurveIntersection intersection, Element intersectedElement, Level lvl)
        {
            MEPCurve curve = mepElement as MEPCurve;
            if (orientaionType == OrientaionType.Vertical)
            {
                foreach (Connector c in curve.ConnectorManager.Connectors)
                {
                    try
                    {
                        double rotationOfMepElement = Math.Asin(-c.CoordinateSystem.BasisY.X) - (Math.PI / 2);
                        if (rotationOfMepElement != 0)
                        {
                            XYZ point1 = intersection.GetCurveSegment(0).GetEndPoint(0);
                            XYZ point2 = intersection.GetCurveSegment(0).GetEndPoint(1);
                            Line axis = Line.CreateBound(point1, point2);

                            double Z = (axis.GetEndPoint(0).Z + axis.GetEndPoint(1).Z) / 2;
                            point1 = new XYZ(point1.X, point1.Y, Z);
                            point2 = new XYZ(point2.X, point2.Y, Z);
                            axis = Line.CreateBound(point1, point2);

                            ElementTransformUtils.RotateElement(this.doc, holeTask.Id, axis, rotationOfMepElement);
                        }
                    }
                    catch { }
                }
            }
            else // walls
            {
                Curve pipeCurve = (mepElement.Location as LocationCurve).Curve;
                Wall wall = intersectedElement as Wall;
                XYZ wallOrientation = wall.Orientation;
                double a = Math.Round((wallOrientation.AngleTo((pipeCurve as Line).Direction)) * (180 / Math.PI), 6);

                if (a > 90 && a < 180)
                {
                    a = (180 - a) * (Math.PI / 180);
                }
                else
                {
                    a = a * (Math.PI / 180);
                }

                XYZ intersectionCurveStartPoint = intersection.GetCurveSegment(0).GetEndPoint(0);
                XYZ intersectionCurveEndPoint = intersection.GetCurveSegment(0).GetEndPoint(1);

                double intersectionPointHeight = 300;

                XYZ originIntersectionCurve = ((intersectionCurveStartPoint + intersectionCurveEndPoint) / 2) - (intersectionPointHeight / 2) * XYZ.BasisZ;

                originIntersectionCurve = new XYZ(originIntersectionCurve.X, originIntersectionCurve.Y, originIntersectionCurve.Z - lvl.Elevation);

                Line rotationLine = Line.CreateBound(originIntersectionCurve, originIntersectionCurve + 1 * XYZ.BasisZ);

                double rotationAngle = 0;

                if (Math.Round(wallOrientation.AngleOnPlaneTo(holeTask.FacingOrientation, XYZ.BasisZ), 6) * (180 / Math.PI) < 180)
                {
                    rotationAngle = -wallOrientation.AngleTo(holeTask.FacingOrientation);
                }
                else
                {
                    rotationAngle = wallOrientation.AngleTo(holeTask.FacingOrientation);
                }

                ElementTransformUtils.RotateElement(doc, holeTask.Id, rotationLine, rotationAngle- Math.PI/2);

            }


            return 0;
        }
    }
}