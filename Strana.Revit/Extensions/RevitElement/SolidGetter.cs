// <copyright file="SolidGetter.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;

namespace Strana.Revit.HoleTask.Extension.RevitElement
{
    /// <summary>
    /// This extension for getting element solids.
    /// </summary>
    public static class SolidGetter
    {
        private static readonly Options Opt = new ()
        {
            ComputeReferences = true,
            DetailLevel = ViewDetailLevel.Fine,
        };

        /// <summary>
        /// Get from wall and floor solid without holes.
        /// </summary>
        /// <param name="element">This is floor or wall.</param>
        /// <param name="revitLink">Transform solid by the given revit link instance.</param>
        /// <returns><seealso cref="Solid"/></returns>
        /// <remarks>now return solid with holes.</remarks>
        public static Solid GetSolidWithoutHoles(this Element element, RevitLinkInstance revitLink)
        {
            try
            {
                Transform transform = revitLink.GetTotalTransform();
                Solid solidWithHoles = element.GetSolidWithHoles();
                Face solidFacade = GetSolidMainFace(solidWithHoles);

                CurveLoop outerСontour = MainOuterContourFromFace(solidFacade); // внешний контур
                List<CurveLoop> outerLoops = [outerСontour];

                CurveLoop sweepPath = GetSweepPath(solidWithHoles); // траектория выдавливания

                Solid solidWithoutHoles = GeometryCreationUtilities
                    .CreateSweptGeometry(sweepPath, 0, 0, outerLoops);

                return SolidUtils.CreateTransformed(solidWithoutHoles, transform);
            }
            catch
            {
                Transform transform = revitLink.GetTotalTransform();
                Solid solidWithHoles = element.GetSolidWithHoles();
                return SolidUtils.CreateTransformed(solidWithHoles, transform);
            }
        }

        /// <summary>
        /// bool give information are need see collision by this element.
        /// </summary>
        /// <param name="element">floor or wall.</param>
        /// <returns> are need create hole task by current element.</returns>
        public static bool AreElementsHaveFaces(this Element element)
        {
            Solid solid = element.GetSolidWithHoles();
            if (solid == null)
            {
                return false;
            }

            foreach (var face in solid.Faces)
            {
                return true;
            }

            return false;
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
            Face faceMaxSquare = null;
            var faces = solid.Faces;
            foreach (Face solidFace in faces)
            {
                if (faceMaxSquare == null || faceMaxSquare.Area < solidFace.Area)
                {
                    faceMaxSquare = solidFace;
                }
            }

            if (faceMaxSquare == null)
            {
                foreach (Face solidFace in faces)
                {
                    if (faceMaxSquare == null || faceMaxSquare.Area < solidFace.Area)
                    {
                        faceMaxSquare = solidFace;
                    }
                }

            }

            return faceMaxSquare;
        }

        private static CurveLoop MainOuterContourFromFace(Face faceWithHoles)
        {
            EdgeArrayArray allFaceEdges = faceWithHoles.EdgeLoops;

            List<CurveLoop> currentUnitedCurveLoopList = new ();
            CurveLoop currentUnitedCurveLoop = null;

            foreach (EdgeArray agesOfOneFace in allFaceEdges)
            {
                List<Curve> unitedCurve = new List<Curve>();

                foreach (Edge item in agesOfOneFace)
                {
                    var curve = item.AsCurve();
                    unitedCurve.Add(curve);
                }

                List<CurveLoop> curveLoopList = new ();
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

        private static CurveLoop GetSweepPath(Solid solid)
        {
            Edge edgeMin = null;
            var edges = solid.Edges;
            foreach (Edge solidEdge in edges)
            {
                if (edgeMin == null || edgeMin.AsCurve().Length > solidEdge.AsCurve().Length)
                {
                    edgeMin = solidEdge;
                }
            }

            List<Curve> curve = [edgeMin?.AsCurve() ?? null];
            CurveLoop edgeMinCurvesLoop = CurveLoop.Create(curve);
            return edgeMinCurvesLoop;
        }
    }
}
