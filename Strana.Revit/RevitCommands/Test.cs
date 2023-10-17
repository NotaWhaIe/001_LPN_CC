using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.IFC;
using System.Diagnostics;
using Autodesk.Revit.Creation;
//using Autodesk.DesignScript.Geometry;


namespace Strana.Revit.HoleTask.RevitCommands
{
    [Autodesk.Revit.Attributes.TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Test : IExternalCommand
    {
        static AddInId addinId = new AddInId(new Guid("f64706dd-e8f6-4cbe-9cc6-a2910be5ad5a"));
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uiDoc.Document;

            XYZ startPoint = new XYZ(0, 0, 0); // Начальная точка (координаты XYZ)
            XYZ endPoint = new XYZ(0, 0, 10); // Конечная точка (координаты XYZ)

            // Создаем новую ModelLine
            using (Transaction transaction = new Transaction(doc, "Create Model Line"))
            {
                transaction.Start();

                // Создаем ModelCurve, указывая тип линии (в данном случае, прямую линию) и координаты точек
                ModelCurve modelCurve = doc.Create.NewModelCurve(
                    Line.CreateBound(
                        startPoint, endPoint),
                    SketchPlane.Create(
                        doc, Plane.CreateByOriginAndBasis(
                        startPoint, XYZ.BasisX, XYZ.BasisZ)));

                transaction.Commit();
            }

            TaskDialog.Show("Done!", "Result.Succeeded");
            return Result.Succeeded;
        }
    }
}