// <copyright file="HoleTaskSolidGetter.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Strana.Revit.HoleTask.Utils;

namespace Strana.Revit.HoleTask.Extensions
{
    /// <summary>
    /// This class contains metod set up a HoleTasks familySybol.
    /// </summary>
    public static class HoleTaskSolidGetter
    {
        public static double delta => Confing.Default.offSetJoin / 304.8;
        public static bool areJoin => Confing.Default.areJoin;
        public static Solid GetHoleTaskSolidWithDelta(this FamilyInstance holeTaskItem)
        {
            Document doc = holeTaskItem.Document;
            Options options = new Options();
            Solid largestSolid = null;
            GeometryElement geometryElement = holeTaskItem.get_Geometry(options);
            DirectShape directShape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Furniture));

            foreach (GeometryObject geometry in geometryElement)
            {
                GeometryInstance instance = geometry as GeometryInstance;
                if (instance != null)
                {
                    GeometryElement instanceGeometryElement = instance.GetInstanceGeometry();
                    foreach (GeometryObject o in instanceGeometryElement)
                    {
                        Solid solid = o as Solid;
                        if (solid != null && solid.Volume != 0)
                        {
                            largestSolid = solid;
                            break;
                        }
                    }
                }
            }

            if (delta == 0 || !areJoin)
            {
                // Create solid in the model by DirectShape
                //directShape.SetShape(new GeometryObject[] { largestSolid });
                return largestSolid;
            }

            CurveLoop GetCurveLoopFromSolid(Solid largestSolid)
            {
                FaceArray faces = largestSolid.Faces;
                foreach (Face face in faces)
                {
                    if (face is PlanarFace planarFace && planarFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ))
                    {
                        CurveLoop curveLoop = planarFace.GetEdgesAsCurveLoops().FirstOrDefault();

                        if (curveLoop != null)
                        {
                            return curveLoop;
                        }
                    }
                }

                return null;
            }
            CurveLoop curveLoopSolid = GetCurveLoopFromSolid(largestSolid);

            // Get sweep's profile
            List<double> DoubleListOffset(CurveLoop curveLoopSolid, double delta)
            {
                List<double> list = new List<double>();
                foreach (Curve curve in curveLoopSolid)
                {
                    list.Add(delta);
                }

                return list;
            }
            List<double> doubleOffsetСontour = DoubleListOffset(curveLoopSolid, delta);
            CurveLoop curveLoopOffset = CurveLoop.CreateViaOffset(curveLoopSolid, doubleOffsetСontour, XYZ.BasisZ);
            List<CurveLoop> curveLoopOffsetСontour = [curveLoopOffset];

            // Get sweep's path
            CurveLoop GetSweepPathOfSolid(Solid largestSolid)
            {
                EdgeArray sweeps = largestSolid.Edges;
                foreach (Edge edge in sweeps)
                {
                    Curve curve = edge.AsCurve();
                    XYZ point0 = curve.GetEndPoint(0);
                    XYZ point1 = curve.GetEndPoint(1);
                    double tolerance = 0.1; // Tolerance=0.1 == 1 degree, tolerance=0.2 == 3 degree, tolerance=0.3 == 4 degree.
                    if ((point0.Z - point1.Z) > tolerance)
                    {
                        curve = AddOffsetToSweepPath(curve, delta);
                        CurveLoop curveLoop = new CurveLoop();
                        curveLoop.Append(curve);
                        return curveLoop;
                    }
                }
                return null;
            }
            Curve AddOffsetToSweepPath(Curve sweepPath, double delta)
            {
                if (sweepPath is Line line)
                {
                    double currentLength = line.Length;
                    double newLength = currentLength + 2 * delta;

                    XYZ direction = line.Direction.Normalize();
                    XYZ newEndPoint = line.GetEndPoint(0) + newLength * direction;

                    Line newLine = Line.CreateBound(line.GetEndPoint(0), newEndPoint);
                    Curve curve = newLine as Curve;
                    return curve;
                }
                return null;
            }
            CurveLoop sweepPath = GetSweepPathOfSolid(largestSolid);

            // Create geomentry by sweep
            Solid solidWithDelta = GeometryCreationUtilities.CreateSweptGeometry(sweepPath, 0, 0, curveLoopOffsetСontour);

            // Move solid on the Z axis to the delta
            XYZ translationVector = new XYZ(0, 0, delta);
            Transform translationTransform = Transform.CreateTranslation(translationVector);
            Solid movedSolid = SolidUtils.CreateTransformed(solidWithDelta, translationTransform);

            return movedSolid;
        }
    }
}
