using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Strana.Revit.HoleTask.ViewModel;
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
            IEnumerable<Element> mepElements;
            double minMepElementLength = UnitUtils.ConvertToInternalUnits(WpfSettings.MinMepElementLength, DisplayUnitType.DUT_MILLIMETERS);
            double minMepElementSizeOrDiametor = UnitUtils.ConvertToInternalUnits(WpfSettings.MinMepElementSize, DisplayUnitType.DUT_MILLIMETERS);

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
                                    double outerDiameterInMm = outerDiameterParam.AsDouble();
                                    return outerDiameterInMm >= minMepElementSizeOrDiametor;
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
                                Parameter diameterParam = duct.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM);
                                double heightInMm = 0, widthInMm = 0, diameterInMm = 0;

                                if (heightParam != null && heightParam.StorageType == StorageType.Double)
                                {
                                    heightInMm = heightParam.AsDouble();
                                }

                                if (widthParam != null && widthParam.StorageType == StorageType.Double)
                                {
                                    widthInMm = widthParam.AsDouble();
                                }
                                if (diameterParam != null && diameterParam.StorageType == StorageType.Double)
                                {
                                    diameterInMm = diameterParam.AsDouble();
                                }
                                return heightInMm >= minMepElementSizeOrDiametor || widthInMm >= minMepElementSizeOrDiametor || diameterInMm >= minMepElementSizeOrDiametor;
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
                                   heightInMm = heightParam.AsDouble();
                               }
                       
                               if (widthParam != null && widthParam.StorageType == StorageType.Double)
                               {
                                   widthInMm = widthParam.AsDouble();
                               }
                       
                               return heightInMm >= minMepElementSizeOrDiametor || widthInMm >= minMepElementSizeOrDiametor;
                           })
                           .Cast<Element>();

                mepElements = ducts.Concat(pipes).Concat(cableTrays).ToList();
                int debagg = mepElements.Count();

            }
            var mepElementsFilterByLength = FilterElementsByLength(mepElements, minMepElementLength);
            int debag = mepElementsFilterByLength.Count();
            return mepElementsFilterByLength;
        }

        private static IEnumerable<Element> FilterElementsByLength(IEnumerable<Element> elements, double lengthInMM)
        {
            return elements.Where(e =>
            {
                Parameter lengthParam = e.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                if (lengthParam != null)
                {
                    var a = lengthParam.AsDouble();
                    var b = a >= lengthInMM;
                    return b;
                }
                return false;
            });
        }
    }
}
