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
        public static void AddFamilyInstancesToList(Document doc, string familyName1, List<FamilyInstance> list1, string familyName2, List<FamilyInstance> list2)
        {
            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>(); // Удаление .ToElements().ToList() для улучшения производительности

            foreach (FamilyInstance fi in collector)
            {
                // Проверяем, является ли семейство родительским (нет вышестоящего компонента)
                if (fi.SuperComponent == null)
                {
                    // Проверяем, имеет ли семейство вложенные компоненты
                    var subComponentIds = fi.GetSubComponentIds();
                    if (!subComponentIds.Any())
                    {
                        // Проверяем, соответствует ли имя семейства первому заданному имени
                        if (fi.Symbol.FamilyName == familyName1)
                        {
                            list1.Add(fi);
                        }
                        // Проверяем, соответствует ли имя семейства второму заданному имени
                        else if (fi.Symbol.FamilyName == familyName2)
                        {
                            list2.Add(fi);
                        }
                    }
                }
            }
        }

        public static void AddFamilyInstancesToList(Document doc, string familyName, List<FamilyInstance> list, string parameterName, string parameterValue)
        {
            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .ToElements().ToList();

            foreach (FamilyInstance fi in collector)
            {
                if (fi.Name == familyName && fi.SuperComponent == null)
                {
                    Parameter param = fi.LookupParameter(parameterName);
                    if (param != null && param.StorageType == StorageType.String && param.AsString() == parameterValue)
                    {
                        var subComponentIds = fi.GetSubComponentIds();
                        if (!subComponentIds.Any())
                        {
                            list.Add(fi);
                        }
                    }
                }
            }
        }

    }

}
