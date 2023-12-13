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
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Autodesk.Revit.DB.Document doc = uidoc.Document;
                var reference = uidoc.Selection.PickObject(ObjectType.Element, "Select a wall");
                var point1 = uidoc.Selection.PickPoint("Select first point");
                var point2 = uidoc.Selection.PickPoint("Select second point");

                Line line = Line.CreateBound(point1, new XYZ(point2.X, point1.Y, point2.Z));

                var wall = doc.GetElement(reference) as Wall;
                double thickness = wall.Width;
                var array = new ReferenceArray();
                var options = new Options()
                {
                    View = doc.ActiveView,
                    ComputeReferences = true
                };
                var geom = wall.get_Geometry(options);
                foreach (GeometryObject go in geom)
                {
                    if (go is Solid)
                    {
                        var solid = go as Solid;
                        foreach (Edge edge in solid.Edges)
                        {
                            if (Math.Abs(thickness - edge.ApproximateLength) < 0.00001 &&
                                //edge.Reference.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_CUT_EDGE) 
                                edge.Reference.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_LINEAR)
                            {
                                array.Insert(edge.Reference, array.Size);
                            }
                        }
                    }
                }

                using (Transaction t = new Transaction(doc, "Dimension creation"))
                {
                    t.Start();
                    doc.Create.NewDimension(doc.ActiveView, line, array);
                    t.Commit();
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("1", e.Message + e.StackTrace);
            }

            return Result.Succeeded;

        }
    }
}