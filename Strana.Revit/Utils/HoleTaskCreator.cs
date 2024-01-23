﻿// <copyright file="HoleTaskCreator.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Strana.Revit.HoleTask.Extensions;
using Strana.Revit.HoleTask.Extensions.RevitElement;
using Strana.Revit.HoleTask.RevitCommands;

namespace Strana.Revit.HoleTask.Utils
{
#nullable enable
    /// <summary> This class contains metod set up a HoleTasks familySybol. </summary>
    internal class HoleTaskCreator
    {
        private readonly Document doc;
        private readonly List<FamilyInstance> intersectionRectangularCombineList = new List<FamilyInstance>();
        private static double clearance => (Confing.Default.offSetHoleTask / 304.8) * 2;

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
        /// <param name="linkInstance">The link instance in current project.</param>
        /// <param name="clearance">Clearance value for the top and bottom of the MEP element.</param>
        /// <param name="roundHoleSizesUpIncrement">The increment used for rounding up hole sizes.</param>
        /// <param name="floorHoleFamilySymbol">The family symbol for creating the hole in the floor.</param>
        /// <returns>
        /// The created FamilyInstance representing the hole task.
        /// </returns>
        public FamilyInstance? PlaceHoleTaskFamilyInstance(
            Element mepElement,
            SolidCurveIntersection intersection,
            Element intersectedElement,
            Document linkDoc,
            RevitLinkInstance linkInstance)
        {
            ///Добавляю в список уже существующие задания на отверстия:
            HoleTasksGetter.AddFamilyInstancesToList(this.doc, "(Отв_Задание)_Стены_Прямоугольное", this.intersectionRectangularCombineList);
            HoleTasksGetter.AddFamilyInstancesToList(this.doc, "(Отв_Задание)_Перекрытия_Прямоугольное", this.intersectionRectangularCombineList);

            OrientaionType orientation = this.GetElementOrientationType(mepElement);
            FamilySymbol holeFamilySymbol;
            HoleTaskFamilyLoader familyLoader = new(this.doc);
            Wall wall = intersectedElement as Wall;
            int tipe;
            if (intersectedElement != wall && orientation == OrientaionType.Vertical)
            {
                holeFamilySymbol = familyLoader.FloorFamilySymbol;
                tipe = 0;
            }
            else if (intersectedElement != wall && orientation == OrientaionType.Horizontal)// Horizontal == Inclined
            {
                holeFamilySymbol = familyLoader.FloorFamilySymbol;
                tipe = 1;
            }
            else
            {
                holeFamilySymbol = familyLoader.WallFamilySymbol;
                tipe = 2;
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

            double holeTaskWidth = (mepHeight + clearance);
            double holeTaskThickness = (this.CalculatedWidth(mepWidth, intersectedElement, intersection) + clearance);
            double holeTaskHeight = intersectedElement.GetInterctedElementThickness() + clearance;

            Level lvl = GetClosestFloorLevel(docLvlList, linkDoc, intersectedElement);
            XYZ intersectionCurveCenter = this.GetIntersectionCurveCenter(intersection);
            intersectionCurveCenter = new XYZ(intersectionCurveCenter.X, intersectionCurveCenter.Y, intersectionCurveCenter.Z - lvl.ProjectElevation);

            ///Добавить в метод чтоб при определении захватывало область вокрг точки вставки
            /// проверка есть ли в intersectionCurveCenter уже ЗНО с теми же геометрическими размерами и в том же месте
            if (!DoesFamilyInstanceExistAtLocation(intersectionCurveCenter))
            {
                holeTask = this.doc.Create.NewFamilyInstance(
                    intersectionCurveCenter,
                    holeFamilySymbol,
                    lvl,
                    StructuralType.NonStructural);
                this.intersectionRectangularCombineList.Add(holeTask);

                double holeTaskAngle = this.RotateHoleTaskAngle(mepElement, orientation, holeTask, intersection, intersectedElement, lvl, linkInstance);

                double holeTaskWidthEX = this.ExchangeParameters(orientation, holeTaskWidth, holeTaskHeight);
                double holeTaskHeightEX = this.ExchangeParameters(orientation, holeTaskHeight, holeTaskWidth);

                double roundHTThickness = HoleTasksRoundUpDimension.RoundUpParameter(holeTaskThickness);
                double roundHTWidth = HoleTasksRoundUpDimension.RoundUpParameter(holeTaskWidthEX);
                double roundHTHeight = HoleTasksRoundUpDimension.RoundUpParameter(holeTaskHeightEX);

                double _roundHTThickness = HoleTasksRoundUpDimension.RoundUpParameter(holeTaskThickness) * 304.8;
                double _roundHTWidth = HoleTasksRoundUpDimension.RoundUpParameter(holeTaskWidthEX) * 304.8;
                double _roundHTHeight = HoleTasksRoundUpDimension.RoundUpParameter(holeTaskHeightEX) * 304.8;

                holeTask.LookupParameter("Глубина").Set(roundHTThickness);
                holeTask.LookupParameter("Ширина").Set(roundHTWidth);
                holeTask.LookupParameter("Высота").Set(roundHTHeight);
                //holeTask.LookupParameter("Глубина").Set(HoleTasksRoundUpDimension.RoundUpParameter(holeTaskThickness)+(delta.deltaGridMax));
                //holeTask.LookupParameter("Ширина").Set(HoleTasksRoundUpDimension.RoundUpParameter(this.ExchangeParameters(orientation, holeTaskWidth, holeTaskHeight)+(delta.deltaGridMax))); // Width
                //holeTask.LookupParameter("Высота").Set(HoleTasksRoundUpDimension.RoundUpParameter(this.ExchangeParameters(orientation, holeTaskHeight, holeTaskWidth))); // Height

                HoleTaskGridDelta delta = GridRoundUpDimension.DeltaHoleTaskToGrids(this.doc, intersectionCurveCenter, roundHTThickness, roundHTWidth, holeTaskAngle);


                this.RotateHoleTask(mepElement, orientation, holeTask, intersection, intersectedElement, lvl, linkInstance);

                double O1;
                double Oa;

                O1 = UnitUtils.ConvertToInternalUnits(delta.DeltaGridNumber, UnitTypeId.Millimeters);
                MoveFamilyInstance(holeTask, O1, "X");

                ///сдвинуть семейство по оси У в верх, от оси и А
                Oa = UnitUtils.ConvertToInternalUnits(delta.deltaGridLetter, UnitTypeId.Millimeters);
                MoveFamilyInstance(holeTask, Oa, "Y");


                return holeTask;
            }
            else
            {
                return null;
            }
        }
        public void MoveFamilyInstance(FamilyInstance familyInstance, double distanceInFeet, string direction)
        {
            if (familyInstance == null)
            {
                throw new ArgumentNullException(nameof(familyInstance));
            }

            if (string.IsNullOrWhiteSpace(direction))
            {
                throw new ArgumentException("Direction cannot be null or empty.", nameof(direction));
            }

            // Получение документа из экземпляра семейства
            Document doc = familyInstance.Document;

            // Определяем вектор перемещения в зависимости от направления
            XYZ moveVector = null;
            if (direction.Equals("X", StringComparison.OrdinalIgnoreCase))
            {
                moveVector = new XYZ(distanceInFeet, 0, 0); // Влево по оси X
            }
            else if (direction.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                moveVector = new XYZ(0, distanceInFeet, 0); // Влево по оси Y
            }
            else
            {
                throw new ArgumentException("Invalid direction. Only 'X' or 'Y' are allowed.", nameof(direction));
            }

            // Перемещаем экземпляр семейства
            ElementTransformUtils.MoveElement(doc, familyInstance.Id, moveVector);
        }

        private bool DoesFamilyInstanceExistAtLocation(XYZ location)
        {
            const double tolerance = 0.02; // Небольшой допуск для сравнения координат

            foreach (FamilyInstance fi in this.intersectionRectangularCombineList)
            {
                XYZ existingLocation = (fi.Location as LocationPoint)?.Point;
                if (existingLocation != null && existingLocation.IsAlmostEqualTo(location, tolerance))
                {
                    return true; // Найден существующий экземпляр в заданных координатах
                }
            }

            return false; // Экземпляр в заданных координатах не найден
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

                double legTriangle = intersectedElement.GetInterctedElementThickness();
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

        private double RotateHoleTaskAngle(Element mepElement, OrientaionType orientaionType, FamilyInstance holeTask, SolidCurveIntersection intersection, Element intersectedElement, Level lvl, RevitLinkInstance linkInstance)
        {
            Wall wall = intersectedElement as Wall;
            MEPCurve curve = mepElement as MEPCurve;
            Transform transform = linkInstance.GetTotalTransform();
            XYZ xAxis = transform.BasisX;
            double linkRotation = Math.Atan2(xAxis.Y, xAxis.X);
            double rotationAngle = 0.0;

            if (intersectedElement != wall && orientaionType == OrientaionType.Vertical)
            {
                rotationAngle = GetAngleFromMEPCurve(curve);
                // Дополнительные действия для вертикального ориентации, если они нужны
            }
            else if (intersectedElement != wall && orientaionType == OrientaionType.Horizontal) // Horizontal == Inclined
            {
                rotationAngle = GetAngleFromMEPCurve(curve) + linkRotation;
                // Дополнительные действия для горизонтального или наклонного ориентации, если они нужны
            }
            else // Wall
            {
                XYZ wallOrientation = wall.Orientation;
                double wallRotationAngle = wallOrientation.AngleTo(holeTask.FacingOrientation);
                if (wallOrientation.AngleOnPlaneTo(holeTask.FacingOrientation, XYZ.BasisZ) < Math.PI)
                {
                    wallRotationAngle *= -1;
                }
                rotationAngle = wallRotationAngle + linkRotation - (Math.PI / 2);
                // Дополнительные действия для ориентации стены, если они нужны
            }

            return rotationAngle;
        }

    }

}
