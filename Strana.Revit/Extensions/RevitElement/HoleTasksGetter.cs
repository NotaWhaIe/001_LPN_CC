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
        //public static void AddFamilyInstancesToList(Document doc, string familyName, List<FamilyInstance> list)
        //{
        //    var collector = new FilteredElementCollector(doc)
        //        .OfClass(typeof(FamilyInstance))
        //        .OfCategory(BuiltInCategory.OST_GenericModel)
        //        .Where(x => x.Name == familyName)
        //        .Cast<FamilyInstance>();

        //    if (collector.Any())
        //    {
        //        foreach (FamilyInstance fi in collector)
        //        {
        //            list.Add(fi);
        //        }
        //    }
        //}

        public static void AddFamilyInstancesToList(Document doc, string familyName, List<FamilyInstance> list)
        {
            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .ToElements().ToList();

            foreach (FamilyInstance fi in collector)
            {
                // Проверяем, соответствует ли имя семейства и является ли семейство родительским
                if (fi.Name == familyName && fi.SuperComponent == null)
                {
                    // Проверяем, имеет ли семейство вложенные компоненты
                    var subComponentIds = fi.GetSubComponentIds();
                    if (!subComponentIds.Any())
                    {
                        // Если у семейства нет вложенных компонентов, добавляем его в список
                        list.Add(fi);
                    }
                }
            }
        }
    }
}
