using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Strana.Revit.HoleTask.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
//using Autodesk.DesignScript.Geometry;

namespace Strana.Revit.HoleTask.RevitCommands
{
    [Autodesk.Revit.Attributes.TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Test : IExternalCommand
    {
        static AddInId addinId = new AddInId(new Guid("f64706dd-e8f6-4cbe-9cc6-a2910be5ad5a"));
        private object offsetCurves;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            ElementId selFiId = null;
            Selection sel = uidoc.Selection;
            if (sel.GetElementIds().Count() != 1)
            {
                Reference selRef = sel.PickObject(ObjectType.Element, "Выберите семейство");
                selFiId = selRef.ElementId;
            }
            else
            {
                selFiId = sel.GetElementIds().First();
            }

            FamilyInstance selFi = doc.GetElement(selFiId) as FamilyInstance;
            if (selFi == null)
            {
                TaskDialog.Show("Ошибка!", "Выбрано не семейство");
                return Result.Succeeded;
            }

            /// FamilyInstance - GeometryElement - GeometryInstance - Solid
            Options options = new Options();
            GeometryElement geometryElement = selFi.get_Geometry(options);

            Solid largestSolid = null;

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

            if (largestSolid == null)
            {
                TaskDialog.Show(":(", "Solid == null");
            }

            // Test Создать солид в модели используя DirectShape на месте имеющегося солид
            using (Transaction t = new(doc, "create Solid"))
            {
                t.Start();

                DirectShape directShape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Furniture));
                directShape.SetShape(new GeometryObject[] { largestSolid });

                t.Commit();
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

            double offsetDistance = 50.0 / 304.8;

            // Получить контур сдвига
            List<double> listOffset(CurveLoop curveLoopSolid, double offsetDistance)
            {
                List<double> list = new List<double>();
                foreach (Curve curve in curveLoopSolid)
                {
                    list.Add(offsetDistance);
                }

                return list;
            }

            // Получить траекторию сдвига
            Curve AddOffsetToSweepPath(Curve sweepPath, double offsetDistance)
            {
                if (sweepPath is Line line)
                {
                    double currentLength = line.Length;
                    double newLength = currentLength + 2 * offsetDistance;

                    XYZ direction = line.Direction.Normalize();
                    XYZ newEndPoint = line.GetEndPoint(0) + direction * newLength;

                    Line newLine = Line.CreateBound(line.GetEndPoint(0), newEndPoint);
                    Curve curve = newLine as Curve;
                    return curve;
                }
                return null;
            }
            Curve GetSweepPathOfSolid(Solid largestSolid)
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
                        curve = AddOffsetToSweepPath(curve, offsetDistance);
                        return curve;
                    }
                }
                return null;
            }
            Curve sweepPath = GetSweepPathOfSolid(largestSolid);

            //Curve/*Loop */sweepPath = GetSweepPathOfSolid(Solid largestSolid);

            // Получить сдвиг

            //Solid solidWithoutHoles = GeometryCreationUtilities
            //.CreateSweptGeometry(sweepPath, 0, 0, outerLoops);

            // Отрисовать в моделе солид



            //CurveLoop curveLoopOffset = CurveLoop.CreateViaOffset(curveLoopSolid, listOffset(curveLoopSolid), XYZ.BasisZ);

            TaskDialog.Show(":)", (sweepPath.Length * 304.8).ToString());
            return Result.Succeeded;
        }
    }
}

