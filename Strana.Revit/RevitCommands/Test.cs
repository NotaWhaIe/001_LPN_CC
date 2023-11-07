using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
//using Autodesk.DesignScript.Geometry;


namespace Strana.Revit.HoleTask.RevitCommands
{
    [Autodesk.Revit.Attributes.TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Test : IExternalCommand
    {
        static AddInId addinId = new AddInId(new Guid("f64706dd-e8f6-4cbe-9cc6-a2910be5ad5a"));
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

            TaskDialog.Show(":)", largestSolid.Volume.ToString());
            return Result.Succeeded;
        }

    }
}