using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Strana.Revit.ViewModelTest;
/// Work
namespace Strana.Revit.HoleTask.RevitCommands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Test : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            List<RevitLinkInstance> revitLinkInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Where(x => x.GetLinkDocument() != null)
                .ToList();

            // Открыть WPF окно для выбора связей
            Window1 mainWindow = new Window1(revitLinkInstances);
            bool? dialogResult = mainWindow.ShowDialog();

            if (dialogResult != true || mainWindow.SelectedLinks == null || !mainWindow.SelectedLinks.Any())
            {
                TaskDialog.Show("Ошибка", "Не выбраны связи.");
                return Result.Failed;
            }

            // Данные в WPF с выбранными связями
            List<DisplayFamilyInstance> listFamilyInstance2a = new List<DisplayFamilyInstance>();
            List<DisplayFamilyInstance> listFamilyInstance2b = new List<DisplayFamilyInstance>();
            List<DisplayFamilySymbol> listFamilySymbol3a = new List<DisplayFamilySymbol>();
            List<DisplayFamilySymbol> listFamilySymbol3b = new List<DisplayFamilySymbol>();

            List<RevitLinkInstance> selectedLinks = mainWindow.SelectedLinks;
            RevitLinkInstance linkInstance = selectedLinks.FirstOrDefault();

            Document linkedDoc = linkInstance.GetLinkDocument();
            if (linkedDoc != null)
                {
                    listFamilyInstance2a.AddRange(new FilteredElementCollector(linkedDoc)
                        .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                        .OfClass(typeof(FamilyInstance))
                        .Cast<FamilyInstance>()
                        .Where(fi => fi.Location != null && (fi.Location is LocationPoint || fi.Location is LocationCurve)) // Проверка, что семейство размещено в пространстве проекта
                        .Where(fi => fi.Document.Equals(linkedDoc)) // Проверка, что семейство размещено в связанном файле
                        .Select(fi => new DisplayFamilyInstance
                        {
                            FamilyName = fi.Symbol.Family?.Name,
                            TypeName = fi.Symbol.Name,
                            Instance = fi
                        })
                        .GroupBy(fi => fi.TypeName)  // Группировка по имени типа
                        .Select(group => group.First())); // Выбор только уникальных имен

                    listFamilyInstance2b.AddRange(new FilteredElementCollector(linkedDoc)
                        .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                        .OfClass(typeof(FamilyInstance))
                        .Cast<FamilyInstance>()
                        .Where(fi => fi.Location != null && (fi.Location is LocationPoint || fi.Location is LocationCurve)) // Проверка, что семейство размещено в пространстве проекта
                        .Where(fi => fi.Document.Equals(linkedDoc)) // Проверка, что семейство размещено в связанном файле
                        .Select(fi => new DisplayFamilyInstance
                        {
                            FamilyName = fi.Symbol.Family?.Name,
                            TypeName = fi.Symbol.Name,
                            Instance = fi
                        })
                        .GroupBy(fi => fi.TypeName)  // Группировка по имени типа
                        .Select(group => group.First())); // Выбор только уникальных имен
                }

            listFamilySymbol3a = new FilteredElementCollector(doc)
                       .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                       .OfClass(typeof(FamilySymbol))
                       .Cast<FamilySymbol>()
                       .Select(fs => new DisplayFamilySymbol
                       {
                           FamilyName = fs.Family.Name,
                           TypeName = fs.Name
                       })
                       .ToList();

            listFamilySymbol3b = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_CommunicationDevices)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Select(fs => new DisplayFamilySymbol
                {
                    FamilyName = fs.Family.Name,
                    TypeName = fs.Name
                })
                .ToList();

            // Открыть второе окно для выбора семейств
            Window2 window2 = new Window2(listFamilyInstance2a, listFamilyInstance2b, listFamilySymbol3a, listFamilySymbol3b);
            dialogResult = window2.ShowDialog();

            if (dialogResult != true)
            {
                TaskDialog.Show("Ошибка", "Не выбраны семейства.");
                return Result.Failed;
            }


            // Обработка выбранных семейств
            List<DisplayFamilyInstance> selectedFamilyInstances = window2.SelectedFamilyInstances;

            List<DisplayFamilyInstance> selectedFamilyInstances2a =
                selectedFamilyInstances.Count > 0 ?
                new List<DisplayFamilyInstance> { selectedFamilyInstances[0] } :
                new List<DisplayFamilyInstance>();

            List<DisplayFamilyInstance> selectedFamilyInstances2b =
                selectedFamilyInstances.Count > 1 ?
                new List<DisplayFamilyInstance> { selectedFamilyInstances[1] } :
                new List<DisplayFamilyInstance>();

            List<DisplayFamilyInstance> selectedFamilyInstances3a =
                selectedFamilyInstances.Count > 2 ?
                new List<DisplayFamilyInstance> { selectedFamilyInstances[2] } :
                new List<DisplayFamilyInstance>();

            List<DisplayFamilyInstance> selectedFamilyInstances3b =
                selectedFamilyInstances.Count > 3 ?
                new List<DisplayFamilyInstance> { selectedFamilyInstances[3] } :
                new List<DisplayFamilyInstance>();

            Transform transform = linkInstance.GetTransform();

            List<XYZ> insertionPoints2a = GetFamilyInstanceLocations(selectedFamilyInstances2a, linkedDoc, transform);
            List<XYZ> insertionPoints2b = GetFamilyInstanceLocations(selectedFamilyInstances2b, linkedDoc, transform);

            FamilySymbol familySymbol3a = FindFamilySymbol(doc, selectedFamilyInstances3a);
            FamilySymbol familySymbol3b = FindFamilySymbol(doc, selectedFamilyInstances3b);



            using (Transaction tx = new Transaction(doc, "Размещение семейств"))
            {
                tx.Start();
                List<ElementId> placedElementIdsA = PlaceFamilyInstances(doc, insertionPoints2a, familySymbol3a);
                List<ElementId> placedElementIds = PlaceFamilyInstances(doc, insertionPoints2b, familySymbol3b);
                tx.Commit();

                if (placedElementIdsA.Count > 0)
                {
                    uiDoc.Selection.SetElementIds(placedElementIdsA);
                    uiDoc.ShowElements(placedElementIdsA);
                    TaskDialog.Show("Результат", $"Семейства успешно размещено: {placedElementIdsA.Count} шт.");
                }
            }        


            return Result.Succeeded;
        }
        public List<XYZ> GetFamilyInstanceLocations(List<DisplayFamilyInstance> selectedFamilyInstances, Document linkedDoc, Transform transform)
        {
            List<XYZ> locations = new List<XYZ>();

            if (selectedFamilyInstances == null || selectedFamilyInstances.Count == 0 || linkedDoc == null)
            {
                return locations;
            }

            string familyName = selectedFamilyInstances[0].FamilyName;
            string typeName = selectedFamilyInstances[0].TypeName;

            var familyInstances = new FilteredElementCollector(linkedDoc)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(fi => fi.Symbol.Family.Name == familyName && fi.Symbol.Name == typeName);

            foreach (var instance in familyInstances)
            {
                var location = instance.Location as LocationPoint;
                if (location != null)
                {
                    XYZ transformedPoint = transform.OfPoint(location.Point);
                    locations.Add(transformedPoint);
                }
            }

            return locations;
        }
        private FamilySymbol FindFamilySymbol(Document doc, List<DisplayFamilyInstance> selectedFamilyInstances)
        {
            if (selectedFamilyInstances == null || selectedFamilyInstances.Count == 0)
                throw new ArgumentException("selectedFamilyInstances cannot be null or empty");

            DisplayFamilyInstance familyInstance = selectedFamilyInstances.First();
            string familyName = familyInstance.FamilyName;
            string typeName = familyInstance.TypeName;

            FamilySymbol familySymbol = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .FirstOrDefault(symbol => symbol.Family.Name.Equals(familyName) && symbol.Name.Equals(typeName));

            return familySymbol;
        }
        private string GetFamilyTypeName(Document doc, ElementId typeId)
        {
            ElementType elementType = doc.GetElement(typeId) as ElementType;
            return elementType != null ? elementType.Name : string.Empty;
        }
        private List<ElementId> PlaceFamilyInstances(Document doc, List<XYZ> insertionPoints, FamilySymbol familySymbol)
        {
            List<ElementId> placedElementIds = new List<ElementId>();

            // Получить все уровни
            List<Level> levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();

            foreach (var point in insertionPoints)
            {
                // Найти ближайший уровень
                Level closestLevel = null;
                double minDifference = double.MaxValue;

                foreach (var level in levels)
                {
                    double difference = Math.Abs(level.Elevation - point.Z);
                    if (difference < minDifference)
                    {
                        minDifference = difference;
                        closestLevel = level;
                    }
                }

                if (closestLevel != null)
                {
                    // Создаем рабочую плоскость на основе уровня
                    Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0, 0, closestLevel.Elevation));
                    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                    // Устанавливаем рабочую плоскость активной
                    doc.ActiveView.SketchPlane = sketchPlane;
                    doc.ActiveView.ShowActiveWorkPlane();

                    // Используем рабочую плоскость для размещения семейства
                    XYZ pointOnPlane = new XYZ(point.X, point.Y, closestLevel.Elevation);
                    FamilyInstance instance = doc.Create.NewFamilyInstance(pointOnPlane, familySymbol, sketchPlane, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                    if (instance != null)
                    {
                        placedElementIds.Add(instance.Id);
                    }
                }
            }

            return placedElementIds;
        }
    }
}

public class DisplayFamilyInstance
{
    public string FamilyName { get; set; }
    public string TypeName { get; set; }
    public FamilyInstance Instance { get; set; }
}

public class DisplayFamilySymbol
{
    public string FamilyName { get; set; }
    public string TypeName { get; set; }
}
