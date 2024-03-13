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
        public static void AddFamilyInstancesToList(Document doc, string familyName, List<FamilyInstance> list, string parameterName, string parameterValue)
        {
            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .ToElements().OfType<FamilyInstance>(); // Используем OfType для немедленного приведения типа

            foreach (FamilyInstance fi in collector)
            {
                // Проверяем, соответствует ли имя семейства и является ли семейство родительским
                if (fi.Symbol.FamilyName == familyName && fi.SuperComponent == null) // Используйте Symbol.FamilyName для проверки имени семейства
                {
                    // Проверяем наличие и значение параметра
                    Parameter param = fi.LookupParameter(parameterName);
                    if (param != null && param.AsString() == parameterValue)
                    {
                        // Проверяем, имеет ли семейство вложенные компоненты
                        var subComponentIds = fi.GetSubComponentIds();
                        if (!subComponentIds.Any())
                        {
                            // Если у семейства нет вложенных компонентов и параметр соответствует ожидаемому, добавляем его в список
                            list.Add(fi);
                        }
                    }
                }
            }
        }

    }
}
