// <copyright file="SolidGetter.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;

namespace Strana.Revit.HoleTask.Extension.RevitElement
{
    /// <summary>
    /// This extension for geting element solids.
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
        /// <param name="element">This is fooor or wall.</param>
        /// <returns><seealso cref="Solid"/></returns>
        /// <remarks>now return solid with holes.</remarks>
        public static Solid GetSolidWithoutHoles(this Element element)
        {
            Solid solidWithHoles = element.GetSolidWithHoles();
            Face solidFacade = GetSolidMainFace(solidWithHoles);
            CurveLoop outerConture = MainOuterContourFromFace(solidFacade); // внешний контур
            List<CurveLoop> outerLoops = new ()
            {
                outerConture,
            };

            CurveLoop sweepPath = GetSweepPath(solidWithHoles); // траектория выдавливания
            double pathAttachmentParam = sweepPath.First().GetEndParameter(0);

            Solid solidWithoutHoles = GeometryCreationUtilities
                .CreateSweptGeometry(
                    sweepPath,
                    0,
                    pathAttachmentParam,
                    outerLoops);

            return solidWithoutHoles;
        }

        private static Solid GetSolidWithHoles(this Element element)
        {
            GeometryElement geometryElement = element.get_Geometry(Opt);
            Solid elementSolid = null;

            foreach (GeometryObject geometry in geometryElement)
            {
                elementSolid = geometry as Solid;
            }

            return elementSolid;
        }

        /// <summary>
        /// return element facade face.
        /// </summary>
        /// <param name="solid"><seealso cref="Solid"/></param>
        /// <returns>facade face. <seealso cref="Face"/></returns>
        private static Face GetSolidMainFace(Solid solid)
        {
            Face faceMaxSquare = null;
            var faces = solid.Faces;
            foreach (Face solidface in faces)
            {
                if (faceMaxSquare == null || faceMaxSquare.Area < solidface.Area)
                {
                    faceMaxSquare = solidface;
                }
            }

            return faceMaxSquare;
        }

        private static CurveLoop MainOuterContourFromFace(Face faceWithHoles)
        {
            EdgeArrayArray allFaceEdges = faceWithHoles.EdgeLoops;

            List<CurveLoop> curentUnitedCurveLoopList = new List<CurveLoop>();
            CurveLoop curentUnitedCurveLoop = null;

            foreach (EdgeArray agesOfOneFace in allFaceEdges)
            {
                List<Curve> unitedCurve = new List<Curve>();

                foreach (Edge item in agesOfOneFace)
                {
                    var curve = item.AsCurve();
                    unitedCurve.Add(curve);
                }

                List<CurveLoop> curveLoopList = new List<CurveLoop>();
                CurveLoop curvesLoop = CurveLoop.Create(unitedCurve);
                curveLoopList.Add(curvesLoop);
                if (curentUnitedCurveLoop == null || ExporterIFCUtils
                    .ComputeAreaOfCurveLoops(curveLoopList) > ExporterIFCUtils
                    .ComputeAreaOfCurveLoops(curentUnitedCurveLoopList))
                {
                    curentUnitedCurveLoop = curvesLoop;
                    curentUnitedCurveLoopList = new List<CurveLoop>();
                    curentUnitedCurveLoopList.Add(curvesLoop);
                }
            }

            return curentUnitedCurveLoop;
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

            List<Curve> curve = new ()
            {
                edgeMin.AsCurve(),
            };
            CurveLoop edgeMinCurvesLoop = CurveLoop.Create(curve);
            return edgeMinCurvesLoop;
        }
    }
}
