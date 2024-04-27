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
        public static void FilterFamilyInstancesToList(IEnumerable<FamilyInstance> collector, string familyName,
            List<FamilyInstance> list, string parameterName, string parameterValue)
        {
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
        public class CollectFamilyInstances
        {
            private static CollectFamilyInstances _instance;
            private readonly List<FamilyInstance> _list1 = new List<FamilyInstance>();
            private readonly List<FamilyInstance> _list2 = new List<FamilyInstance>();
            private readonly List<Level> _list3 = new List<Level>();

            public static CollectFamilyInstances Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new CollectFamilyInstances();
                    }
                    return _instance;
                }
            }

            public IReadOnlyList<FamilyInstance> FamilyInstance1 => _list1;
            public IReadOnlyList<FamilyInstance> FamilyInstance2 => _list2;
            public IReadOnlyList<Level> Level => _list3;

            private CollectFamilyInstances() { }

            public void AddToListFamilyInstances(Document doc, string familyName1, string familyName2)
            {
                var collector = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>();

                foreach (FamilyInstance fi in collector)
                {
                    if (fi.SuperComponent == null && !fi.GetSubComponentIds().Any())
                    {
                        if (fi.Symbol.FamilyName == familyName1)
                        {
                            _list1.Add(fi);
                        }
                        else if (fi.Symbol.FamilyName == familyName2)
                        {
                            _list2.Add(fi);
                        }
                    }
                }
            }
            
            public void AddToListLevels(Document doc)
            {
                var levels = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Levels)
                     .WhereElementIsNotElementType()
                     .Cast<Level>()
                     .ToList();

                _list3.AddRange(levels);
            }
            public void ClearDataFamilyInstance()
            {
                _list1.Clear();
                _list2.Clear();
            }

            public void ClearDataLevel()
            {
                _list3.Clear();
            }
        }
    }
}
