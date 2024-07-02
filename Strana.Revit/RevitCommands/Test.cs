using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Data.SQLite;

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

            // Get all Revit link instances in the document
            List<RevitLinkInstance> revitLinkInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Where(x => x.GetLinkDocument() != null)
                .ToList();

            // Open WPF window to select links
            Window1 mainWindow = new Window1(revitLinkInstances);
            bool? dialogResult = mainWindow.ShowDialog();

            if (dialogResult != true || mainWindow.SelectedLinks == null || !mainWindow.SelectedLinks.Any())
            {
                TaskDialog.Show("Error", "No links selected.");
                return Result.Failed;
            }

            // Data in WPF with selected links
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
                    .Where(fi => fi.Location != null && (fi.Location is LocationPoint || fi.Location is LocationCurve)) // Check that the family is placed in the project space
                    .Where(fi => fi.Document.Equals(linkedDoc)) // Check that the family is placed in the linked file
                    .Select(fi => new DisplayFamilyInstance
                    {
                        FamilyName = fi.Symbol.Family?.Name,
                        TypeName = fi.Symbol.Name,
                        Instance = fi
                    })
                    .GroupBy(fi => fi.TypeName)  // Group by type name
                    .Select(group => group.First())); // Select only unique names

                listFamilyInstance2b.AddRange(new FilteredElementCollector(linkedDoc)
                    .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .Where(fi => fi.Location != null && (fi.Location is LocationPoint || fi.Location is LocationCurve)) // Check that the family is placed in the project space
                    .Where(fi => fi.Document.Equals(linkedDoc)) // Check that the family is placed in the linked file
                    .Select(fi => new DisplayFamilyInstance
                    {
                        FamilyName = fi.Symbol.Family?.Name,
                        TypeName = fi.Symbol.Name,
                        Instance = fi
                    })
                    .GroupBy(fi => fi.TypeName)  // Group by type name
                    .Select(group => group.First())); // Select only unique names
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

            // Open second window to select families
            Window2 window2 = new Window2(listFamilyInstance2a, listFamilyInstance2b, listFamilySymbol3a, listFamilySymbol3b);
            dialogResult = window2.ShowDialog();

            if (dialogResult != true)
            {
                TaskDialog.Show("Error", "No families selected.");
                return Result.Failed;
            }
            // Process selected families
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

            // Get insertion points for selected FamilyInstances
            List<XYZ> insertionPoints2a = GetFamilyInstanceLocations(selectedFamilyInstances2a, linkedDoc, transform);
            List<XYZ> insertionPoints2b = GetFamilyInstanceLocations(selectedFamilyInstances2b, linkedDoc, transform);

            // Find FamilySymbols in the current document for selected FamilyInstances
            FamilySymbol familySymbol3a = FindFamilySymbol(doc, selectedFamilyInstances3a);
            FamilySymbol familySymbol3b = FindFamilySymbol(doc, selectedFamilyInstances3b);

            using (Transaction tx = new Transaction(doc, "Place Families"))
            {
                tx.Start();
                // Place FamilySymbol3a instances
                List<ElementId> placedElementIdsA = PlaceFamilyInstances(doc, insertionPoints2a, familySymbol3a);
                // Place FamilySymbol3b instances
                List<ElementId> placedElementIdsB = PlaceFamilyInstances(doc, insertionPoints2b, familySymbol3b);
                tx.Commit();

                if (placedElementIdsA.Count > 0)
                {
                    uiDoc.Selection.SetElementIds(placedElementIdsA);
                    uiDoc.ShowElements(placedElementIdsA);
                    TaskDialog.Show("Result", $"Families successfully placed: {placedElementIdsA.Count} instances.");
                }

                if (placedElementIdsB.Count > 0)
                {
                    uiDoc.Selection.SetElementIds(placedElementIdsB);
                    uiDoc.ShowElements(placedElementIdsB);
                    TaskDialog.Show("Result", $"Families successfully placed: {placedElementIdsB.Count} instances.");
                }
            }

            return Result.Succeeded;
        }

        // Method to get XYZ locations for selected FamilyInstances
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

        // Method to find FamilySymbol in the current document based on selected FamilyInstances
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

        // Method to place FamilyInstances at specified XYZ points
        private List<ElementId> PlaceFamilyInstances(Document doc, List<XYZ> insertionPoints, FamilySymbol familySymbol)
        {
            List<ElementId> placedElementIds = new List<ElementId>();

            // Get all levels
            List<Level> levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();

            foreach (var point in insertionPoints)
            {
                // Find the nearest level
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
                    // Create a work plane based on the level
                    Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0, 0, closestLevel.Elevation));
                    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                    // Set the work plane active
                    doc.ActiveView.SketchPlane = sketchPlane;
                    doc.ActiveView.ShowActiveWorkPlane();

                    // Use the work plane to place the family instance
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
