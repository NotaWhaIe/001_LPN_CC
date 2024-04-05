// <copyright file="MepElementCollections.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Strana.Revit.HoleTask.Utils;
using Autodesk.Revit.Attributes;

namespace Strana.Revit.HoleTask.ElementCollections
{
    /// <summary> Class contains all mep elements in revit model. </summary>
    public static class WallFloorLinkElementCollections
    {
        /// <summary> Gets all cable trays to check intersecting + fastFilterBB. </summary>
        /// <param name="mepElement"> This is a duct, cable tray or pipe.</param>
        /// <param name="doc"><seealso cref="Document"/></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IEnumerable<Element> AllElementsByMepBBox(this Element mepElement, RevitLinkInstance linkInstance)
        {
            Document doc = linkInstance.Document;
            Document linkDoc = linkInstance.GetLinkDocument();
            Transform linkTransform = linkInstance.GetTransform();

            BoundingBoxXYZ mepBoundingBox = mepElement.get_BoundingBox(null);
            if (mepBoundingBox == null)
            {
                return Enumerable.Empty<Element>();
            }
            if (linkTransform != null)
            {
                mepBoundingBox.Transform = linkTransform;
            }
            Outline mepOutline = new Outline(mepBoundingBox.Min, mepBoundingBox.Max);
            BoundingBoxIntersectsFilter mepFilter = new BoundingBoxIntersectsFilter(mepOutline);

            IEnumerable<Element> walls = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .WherePasses(mepFilter)
                .Cast<Wall>()
                .Where(w => w.WallType.Kind != WallKind.Curtain)
                .Cast<Element>();

            IEnumerable<Element> floors = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_Floors)
                .OfClass(typeof(Floor))
                .WhereElementIsNotElementType()
                .WherePasses(mepFilter)
                .Cast<Element>();

            ///test
            var firstFloor = floors.FirstOrDefault();
            if (firstFloor != null)
            {
                BoundingBoxXYZ boundingBox = firstFloor.get_BoundingBox(null);
                if (boundingBox != null)
                {
                    XYZ minPoint = boundingBox.Min;
                    XYZ maxPoint = boundingBox.Max;
                    SphereByPoint.CreateSphereByPoint(minPoint, doc, "firstFloor");
                    SphereByPoint.CreateSphereByPoint(maxPoint, doc, "firstFloor");

                }
            }

            return walls.Concat(floors);
        }
            //SphereByPoint.CreateSphereByPoint(transformedMin, doc, "mepElement");
            //SphereByPoint.CreateSphereByPoint(transformedMax, doc, "mepElement");

            //Outline mepOutline = new Outline(mepBoundingBox.Min, mepBoundingBox.Max);
        public static XYZ TransformPoint(XYZ point, Transform transform)
        {
            // Получаем базовые векторы трансформации
            XYZ bX = transform.BasisX;
            XYZ bY = transform.BasisY;
            XYZ bZ = transform.BasisZ;
            XYZ origin = transform.Origin;

            // Создаем матрицу 3x4
            double[,] matrix3x4 = new double[,]
            {
        { bX.X, bX.Y, bX.Z, origin.X },
        { bY.X, bY.Y, bY.Z, origin.Y },
        { bZ.X, bZ.Y, bZ.Z, origin.Z }
            };

            // Создаем матрицу 4x1
            double[,] matrix4x1 = new double[,]
            {
        { point.X },
        { point.Y },
        { point.Z },
        { 1 } // Элемент гомогенной координаты для трансформации
            };

            // Создаем матрицу-результат размером 3x1
            double[,] resultMatrix = new double[3, 1];

            // Выполняем умножение матриц
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 1; j++)
                {
                    resultMatrix[i, j] = 0;
                    for (int k = 0; k < 4; k++) // Матрица 3x4 требует k < 4
                    {
                        resultMatrix[i, j] += matrix3x4[i, k] * matrix4x1[k, j];
                    }
                }
            }

            // Получаем новые координаты элемента
            double Xtemp = resultMatrix[0, 0];
            double Ytemp = resultMatrix[1, 0];
            double Ztemp = resultMatrix[2, 0];

            return new XYZ(Xtemp, Ytemp, Ztemp);
        }

        public static Outline TransformOutline(Outline originalOutline, Transform transform)
        {
            // Трансформируем минимальную и максимальную точки исходного Outline
            XYZ transformedMin = transform.OfPoint(originalOutline.MinimumPoint);
            XYZ transformedMax = transform.OfPoint(originalOutline.MaximumPoint);

            // Создаем новый Outline с трансформированными точками
            Outline transformedOutline = new Outline(transformedMin, transformedMax);

            return transformedOutline;
        }

    }
}

