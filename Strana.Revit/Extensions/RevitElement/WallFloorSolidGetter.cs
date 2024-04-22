// <copyright file="SolidGetter.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.UI;
using Strana.Revit.HoleTask.Extensions;
using Strana.Revit.HoleTask.RevitCommands;
using Strana.Revit.HoleTask.Utils;

namespace Strana.Revit.HoleTask.Extension.RevitElement
{
    /// <summary>
    /// This extension for getting element solids.
    /// </summary>
    public static class WallFloorSolidGetter
    {
        private static readonly Options Opt = new()
        {
            ComputeReferences = true,
            DetailLevel = ViewDetailLevel.Fine,
        };
        private static bool arePlaceHoleTaskInOpenings => Confing.Default.arePlaceHoleTaskInOpenings;

        /// <summary>
        /// Get from wall and floor solid without holes.
        /// </summary>
        /// <param name="element">This is floor or wall.</param>
        /// <param name="revitLink">Transform solid by the given revit link instance.</param>
        /// <returns><seealso cref="Solid"/></returns>
        /// <remarks>now return solid with holes.</remarks>
        public static Solid GetSolidWithoutHoles(this Element element, RevitLinkInstance revitLink)
        {
            Transform transform = revitLink.GetTotalTransform();
            Solid solidWithHoles = element.GetSolidWithHoles();
            if (!arePlaceHoleTaskInOpenings)
            {
                return SolidUtils.CreateTransformed(solidWithHoles, transform);
            }

            try
            {
                Face solidFacade = GetSolidMainFace(solidWithHoles);
                if (solidFacade == null)
                {
                    return null;
                }

                XYZ solidFacadeNormal = solidFacade.ComputeNormal(new UV(0, 0));
                CurveLoop outerContour = MainOuterContourFromFace(solidFacade);
                if (outerContour == null)
                {
                    return null;
                }

                List<CurveLoop> outerLoops = new List<CurveLoop> { outerContour };

                double sweepPathLenght = element.GetInterctedElementThickness();
                XYZ startPoint = outerContour.GetCurveLoopIterator().Current.GetEndPoint(0);
                XYZ endPoint = startPoint - sweepPathLenght * solidFacadeNormal.Normalize();

                Curve sweepPath = Line.CreateBound(startPoint, endPoint);
                CurveLoop sweepPathLoop = CurveLoop.Create(new List<Curve> { sweepPath });

                Solid solidWithoutHoles = GeometryCreationUtilities.CreateSweptGeometry(sweepPathLoop, 0, 0, outerLoops);
                return SolidUtils.CreateTransformed(solidWithoutHoles, transform);
            }
            catch
            {
                return SolidUtils.CreateTransformed(solidWithHoles, transform);
            }
        }

        public static Solid GetSolidWithHoles(this Element element)
        {
            GeometryElement geometryElement = element.get_Geometry(Opt);
            Solid largestSolid = null;
            double largestVolume = 0.0;

            foreach (GeometryObject geometry in geometryElement)
            {
                if (geometry is Solid solid && solid.Volume > 0 && solid.Volume > largestVolume)
                {
                    largestSolid = solid;
                    largestVolume = solid.Volume;
                }
            }

            return largestSolid;
        }
        /// <summary>
        /// return element façade face.
        /// </summary>
        /// <param name="solid"><seealso cref="Solid"/></param>
        /// <returns>façade face. <seealso cref="Face"/></returns>
        private static Face GetSolidMainFace(Solid solid)
        {
            // Проверяем, не равен ли solid null
            if (solid == null || solid.Faces == null || solid.Faces.Size == 0)
            {
                return null;
            }

            Face faceMaxSquare = null;

            foreach (Face solidFace in solid.Faces)
            {
                // Условие на null для faceMaxSquare не требуется, так как оно проверяется в условии ниже
                if (faceMaxSquare == null || faceMaxSquare.Area < solidFace.Area)
                {
                    faceMaxSquare = solidFace;
                }
            }

            return faceMaxSquare;
        }

        private static CurveLoop MainOuterContourFromFace(Face faceWithHoles)
        {
            EdgeArrayArray allFaceEdges = faceWithHoles.EdgeLoops;

            List<CurveLoop> currentUnitedCurveLoopList = new();
            CurveLoop currentUnitedCurveLoop = null;

            foreach (EdgeArray agesOfOneFace in allFaceEdges)
            {
                List<Curve> unitedCurve = new List<Curve>();

                foreach (Edge item in agesOfOneFace)
                {
                    var curve = item.AsCurve();
                    unitedCurve.Add(curve);
                }

                List<CurveLoop> curveLoopList = new();
                CurveLoop curvesLoop = CurveLoop.Create(unitedCurve);
                curveLoopList.Add(curvesLoop);
                if (currentUnitedCurveLoop == null || ExporterIFCUtils
                    .ComputeAreaOfCurveLoops(curveLoopList) > ExporterIFCUtils
                    .ComputeAreaOfCurveLoops(currentUnitedCurveLoopList))
                {
                    currentUnitedCurveLoop = curvesLoop;
                    currentUnitedCurveLoopList = [curvesLoop];
                }
            }

            return currentUnitedCurveLoop;
        }
    }
}
