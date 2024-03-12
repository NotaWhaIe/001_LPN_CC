// <copyright file="HoleTasksJoiner.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Strana.Revit.HoleTask.Extensions;
using Strana.Revit.HoleTask.Extensions.RevitElement;

namespace Strana.Revit.HoleTask.Utils
{
    internal class HoleTasksJoiner
    {
        private static bool areJoin => Confing.Default.areJoin;

        /// <summary>
        /// list of hole tasks after joining.
        /// </summary>
        /// <param name="allFamilyInstances00"></param>
        /// <returns></returns>
        /// <remark>Intersected volume don't calculeted corrected, that whay I used try-catch</remark>t
        internal List<FamilyInstance> JoinAllHoleTask(List<FamilyInstance> allFamilyInstances00)
        {
            var firstInstance = allFamilyInstances00.FirstOrDefault();

            // Если нет элементов или документ первого элемента равен null, возвращаем исходный список
            if (firstInstance == null || firstInstance.Document == null)
            {
                return allFamilyInstances00;
            }

            // Теперь можно безопасно использовать документ, так как мы проверили его наличие
            Document doc = firstInstance.Document;
            List<FamilyInstance> intersectionWallRectangularCombineList01 = new();
            List<FamilyInstance> intersectionFloorRectangularCombineList02 = new();
            HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Стены_Прямоугольное", intersectionWallRectangularCombineList01);
            HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Перекрытия_Прямоугольное", intersectionFloorRectangularCombineList02);

            if (!areJoin)
            {
                return allFamilyInstances00;
            }

            List<FamilyInstance> copyOfAllFamilyInstances = new();
            if (allFamilyInstances00.Count != 0)
            {
                HoleTaskFamilyLoader familyLoader = new(doc);
                FamilySymbol holeFamilySymbol;



                Options opt = new();
                opt.ComputeReferences = true;
                opt.DetailLevel = ViewDetailLevel.Fine;
                string info;



                while (intersectionWallRectangularCombineList01.Count != 0)
                {
                    string holeTaskWidth = "Глубина";
                    string holeTaskHeight = "Высота";
                    string holeTaskThickness = "Ширина";

                    double width = 0; // (Ширина)
                    double height = 0;// (Высота)
                    double thickness = 0; // (Толщина)

                    holeFamilySymbol = familyLoader.WallFamilySymbol;
                    string holeTaskName = intersectionWallRectangularCombineList01.First()?.Name.ToString();

                    List<FamilyInstance> intersectionWallRectangularSolidIntersectCombineList001 = new()
                             {
                                intersectionWallRectangularCombineList01[0],
                             };
                    intersectionWallRectangularCombineList01.RemoveAt(0);

                    List<FamilyInstance> tmpIntersectionWallRectangularSolidIntersectCombineList = [.. intersectionWallRectangularCombineList01];
                    for (int i = 0; i < intersectionWallRectangularSolidIntersectCombineList001.Count; i++)
                    {
                        FamilyInstance firstIntersectionPoint = intersectionWallRectangularSolidIntersectCombineList001[i];
                        Solid firstIntersectionPointSolid = firstIntersectionPoint.GetHoleTaskSolidWithDelta();

                        for (int j = 0; j < tmpIntersectionWallRectangularSolidIntersectCombineList.Count; j++)
                        {
                            FamilyInstance secondIntersectionPoint = tmpIntersectionWallRectangularSolidIntersectCombineList[j];
                            Solid secondIntersectionPointSolid = secondIntersectionPoint.GetHoleTaskSolidWithDelta();
                            double unionvolume = 0;

                            try
                            {
                                unionvolume = BooleanOperationsUtils.ExecuteBooleanOperation(
                                    firstIntersectionPointSolid,
                                    secondIntersectionPointSolid,
                                    BooleanOperationsType.Intersect).Volume;
                            }
                            catch (Exception)
                            {
                                /// Do nothing.
                            }

                            if (unionvolume > 0)
                            {
                                intersectionWallRectangularSolidIntersectCombineList001
                                    .Add(secondIntersectionPoint);
                                tmpIntersectionWallRectangularSolidIntersectCombineList
                                    .Remove(secondIntersectionPoint);
                                i = 0;
                                j = 0;
                            }
                        }
                    }

                    if (intersectionWallRectangularSolidIntersectCombineList001.Count > 1)
                    {
                        List<XYZ> pointsList = new();
                        double intersectionPointThickness = 0;
                        foreach (FamilyInstance holeTask in intersectionWallRectangularSolidIntersectCombineList001)
                        {
                            info = holeTask.LookupParameter(":Назначение отверстия").ToString();
                            width = holeTask.LookupParameter(holeTaskWidth).AsDouble();
                            height = holeTask.LookupParameter(holeTaskHeight).AsDouble();
                            thickness = holeTask.LookupParameter(holeTaskThickness).AsDouble();



                            XYZ originPoint = (holeTask.Location as LocationPoint).Point;

                            XYZ holeTaskholeTask = holeTask.HandOrientation;
                            XYZ holeTaskholeTaskTest = new XYZ(holeTaskholeTask.Y, holeTaskholeTask.X, holeTaskholeTask.Z);

                            XYZ downLeftPoint = originPoint + (width / 2) * holeTaskholeTaskTest - (height / 2 * XYZ.BasisZ);
                            pointsList.Add(downLeftPoint);

                            XYZ downRightPoint = originPoint + width / 2 * holeTaskholeTaskTest.Negate() - (height / 2 * XYZ.BasisZ);
                            pointsList.Add(downRightPoint);

                            XYZ upLeftPoint = originPoint + (width / 2 * holeTaskholeTaskTest) + (height / 2 * XYZ.BasisZ);
                            pointsList.Add(upLeftPoint);

                            XYZ upRightPoint = originPoint + ((width / 2) * holeTaskholeTaskTest.Negate()) + height / 2 * XYZ.BasisZ;
                            pointsList.Add(upRightPoint);

                            if (thickness > intersectionPointThickness)
                            {
                                intersectionPointThickness = thickness;
                            }
                        }

                        /// Find the center and calculete sizes.
                        double maxHorizontalDistance = 0;
                        double maxVerticalDistance = 0;
                        XYZ pointP1 = null;
                        XYZ pointP2 = null;
                        XYZ pointP3 = null;
                        XYZ pointP4 = null;
                        foreach (XYZ p1 in pointsList)
                        {
                            foreach (XYZ p2 in pointsList)
                            {
                                if (new XYZ(p1.X, p1.Y, 0).DistanceTo(new XYZ(p2.X, p2.Y, 0)) > maxHorizontalDistance)
                                {
                                    maxHorizontalDistance = new XYZ(p1.X, p1.Y, 0).DistanceTo(new XYZ(p2.X, p2.Y, 0));
                                    pointP1 = p1;
                                    pointP2 = p2;
                                }
                                if (new XYZ(0, 0, p1.Z).DistanceTo(new XYZ(0, 0, p2.Z)) > maxVerticalDistance)
                                {
                                    maxVerticalDistance = new XYZ(0, 0, p1.Z).DistanceTo(new XYZ(0, 0, p2.Z));
                                    pointP3 = p1;
                                    pointP4 = p2;
                                }
                            }
                        }

                        XYZ midPointLeftRight = (pointP1 + pointP2) / 2;
                        XYZ midPointUpDown = (pointP3 + pointP4) / 2;
                        XYZ centroidIntersectionPoint = new(
                            midPointLeftRight.X,
                            midPointLeftRight.Y,
                            midPointUpDown.Z);

                        List<XYZ> combineDownLeftPointList = [];
                        List<XYZ> combineDownRightPointList = [];
                        List<XYZ> combineUpLeftPointList = [];
                        List<XYZ> combineUpRightPointList = [];

                        XYZ pointFacingOrientation = intersectionWallRectangularSolidIntersectCombineList001
                            .First().FacingOrientation;
                        XYZ pointHandOrientation = intersectionWallRectangularSolidIntersectCombineList001
                            .First().HandOrientation;

                        foreach (XYZ p in pointsList)
                        {
                            XYZ vectorToPoint = (p - centroidIntersectionPoint).Normalize();
                            /// lower left corner
                            if (pointFacingOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2
                                && pointHandOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2)
                            {
                                combineDownLeftPointList.Add(p);
                            }

                            /// lower right corner
                            if (pointFacingOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2
                                && pointHandOrientation.AngleTo(vectorToPoint) <= Math.PI / 2)
                            {
                                combineDownRightPointList.Add(p);
                            }

                            /// upper left corner
                            if (pointFacingOrientation.AngleTo(vectorToPoint) <= Math.PI / 2
                                && pointHandOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2)
                            {
                                combineUpLeftPointList.Add(p);
                            }

                            /// upper right corner
                            if (pointFacingOrientation.AngleTo(vectorToPoint) <= Math.PI / 2
                                && pointHandOrientation.AngleTo(vectorToPoint) <= Math.PI / 2)
                            {
                                combineUpRightPointList.Add(p);
                            }
                        }

                        List<XYZ> maxRightPointList = [.. combineDownRightPointList, .. combineUpRightPointList];
                        double maxRightDistance = -1000000;

                        /// .Z = 0.0 to calculate the distance on the XY plane
                        List<XYZ> pointsListZ0 = new List<XYZ>(maxRightPointList);
                        for (int i = 0; i < pointsListZ0.Count; i++)
                        {
                            pointsListZ0[i] = new XYZ(pointsListZ0[i].X, pointsListZ0[i].Y, 0.0);
                        }

                        XYZ centroidIntersectionPointZ0 = new XYZ(centroidIntersectionPoint.X, centroidIntersectionPoint.Y, 0.0);

                        foreach (XYZ p in pointsListZ0)
                        {
                            if (p.DistanceTo(centroidIntersectionPointZ0) > maxRightDistance)
                            {
                                maxRightDistance = p.DistanceTo(centroidIntersectionPointZ0);
                            }
                        }

                        double intersectionPointWidthCalculete = maxRightDistance * 2;
                        double minZ = 10000000000;
                        XYZ minZPoint = null;
                        double maxZ = -10000000000;
                        XYZ maxZPoint = null;
                        foreach (XYZ p in pointsList)
                        {
                            if (p.Z < minZ)
                            {
                                minZ = p.Z;
                                minZPoint = p;
                            }
                            if (p.Z > maxZ)
                            {
                                maxZ = p.Z;
                                maxZPoint = p;
                            }
                        }


                        double intersectionPointHeight = maxZPoint.Z - minZPoint.Z;
                        XYZ newCenterPoint = new XYZ(centroidIntersectionPoint.X, centroidIntersectionPoint.Y, centroidIntersectionPoint.Z - (doc.GetElement(intersectionWallRectangularSolidIntersectCombineList001.First().LevelId) as Level).Elevation);
                        double roundHTWidth = HoleTasksRoundUpDimension.RoundUpParameter(intersectionPointWidthCalculete);
                        double roundHTThickness = HoleTasksRoundUpDimension.RoundUpParameter(intersectionPointThickness);
                        double roundHTHeight = HoleTasksRoundUpDimension.RoundUpParameter(intersectionPointHeight);

                        //if (!FindMatchingInstance(GlobalParameters.ЕxistingTaskFloor, newCenterPoint, 0.03))
                        //{
                        //    return intersectionWallRectangularCombineList01;
                        //}

                        FamilyInstance intersectionPoint = doc.Create.NewFamilyInstance(
                        newCenterPoint,
                        holeFamilySymbol,
                        doc.GetElement(intersectionWallRectangularSolidIntersectCombineList001
                            .First().LevelId) as Level, StructuralType.NonStructural);

                        if (Math.Round(intersectionWallRectangularSolidIntersectCombineList001.First().FacingOrientation.AngleTo(intersectionPoint.FacingOrientation), 6) != 0)
                        {
                            Line rotationLine = Line.CreateBound(newCenterPoint, newCenterPoint + (1 * XYZ.BasisZ));
                            ElementTransformUtils.RotateElement(
                                doc,
                                intersectionPoint.Id,
                                rotationLine,
                                intersectionWallRectangularSolidIntersectCombineList001.First()
                                    .FacingOrientation.AngleTo(intersectionPoint.FacingOrientation));
                        }


                        intersectionPoint.LookupParameter(holeTaskWidth).Set(roundHTWidth);
                        intersectionPoint.LookupParameter(holeTaskThickness).Set(roundHTThickness);
                        intersectionPoint.LookupParameter(holeTaskHeight).Set(roundHTHeight);
                        intersectionPoint.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM).Set(newCenterPoint.Z);

                        var locationPoint = intersectionPoint.Location as LocationPoint;
                        double rotationAngle = locationPoint?.Rotation ?? 0.0;

                        HoleTaskGridDelta delta = GridRoundUpDimension.DeltaHoleTaskToGrids(doc, newCenterPoint, roundHTThickness, roundHTWidth, rotationAngle);
                        double O1 = UnitUtils.ConvertToInternalUnits(delta.DeltaGridNumber, UnitTypeId.Millimeters);
                        double Oa = UnitUtils.ConvertToInternalUnits(delta.deltaGridLetter, UnitTypeId.Millimeters);
                        HoleTaskCreator.MoveFamilyInstance(intersectionPoint, O1, "X");
                        ///сдвинуть семейство по оси фУ в верх, от оси и А
                        HoleTaskCreator.MoveFamilyInstance(intersectionPoint, Oa, "Y");

                        intersectionPoint.LookupParameter(":Назначение отверстия")?.Set(GlobalParameters.SectionName);
                        intersectionPoint.LookupParameter(":Примечание")?.Set(GlobalParameters.LinkInfo);
                        intersectionPoint.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(GlobalParameters.Date);
                        intersectionPoint.LookupParameter("SD_Версия задания")?.Set(GlobalParameters.UserName);
                        GlobalParameters.SetScriptCreationMethod(intersectionPoint);


                        foreach (FamilyInstance forDel in intersectionWallRectangularSolidIntersectCombineList001)
                        {

                            try
                            {
                                doc.Delete(forDel.Id);
                                intersectionWallRectangularCombineList01.Remove(forDel);

                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    else
                    {
                        intersectionWallRectangularCombineList01.Remove(intersectionWallRectangularSolidIntersectCombineList001[0]);
                    }
                }
                //}




                while (intersectionFloorRectangularCombineList02.Count != 0)
                {
                    string holeTaskWidth = "Ширина";
                    string holeTaskHeight = "Глубина";
                    string holeTaskThickness = "Высота";

                    holeFamilySymbol = familyLoader.FloorFamilySymbol;
                    string holeTaskName = intersectionFloorRectangularCombineList02.First()?.Name.ToString();
                    List<FamilyInstance> intersectionFloorRectangularSolidIntersectCombineList002 =
                        [intersectionFloorRectangularCombineList02[0]];
                    intersectionFloorRectangularCombineList02.RemoveAt(0);
                    List<FamilyInstance> tmpIntersectionFloorRectangularSolidIntersectCombineList =
                        [.. intersectionFloorRectangularCombineList02];
                    for (int i = 0; i < intersectionFloorRectangularSolidIntersectCombineList002.Count; i++)
                    {
                        FamilyInstance firstIntersectionPoint = intersectionFloorRectangularSolidIntersectCombineList002[i];
                        Solid firstIntersectionPointSolid = firstIntersectionPoint.GetHoleTaskSolidWithDelta();

                        for (int j = 0; j < tmpIntersectionFloorRectangularSolidIntersectCombineList.Count; j++)
                        {
                            FamilyInstance secondIntersectionPoint =
                                tmpIntersectionFloorRectangularSolidIntersectCombineList[j];
                            Solid secondIntersectionPointSolid = secondIntersectionPoint.GetHoleTaskSolidWithDelta();
                            double unionVolume = 0;

                            try
                            {
                                unionVolume = BooleanOperationsUtils.ExecuteBooleanOperation(
                                   firstIntersectionPointSolid,
                                   secondIntersectionPointSolid,
                                   BooleanOperationsType.Intersect).Volume;

                            }
                            catch (Exception)
                            {
                                /// Do nothing.
                            }

                            if (unionVolume > 0)
                            {
                                intersectionFloorRectangularSolidIntersectCombineList002
                                    .Add(secondIntersectionPoint);
                                tmpIntersectionFloorRectangularSolidIntersectCombineList
                                    .Remove(secondIntersectionPoint);
                                i = 0;
                                j = 0;
                            }
                        }
                    }

                    if (intersectionFloorRectangularSolidIntersectCombineList002.Count > 1)
                    {
                        List<XYZ> pointsList = new List<XYZ>();
                        double intersectionPointThickness = 0;
                        foreach (FamilyInstance holeTask in intersectionFloorRectangularSolidIntersectCombineList002)
                        {
                            XYZ originPoint = (holeTask.Location as LocationPoint).Point;

                            XYZ downLeftPoint = originPoint +
                                (holeTask.LookupParameter(holeTaskWidth).AsDouble() / 2 * holeTask.HandOrientation.Negate()) +
                                (holeTask.LookupParameter(holeTaskHeight).AsDouble() / 2 * holeTask.FacingOrientation.Negate());
                            pointsList.Add(downLeftPoint);

                            XYZ downRightPoint = originPoint +
                                (holeTask.LookupParameter(holeTaskWidth).AsDouble() / 2 * holeTask.HandOrientation) +
                                (holeTask.LookupParameter(holeTaskHeight).AsDouble() / 2 * holeTask.FacingOrientation.Negate());
                            pointsList.Add(downRightPoint);

                            XYZ upLeftPoint = originPoint +
                                (holeTask.LookupParameter(holeTaskWidth).AsDouble() / 2 * holeTask.HandOrientation.Negate()) +
                                (holeTask.LookupParameter(holeTaskHeight).AsDouble() / 2 * holeTask.FacingOrientation);
                            pointsList.Add(upLeftPoint);

                            XYZ upRightPoint = originPoint +
                                (holeTask.LookupParameter(holeTaskWidth).AsDouble() / 2 * holeTask.HandOrientation) +
                                (holeTask.LookupParameter(holeTaskHeight).AsDouble() / 2 * holeTask.FacingOrientation);
                            pointsList.Add(upRightPoint);

                            if (holeTask.LookupParameter(holeTaskThickness).AsDouble() > intersectionPointThickness)
                            {
                                intersectionPointThickness = holeTask
                                    .LookupParameter(holeTaskThickness).AsDouble();
                            }
                        }

                        double maxX = pointsList.Max(p => p.X);
                        double minX = pointsList.Min(p => p.X);
                        double maxY = pointsList.Max(p => p.Y);
                        double minY = pointsList.Min(p => p.Y);
                        XYZ centroidIntersectionPoint = new((maxX + minX) /
                            2, (maxY + minY) / 2, pointsList.First().Z);

                        List<XYZ> combineDownLeftPointList = new();
                        List<XYZ> combineDownRightPointList = new();
                        List<XYZ> combineUpLeftPointList = new();
                        List<XYZ> combineUpRightPointList = new();

                        XYZ pointFacingOrientation = intersectionFloorRectangularSolidIntersectCombineList002
                            .First().FacingOrientation;
                        XYZ pointHandOrientation = intersectionFloorRectangularSolidIntersectCombineList002
                            .First().HandOrientation;
                        Level pointLevel = doc.GetElement(intersectionFloorRectangularSolidIntersectCombineList002
                            .First().LevelId) as Level;
                        double pointLevelElevation = pointLevel.Elevation;
                        foreach (XYZ p in pointsList)
                        {
                            XYZ vectorToPoint = (p - centroidIntersectionPoint).Normalize();
                            /// lower left corner
                            if (pointFacingOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2
                                && pointHandOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2)
                            {
                                combineDownLeftPointList.Add(p);
                            }

                            /// lower right corner
                            if (pointFacingOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2
                                && pointHandOrientation.AngleTo(vectorToPoint) <= Math.PI / 2)
                            {
                                combineDownRightPointList.Add(p);
                            }

                            /// upper left corner
                            if (pointFacingOrientation.AngleTo(vectorToPoint) <= Math.PI / 2
                                && pointHandOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2)
                            {
                                combineUpLeftPointList.Add(p);
                            }

                            /// upper right corner
                            if (pointFacingOrientation.AngleTo(vectorToPoint) <= Math.PI / 2
                                && pointHandOrientation.AngleTo(vectorToPoint) <= Math.PI / 2)
                            {
                                combineUpRightPointList.Add(p);
                            }
                        }

                        List<XYZ> maxUpPointList = [.. combineUpLeftPointList, .. combineUpRightPointList];
                        double maxUpDistance = -1000000;
                        foreach (XYZ p in maxUpPointList)
                        {
                            XYZ x = Line.CreateBound(centroidIntersectionPoint, centroidIntersectionPoint +
                                (1000000 * pointFacingOrientation)).Project(p).XYZPoint;
                            if (x.DistanceTo(centroidIntersectionPoint) > maxUpDistance)
                            {
                                maxUpDistance = x.DistanceTo(centroidIntersectionPoint);
                            }
                        }

                        List<XYZ> maxDownPointList = [.. combineDownLeftPointList, .. combineDownRightPointList];
                        double maxDownDistance = -1000000;
                        foreach (XYZ p in maxDownPointList)
                        {
                            XYZ x = Line.CreateBound(centroidIntersectionPoint, centroidIntersectionPoint +
                                (1000000 * pointFacingOrientation.Negate())).Project(p).XYZPoint;
                            if (x.DistanceTo(centroidIntersectionPoint) > maxDownDistance)
                            {
                                maxDownDistance = x.DistanceTo(centroidIntersectionPoint);
                            }
                        }

                        List<XYZ> maxRightPointList = [.. combineDownRightPointList, .. combineUpRightPointList];
                        double maxRightDistance = -1000000;
                        foreach (XYZ p in maxRightPointList)
                        {
                            XYZ x = Line.CreateBound(centroidIntersectionPoint, centroidIntersectionPoint +
                                (1000000 * pointHandOrientation)).Project(p).XYZPoint;
                            if (x.DistanceTo(centroidIntersectionPoint) > maxRightDistance)
                            {
                                maxRightDistance = x.DistanceTo(centroidIntersectionPoint);
                            }
                        }

                        List<XYZ> maxLeftPointList = [.. combineDownLeftPointList, .. combineUpLeftPointList];
                        double maxLeftDistance = -1000000;
                        foreach (XYZ p in maxLeftPointList)
                        {
                            XYZ x = Line.CreateBound(centroidIntersectionPoint, centroidIntersectionPoint +
                                (1000000 * pointHandOrientation.Negate())).Project(p).XYZPoint;
                            if (x.DistanceTo(centroidIntersectionPoint) > maxLeftDistance)
                            {
                                maxLeftDistance = x.DistanceTo(centroidIntersectionPoint);
                            }
                        }

                        double intersectionPointHeight = maxUpDistance + maxDownDistance;
                        double intersectionPointWidth = maxLeftDistance + maxRightDistance;

                        XYZ newCenterPoint = new XYZ(
                            centroidIntersectionPoint.X,
                            centroidIntersectionPoint.Y,
                            centroidIntersectionPoint.Z - pointLevelElevation);

                        double roundHTWidth = HoleTasksRoundUpDimension.RoundUpParameter(intersectionPointWidth);
                        double roundHTThickness = HoleTasksRoundUpDimension.RoundUpParameter(intersectionPointThickness);
                        double roundHTHeight = HoleTasksRoundUpDimension.RoundUpParameter(intersectionPointHeight);

                        //if (!FindMatchingInstance(GlobalParameters.ЕxistingTaskFloor, newCenterPoint, 0.03))
                        //{
                        //    return intersectionFloorRectangularCombineList02;

                        //}

                        FamilyInstance intersectionPoint = doc.Create.NewFamilyInstance(
                            newCenterPoint,
                            holeFamilySymbol,
                            pointLevel,
                            StructuralType.NonStructural);

                        intersectionPoint.LookupParameter(holeTaskWidth).Set(roundHTWidth);
                        intersectionPoint.LookupParameter(holeTaskThickness).Set(roundHTThickness);
                        intersectionPoint.LookupParameter(holeTaskHeight).Set(roundHTHeight);

                        double rotationAngle = pointFacingOrientation
                           .AngleTo(intersectionPoint.FacingOrientation);
                        if (rotationAngle != 0)
                        {
                            Line rotationAxis = Line.CreateBound(
                                newCenterPoint, newCenterPoint + (1 * XYZ.BasisZ));
                            ElementTransformUtils.RotateElement(
                                doc,
                                intersectionPoint.Id,
                                rotationAxis,
                                rotationAngle);
                        }



                        var locationPoint = intersectionPoint.Location as LocationPoint;
                        double rotationAngle1 = locationPoint?.Rotation ?? 0.0;

                        HoleTaskGridDelta delta = GridRoundUpDimension.DeltaHoleTaskToGrids(doc, newCenterPoint, roundHTThickness, roundHTWidth, rotationAngle1);

                        double O1 = UnitUtils.ConvertToInternalUnits(delta.DeltaGridNumber, UnitTypeId.Millimeters);
                        double Oa = UnitUtils.ConvertToInternalUnits(delta.deltaGridLetter, UnitTypeId.Millimeters);
                        HoleTaskCreator.MoveFamilyInstance(intersectionPoint, O1, "X");
                        ///сдвинуть семейство по оси У в верх, от оси и А
                        HoleTaskCreator.MoveFamilyInstance(intersectionPoint, Oa, "Y");

                        intersectionPoint.LookupParameter(":Назначение отверстия")?.Set(GlobalParameters.SectionName);
                        intersectionPoint.LookupParameter(":Примечание")?.Set(GlobalParameters.LinkInfo);
                        intersectionPoint.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(GlobalParameters.Date);
                        intersectionPoint.LookupParameter("SD_Версия задания")?.Set(GlobalParameters.UserName);
                        GlobalParameters.SetScriptCreationMethod(intersectionPoint);

                        foreach (FamilyInstance forDel in intersectionFloorRectangularSolidIntersectCombineList002)
                        {
                            try
                            {

                                doc.Delete(forDel.Id);
                                intersectionFloorRectangularCombineList02.Remove(forDel);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    else
                    {
                        intersectionFloorRectangularCombineList02.Remove(intersectionFloorRectangularSolidIntersectCombineList002[0]);
                    }
                }
                //}
                allFamilyInstances00 = intersectionWallRectangularCombineList01
                .Concat(intersectionFloorRectangularCombineList02)
                .ToList();

                List<FamilyInstance> copyOfAllFamilyInstancesWall = new();
                List<FamilyInstance> copyOfAllFamilyInstancesFloor = new();
                HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Стены_Прямоугольное", copyOfAllFamilyInstancesWall);
                HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Перекрытия_Прямоугольное", copyOfAllFamilyInstancesFloor);
                copyOfAllFamilyInstances.AddRange(copyOfAllFamilyInstancesWall.Concat(copyOfAllFamilyInstancesFloor));

            }
            return copyOfAllFamilyInstances;
        }
        public static bool FindMatchingInstance(
        List<FamilyInstance> startHoleTask,
        XYZ newCenterPoint,
        double tolerance)
        {
            foreach (var instance in startHoleTask)
            {
                LocationPoint locationPoint = instance.Location as LocationPoint;
                if (locationPoint != null)
                {
                    // Вычисляем расстояние от центра экземпляра до новой точки центра
                    double distance = (locationPoint.Point - newCenterPoint).GetLength();
                    // Проверяем, находится ли расстояние в пределах допуска
                    if (distance <= tolerance)
                    {
                        return true; // Найдено совпадение
                    }
                }
            }

            return false; // Совпадений не найдено
        }
    }
}