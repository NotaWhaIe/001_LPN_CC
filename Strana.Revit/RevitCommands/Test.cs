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
using System.Net;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Visual;
using static System.Net.Mime.MediaTypeNames;
using System.Data.Common;
using System.Xml.Linq;
//using Autodesk.DesignScript.Geometry;


namespace Strana.Revit.HoleTask.RevitCommands
{
    [Autodesk.Revit.Attributes.TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Test : IExternalCommand
    {
        static AddInId addinId = new AddInId(new Guid("f64706dd-e8f6-4cbe-9cc6-a2910be5ad5a"));
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Получить активный документ Revit
            Document doc = commandData.Application.ActiveUIDocument.Document;

            // Получить все экземпляры RevitLinkInstance в документе
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<RevitLinkInstance> linkInstances = collector.OfClass(typeof(RevitLinkInstance)).ToElements().Cast<RevitLinkInstance>().ToList();

            // Пройтись по всем экземплярам RevitLinkInstance
            foreach (RevitLinkInstance linkInstance in linkInstances)
            {
                Transform transform = linkInstance.GetTotalTransform();
                XYZ xAxis = transform.BasisX;

                double angle = Math.Atan2(xAxis.Y, xAxis.X);

                // Преобразовать угол из радиан в градусы
                double angleInDegrees = angle * 180 / Math.PI;

                // Теперь у вас есть угол поворота в градусах для данного экземпляра RevitLinkInstance
                TaskDialog.Show("Rotation", "Угол поворота для RevitLinkInstance: " + angleInDegrees.ToString());
            }

            return Result.Succeeded;
        }

    }
}