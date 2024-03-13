using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace Strana.Revit.HoleTask.ElementCollections
{
    public static class MepElementSelector
    {
        public static UIDocument UIDocument { get; set; }

        public static IEnumerable<Element> GetSelectedOrAllMepElements()
        {
            Document doc = UIDocument.Document;
            ICollection<ElementId> selectedIds = UIDocument.Selection.GetElementIds();

            if (selectedIds.Count > 0)
            {
                return selectedIds.Select(id => doc.GetElement(id))
                                  .Where(elem => elem is Duct || elem is Pipe || elem is CableTray)
                                  .ToList();
            }

            return new FilteredElementCollector(doc)
                   .WherePasses(new ElementMulticlassFilter(new List<System.Type> { typeof(Duct), typeof(Pipe), typeof(CableTray) }))
                   .WhereElementIsNotElementType()
                   .ToElements();
        }
    }
}
