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
        private static readonly double minMepElementLength = Confing.Default.minMepElementLength;
        private static readonly double minMepElementSize = Confing.Default.minMepElementSize;

        public static IEnumerable<Element> GetSelectedOrAllMepElements()
        {
            Document doc = UIDocument.Document;
            ICollection<ElementId> selectedIds = UIDocument.Selection.GetElementIds();

            IEnumerable<Element> mepElements;

            if (selectedIds.Count > 0)
            {
                mepElements = selectedIds.Select(id => doc.GetElement(id))
                                         .Where(elem => elem is Duct || elem is Pipe || elem is CableTray)
                                         .ToList();
            }
            else
            {
                var pipes = new FilteredElementCollector(doc)
                            .OfClass(typeof(Pipe))
                            .WhereElementIsNotElementType()
                            .Where(pipe =>
                            {
                                Parameter outerDiameterParam = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
                                if (outerDiameterParam != null && outerDiameterParam.StorageType == StorageType.Double)
                                {
                                    double outerDiameterInMm = UnitUtils.ConvertFromInternalUnits(outerDiameterParam.AsDouble(), UnitTypeId.Millimeters);
                                    return outerDiameterInMm >= minMepElementSize;
                                }
                                return false;
                            })
                            .Cast<Element>();

                var ducts = new FilteredElementCollector(doc)
                            .OfClass(typeof(Duct))
                            .WhereElementIsNotElementType()
                            .Where(duct =>
                            {
                                Parameter heightParam = duct.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM);
                                Parameter widthParam = duct.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM);
                                double heightInMm = 0, widthInMm = 0;
                                if (heightParam != null && heightParam.StorageType == StorageType.Double)
                                {
                                    heightInMm = UnitUtils.ConvertFromInternalUnits(heightParam.AsDouble(), UnitTypeId.Millimeters);
                                }
                                if (widthParam != null && widthParam.StorageType == StorageType.Double)
                                {
                                    widthInMm = UnitUtils.ConvertFromInternalUnits(widthParam.AsDouble(), UnitTypeId.Millimeters);
                                }
                                return heightInMm >= minMepElementSize && widthInMm >= minMepElementSize;
                            })
                            .Cast<Element>();

                var cableTrays = new FilteredElementCollector(doc)
                            .OfClass(typeof(CableTray))
                            .WhereElementIsNotElementType()
                            .Where(tray =>
                            {
                                Parameter heightParam = tray.get_Parameter(BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM);
                                Parameter widthParam = tray.get_Parameter(BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM);
                                double heightInMm = 0, widthInMm = 0;
                                if (heightParam != null && heightParam.StorageType == StorageType.Double)
                                {
                                    heightInMm = UnitUtils.ConvertFromInternalUnits(heightParam.AsDouble(), UnitTypeId.Millimeters);
                                }
                                if (widthParam != null && widthParam.StorageType == StorageType.Double)
                                {
                                    widthInMm = UnitUtils.ConvertFromInternalUnits(widthParam.AsDouble(), UnitTypeId.Millimeters);
                                }
                                return heightInMm >= minMepElementSize && widthInMm >= minMepElementSize;
                            })
                            .Cast<Element>();

                mepElements = ducts.Concat(pipes).Concat(cableTrays).ToList();
            }

            var mepElementsFilterByLength = FilterElementsByLength(mepElements, minMepElementLength);
            return mepElementsFilterByLength;
        }

        private static IEnumerable<Element> FilterElementsByLength(IEnumerable<Element> elements, double lengthInMM)
        {
            return elements.Where(e =>
            {
                Parameter lengthParam = e.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                if (lengthParam != null)
                {
                    double lengthInInternalUnits = lengthParam.AsDouble();
                    var a = UnitUtils.ConvertFromInternalUnits(lengthInInternalUnits, UnitTypeId.Millimeters);
                    var b = a >= lengthInMM;
                    return b;
                }
                return false;
            });
        }
    }
}
