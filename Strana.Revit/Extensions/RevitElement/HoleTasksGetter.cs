using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strana.Revit.HoleTask.Extensions.RevitElement
{
    public static class HoleTasksGetter
    {
        public static void AddFamilyInstancesToList(Document doc, string familyName, List<FamilyInstance> list)
        {
            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .Where(x => x.Name == familyName)
                .Cast<FamilyInstance>();

            if (collector.Any())
            {
                foreach (FamilyInstance fi in collector)
                {
                    list.Add(fi);
                }
            }
        }

    }
}
