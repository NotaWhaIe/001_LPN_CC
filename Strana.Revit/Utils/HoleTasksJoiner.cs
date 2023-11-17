﻿// <copyright file="HoleTasksJoiner.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace Strana.Revit.HoleTask.Utils
{
    internal class HoleTasksJoiner
    {
        private readonly Document doc;

        public HoleTasksJoiner()
        {
        }

        public HoleTasksJoiner(Document doc)
        {
            this.doc = doc;
        }

        /// <summary>
        /// list of hole tasks after joining.
        /// </summary>
        /// <param name="allFamilyInstances"></param>
        /// <returns></returns>
        internal List<FamilyInstance> JoinAllHoleTask(List<FamilyInstance> allFamilyInstances)
        {
            Document doc = allFamilyInstances.First().Document;
            Options opt = new();
            HoleTaskFamilyLoader familyLoader = new(doc);
            FamilySymbol holeFamilySymbol;
            opt.ComputeReferences = true;
            opt.DetailLevel = ViewDetailLevel.Fine;
            string holeTaskWidth = "Ширина";
            string holeTaskHeight = "Глубина";
            string holeTaskThickness = "Высота";
            List<FamilyInstance> intersectionWallRectangularCombineList = allFamilyInstances
                .Where(fi => fi.Name.ToString() == "(Отв_Задание)_Стены_Прямоугольное)")
                .ToList();
            List<FamilyInstance> intersectionFloorRectangularCombineList = allFamilyInstances
                 .Where(fi => fi.Name.ToString() == "(Отв_Задание)_Перекрытия_Прямоугольное")
                 .ToList();

            while (intersectionWallRectangularCombineList.Count != 0)
            {
                holeFamilySymbol = familyLoader.WallFamilySymbol;
                string holeTaskName = intersectionWallRectangularCombineList.First()?.Name.ToString();
                List<FamilyInstance> intersectionWallRectangularSolidIntersectCombineList = new()
                {
                   intersectionWallRectangularCombineList[0],
                };
                intersectionWallRectangularCombineList.RemoveAt(0);

                List<FamilyInstance> tmpIntersectionWallRectangularSolidIntersectCombineList =
                    [.. intersectionWallRectangularCombineList];
                for (int i = 0; i < intersectionWallRectangularSolidIntersectCombineList.Count; i++)
                {
                    FamilyInstance firstIntersectionPoint =
                        intersectionWallRectangularSolidIntersectCombineList[i];
                    Solid firstIntersectionPointSolid = null;
                    GeometryElement firstIntersectionPointGeomElem =
                        firstIntersectionPoint.get_Geometry(opt);
                    foreach (GeometryObject geomObj in firstIntersectionPointGeomElem)
                    {
                        GeometryInstance instance = geomObj as GeometryInstance;
                        if (instance != null)
                        {
                            GeometryElement instanceGeometryElement =
                                instance.GetInstanceGeometry();
                            foreach (GeometryObject o in instanceGeometryElement)
                            {
                                Solid solid = o as Solid;
                                if (solid != null && solid.Volume != 0)
                                {
                                    firstIntersectionPointSolid = solid;
                                    break;
                                }
                            }
                        }
                    }

                    for (int j = 0; j < tmpIntersectionWallRectangularSolidIntersectCombineList.Count; j++)
                    {
                        FamilyInstance secondIntersectionPoint =
                            tmpIntersectionWallRectangularSolidIntersectCombineList[j];
                        Solid secondIntersectionPointSolid = null;
                        GeometryElement secondIntersectionPointGeomElem =
                            secondIntersectionPoint.get_Geometry(opt);
                        foreach (GeometryObject geomObj in secondIntersectionPointGeomElem)
                        {
                            GeometryInstance instance = geomObj as GeometryInstance;
                            if (instance != null)
                            {
                                GeometryElement instanceGeometryElement =
                                    instance.GetInstanceGeometry();
                                foreach (GeometryObject o in instanceGeometryElement)
                                {
                                    Solid solid = o as Solid;
                                    if (solid != null && solid.Volume != 0)
                                    {
                                        secondIntersectionPointSolid = solid;
                                        break;
                                    }
                                }
                            }
                        }

                        double unionvolume = BooleanOperationsUtils.ExecuteBooleanOperation(
                            firstIntersectionPointSolid,
                            secondIntersectionPointSolid,
                            BooleanOperationsType.Intersect).Volume;

                        if (unionvolume > 0)
                        {
                            intersectionWallRectangularSolidIntersectCombineList
                                .Add(secondIntersectionPoint);
                            tmpIntersectionWallRectangularSolidIntersectCombineList
                                .Remove(secondIntersectionPoint);
                            i = 0;
                            j = 0;
                        }
                    }
                }

                if (intersectionWallRectangularSolidIntersectCombineList.Count > 1)
                {
                    List<XYZ> pointsList = new();
                    double intersectionPointThickness = 0;
                    foreach (FamilyInstance holeTask in intersectionWallRectangularSolidIntersectCombineList)
                    {
                        //holeTask.Name

                        XYZ originPoint = (holeTask.Location as LocationPoint).Point;
                        XYZ downLeftPoint = originPoint +
                            (holeTask.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskWidth, holeTaskHeight))
                                .AsDouble() / 2) * holeTask.HandOrientation;
                        pointsList.Add(downLeftPoint);

                        XYZ downRightPoint = originPoint +
                            (holeTask.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskWidth, holeTaskHeight))
                                .AsDouble() / 2 * holeTask.HandOrientation.Negate());
                        pointsList.Add(downRightPoint);

                        XYZ upLeftPoint = originPoint +
                            (holeTask.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskWidth, holeTaskHeight))
                                .AsDouble() / 2 * holeTask.HandOrientation) +
                            (holeTask.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskHeight, holeTaskWidth))
                                .AsDouble() * XYZ.BasisZ);
                        pointsList.Add(upLeftPoint);

                        XYZ upRightPoint = originPoint +
                            ((holeTask.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskWidth, holeTaskHeight)).AsDouble() / 2) * holeTask.HandOrientation.Negate()) +
                            holeTask.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskHeight, holeTaskWidth)).AsDouble() * XYZ.BasisZ; pointsList.Add(upRightPoint);

                        if (holeTask.LookupParameter(holeTaskThickness)
                            .AsDouble() > intersectionPointThickness)
                        {
                            intersectionPointThickness = holeTask.LookupParameter(holeTaskThickness)
                                .AsDouble();
                        }
                    }

                    // Найти центр спроецировать точки на одну отметку и померить расстояние
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

                    XYZ pointFacingOrientation = intersectionWallRectangularSolidIntersectCombineList
                        .First().FacingOrientation;
                    XYZ pointHandOrientation = intersectionWallRectangularSolidIntersectCombineList
                        .First().HandOrientation;

                    foreach (XYZ p in pointsList)
                    {
                        XYZ vectorToPoint = (p - centroidIntersectionPoint).Normalize();
                        // Нижний левый угол
                        if (pointFacingOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2
                            && pointHandOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2)
                        {
                            combineDownLeftPointList.Add(p);
                        }

                        // Нижний правый угол
                        if (pointFacingOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2
                            && pointHandOrientation.AngleTo(vectorToPoint) <= Math.PI / 2)
                        {
                            combineDownRightPointList.Add(p);
                        }

                        // Верхний левый угол
                        if (pointFacingOrientation.AngleTo(vectorToPoint) <= Math.PI / 2
                            && pointHandOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2)
                        {
                            combineUpLeftPointList.Add(p);
                        }

                        // Верхний правый угол
                        if (pointFacingOrientation.AngleTo(vectorToPoint) <= Math.PI / 2
                            && pointHandOrientation.AngleTo(vectorToPoint) <= Math.PI / 2)
                        {
                            combineUpRightPointList.Add(p);
                        }
                    }

                    List<XYZ> maxRightPointList = [.. combineDownRightPointList, .. combineUpRightPointList];
                    double maxRightDistance = -1000000;
                    foreach (XYZ p in pointsList)
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
                    foreach (XYZ p in pointsList)
                    {
                        XYZ x = Line.CreateBound(centroidIntersectionPoint, centroidIntersectionPoint +
                            (1000000 * pointHandOrientation.Negate())).Project(p).XYZPoint;
                        if (x.DistanceTo(centroidIntersectionPoint) > maxLeftDistance)
                        {
                            maxLeftDistance = x.DistanceTo(centroidIntersectionPoint);
                        }
                    }

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
                    double intersectionPointWidth = maxLeftDistance + maxRightDistance;
                    XYZ newCenterPoint = new XYZ(
                            centroidIntersectionPoint.X,
                            centroidIntersectionPoint.Y,
                            (centroidIntersectionPoint.Z - (intersectionPointHeight / 2)) -
                            (doc.GetElement(intersectionWallRectangularSolidIntersectCombineList
                                .First().LevelId) as Level).Elevation);
                    FamilyInstance intersectionPoint = doc.Create.NewFamilyInstance(
                        newCenterPoint,
                        holeFamilySymbol,
                        doc.GetElement(intersectionWallRectangularSolidIntersectCombineList
                            .First().LevelId) as Level, StructuralType.NonStructural);

                    if (Math.Round(intersectionWallRectangularSolidIntersectCombineList.First().FacingOrientation.AngleTo(intersectionPoint.FacingOrientation), 6) != 0)
                    {
                        Line rotationLine = Line.CreateBound(newCenterPoint, newCenterPoint + (1 * XYZ.BasisZ));
                        ElementTransformUtils.RotateElement(
                            doc,
                            intersectionPoint.Id,
                            rotationLine,
                            intersectionWallRectangularSolidIntersectCombineList.First()
                                .FacingOrientation.AngleTo(intersectionPoint.FacingOrientation));
                    }

                    intersectionPoint.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskWidth, holeTaskHeight)).Set(intersectionPointWidth);
                    intersectionPoint.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskHeight, holeTaskWidth)).Set(intersectionPointHeight);
                    intersectionPoint.LookupParameter(holeTaskThickness).Set(intersectionPointThickness);
                    intersectionPoint.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM).Set(newCenterPoint.Z);
                    //intersectionPoint.get_Parameter(heightOfBaseLevelGuid).Set((doc.GetElement(intersectionPoint.LevelId)
                    //as Level).Elevation);
                    //intersectionPoint.get_Parameter(levelOffsetGuid).Set(newCenterPoint.Z);

                    foreach (FamilyInstance forDel in intersectionWallRectangularSolidIntersectCombineList)
                    {
                        doc.Delete(forDel.Id);
                        intersectionWallRectangularCombineList.Remove(forDel);
                        //return intersectionWallRectangularCombineList;
                    }
                }
                else
                {
                    intersectionWallRectangularCombineList.Remove(intersectionWallRectangularSolidIntersectCombineList[0]);
                    //return intersectionWallRectangularCombineList;
                }
                //return intersectionWallRectangularCombineList;
            }

            while (intersectionFloorRectangularCombineList.Count != 0)
            {
                holeFamilySymbol = familyLoader.FloorFamilySymbol;
                string holeTaskName = intersectionFloorRectangularCombineList.First()?.Name.ToString();
                List<FamilyInstance> intersectionFloorRectangularSolidIntersectCombineList =
                    [intersectionFloorRectangularCombineList[0]];
                intersectionFloorRectangularCombineList.RemoveAt(0);
                List<FamilyInstance> tmpIntersectionFloorRectangularSolidIntersectCombineList =
                    [.. intersectionFloorRectangularCombineList];
                for (int i = 0; i < intersectionFloorRectangularSolidIntersectCombineList.Count; i++)
                {
                    FamilyInstance firstIntersectionPoint = intersectionFloorRectangularSolidIntersectCombineList[i];
                    Solid firstIntersectionPointSolid = null;
                    GeometryElement firstIntersectionPointGeomElem = firstIntersectionPoint.get_Geometry(opt);
                    foreach (GeometryObject geomObj in firstIntersectionPointGeomElem)
                    {
                        GeometryInstance instance = geomObj as GeometryInstance;
                        if (instance != null)
                        {
                            GeometryElement instanceGeometryElement = instance.GetInstanceGeometry();
                            foreach (GeometryObject o in instanceGeometryElement)
                            {
                                Solid solid = o as Solid;
                                if (solid != null && solid.Volume != 0)
                                {
                                    firstIntersectionPointSolid = solid;
                                    break;
                                }
                            }
                        }
                    }

                    for (int j = 0; j < tmpIntersectionFloorRectangularSolidIntersectCombineList.Count; j++)
                    {
                        FamilyInstance secondIntersectionPoint =
                            tmpIntersectionFloorRectangularSolidIntersectCombineList[j];
                        Solid secondIntersectionPointSolid = null;
                        GeometryElement secondIntersectionPointGeomElem = secondIntersectionPoint.get_Geometry(opt);
                        foreach (GeometryObject geomObj in secondIntersectionPointGeomElem)
                        {
                            GeometryInstance instance = geomObj as GeometryInstance;
                            if (instance != null)
                            {
                                GeometryElement instanceGeometryElement = instance.GetInstanceGeometry();
                                foreach (GeometryObject o in instanceGeometryElement)
                                {
                                    Solid solid = o as Solid;
                                    if (solid != null && solid.Volume != 0)
                                    {
                                        secondIntersectionPointSolid = solid;
                                        break;
                                    }
                                }
                            }
                        }

                        double unionvolume = BooleanOperationsUtils.ExecuteBooleanOperation(
                            firstIntersectionPointSolid,
                            secondIntersectionPointSolid,
                            BooleanOperationsType.Intersect).Volume;

                        if (unionvolume > 0)
                        {
                            intersectionFloorRectangularSolidIntersectCombineList
                                .Add(secondIntersectionPoint);
                            tmpIntersectionFloorRectangularSolidIntersectCombineList
                                .Remove(secondIntersectionPoint);
                            i = 0;
                            j = 0;
                        }
                    }
                }

                if (intersectionFloorRectangularSolidIntersectCombineList.Count > 1)
                {
                    List<XYZ> pointsList = new List<XYZ>();
                    double intersectionPointThickness = 0;
                    foreach (FamilyInstance holeTask in intersectionFloorRectangularSolidIntersectCombineList)
                    {
                        XYZ originPoint = (holeTask.Location as LocationPoint).Point;

                        XYZ downLeftPoint = originPoint +
                            (holeTask.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskWidth, holeTaskHeight)).AsDouble() /
                            2 * holeTask.HandOrientation.Negate()) +
                            (holeTask.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskHeight, holeTaskWidth)).AsDouble() /
                            2 * holeTask.FacingOrientation.Negate());
                        pointsList.Add(downLeftPoint);

                        XYZ downRightPoint = originPoint +
                            (holeTask.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskWidth, holeTaskHeight)).AsDouble() /
                            2 * holeTask.HandOrientation) +
                            (holeTask.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskHeight, holeTaskWidth)).AsDouble() /
                            2 * holeTask.FacingOrientation.Negate());
                        pointsList.Add(downRightPoint);

                        XYZ upLeftPoint = originPoint +
                            (holeTask.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskWidth, holeTaskHeight)).AsDouble() /
                            2 * holeTask.HandOrientation.Negate()) +
                            (holeTask.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskHeight, holeTaskWidth)).AsDouble() /
                            2 * holeTask.FacingOrientation);
                        pointsList.Add(upLeftPoint);

                        XYZ upRightPoint = originPoint +
                            (holeTask.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskWidth, holeTaskHeight)).AsDouble() /
                            2 * holeTask.HandOrientation) +
                            (holeTask.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskHeight, holeTaskWidth)).AsDouble() /
                            2 * holeTask.FacingOrientation);
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

                    XYZ pointFacingOrientation = intersectionFloorRectangularSolidIntersectCombineList
                        .First().FacingOrientation;
                    XYZ pointHandOrientation = intersectionFloorRectangularSolidIntersectCombineList
                        .First().HandOrientation;
                    Level pointLevel = doc.GetElement(intersectionFloorRectangularSolidIntersectCombineList
                        .First().LevelId) as Level;
                    double pointLevelElevation = pointLevel.Elevation;
                    foreach (XYZ p in pointsList)
                    {
                        XYZ vectorToPoint = (p - centroidIntersectionPoint).Normalize();
                        // Нижний левый угол
                        if (pointFacingOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2
                            && pointHandOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2)
                        {
                            combineDownLeftPointList.Add(p);
                        }

                        // Нижний правый угол
                        if (pointFacingOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2
                            && pointHandOrientation.AngleTo(vectorToPoint) <= Math.PI / 2)
                        {
                            combineDownRightPointList.Add(p);
                        }

                        // Верхний левый угол
                        if (pointFacingOrientation.AngleTo(vectorToPoint) <= Math.PI / 2
                            && pointHandOrientation.Negate().AngleTo(vectorToPoint) <= Math.PI / 2)
                        {
                            combineUpLeftPointList.Add(p);
                        }

                        // Верхний правый угол
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
                    FamilyInstance intersectionPoint = doc.Create.NewFamilyInstance(
                        newCenterPoint,
                        holeFamilySymbol,
                        pointLevel,
                        StructuralType.NonStructural);
                    intersectionPoint.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskWidth, holeTaskHeight))
                        .Set(intersectionPointWidth);
                    intersectionPoint.LookupParameter(this.ExchangeParameters(holeTaskName, holeTaskWidth, holeTaskHeight))
                        .Set(intersectionPointHeight);
                    intersectionPoint.LookupParameter(holeTaskThickness)
                        .Set(intersectionPointThickness);
                    //intersectionPoint.get_Parameter(heightOfBaseLevelGuid)
                    //    .Set(pointLevelElevation);
                    //intersectionPoint.get_Parameter(levelOffsetGuid)
                    //    .Set(intersectionPoint.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM)
                    //         .AsDouble() - (50 / 304.8));

                    double rotationAngle = pointFacingOrientation
                        .AngleTo(intersectionPoint.FacingOrientation) + Math.PI/2;
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

                    foreach (FamilyInstance forDel in intersectionFloorRectangularSolidIntersectCombineList)
                    {
                        doc.Delete(forDel.Id);
                        intersectionFloorRectangularCombineList.Remove(forDel);
                        //return intersectionFloorRectangularCombineList;
                    }
                }
                else
                {
                    intersectionFloorRectangularCombineList.Remove(intersectionFloorRectangularSolidIntersectCombineList[0]);
                    //return intersectionFloorRectangularCombineList;
                }
                //return intersectionFloorRectangularCombineList;
            }
            allFamilyInstances = intersectionWallRectangularCombineList
                 .Concat(intersectionFloorRectangularCombineList)
                 .ToList();
            return allFamilyInstances;
        }

        private string ExchangeParameters(string holeTaskName, string holeTaskHeight, string holeTaskWidth)
        {
            if (holeTaskName == "(Отв_Задание)_Стены_Прямоугольное)")
            {
                holeTaskHeight = holeTaskWidth;
                return holeTaskHeight;
            }
            else
            {
                return holeTaskHeight;
            }
        }
    }
}