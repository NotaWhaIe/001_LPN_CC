using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using HoleTasksPlagin;
using Autodesk.Revit.DB.IFC;
using System.Diagnostics;
//using Autodesk.DesignScript.Geometry;


namespace HoleTask1Plugin
{
    [Autodesk.Revit.Attributes.TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class HoleTasks1Command : IExternalCommand
    {
        static AddInId addinId = new AddInId(new Guid("f64706dd-e8f6-4cbe-9cc6-a2910be5ad5a"));
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {


            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Document linkDoc = null;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;

            var selFilterRevitLinkInstance = new RevitLinkInstanceSelectionFilter();
            Reference selRevitLinkInstance = null;
            try
            {
                selRevitLinkInstance = sel.PickObject(ObjectType.Element, selFilterRevitLinkInstance, "Выберите связанный файл!");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            var revitLinkInstance = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Where(li => li.Id == selRevitLinkInstance.ElementId)
                .Cast<RevitLinkInstance>();
            if (revitLinkInstance.Count() == 0)
            {
                TaskDialog.Show("Revit", "Связанный файл не найден!");
                return Result.Cancelled;
            }
            linkDoc = revitLinkInstance.First().GetLinkDocument();
            Transform transform = revitLinkInstance.First().GetTotalTransform();

            var wallsInLinkList = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .Cast<Wall>()
                .ToList();
            //List<Floor> floorsInLinkList = new FilteredElementCollector(linkDoc)
            //    .OfClass(typeof(Floor))
            //    .Cast<Floor>()
            //    .ToList();
            //List<Pipe> pipesList = new FilteredElementCollector(doc)
            //    .OfClass(typeof(Pipe))
            //    .Cast<Pipe>()
            //    .ToList();
            //List<Duct> ductsList = new FilteredElementCollector(doc)
            //    .OfClass(typeof(Duct))
            //    .Cast<Duct>()
            //    .ToList();

            Solid wallSolid = GetElementSolid(doc, wallsInLinkList[0], revitLinkInstance.First());



            Face faceWithHoles = GetSolidMainFace(wallSolid);
            CurveLoop getCurveLoopWithoutHoles = GetCurveLoopWithoutHoles(faceWithHoles);
            Solid getSolidWithoutHoles = GetSolidWithoutHoles(getCurveLoopWithoutHoles, wallSolid);

            //стена в проекте Volume: 141,369009349903 ft3 ~ 4 m3

            double vWallSolid = wallSolid.Volume;
            double vGSWH = getSolidWithoutHoles.Volume;

            TaskDialog.Show("vWallSolid", vWallSolid.ToString());
            TaskDialog.Show("vGSWH", vGSWH.ToString());

            //директ шейп

            //using (Transaction t = new Transaction(doc, "Create sphere direct shape"))
            //{
            //    t.Start();
            //    // create direct shape and assign the sphere shape
            //    DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));

            //    ds.ApplicationId = "Application id";
            //    ds.ApplicationDataId = "Geometry object id";
            //    ds.SetShape(new GeometryObject[] { wallSolid });
            //    t.Commit();
            //}


            ///Найти пересечения созданного через эту фичу объема и трубы

            //var bb = wall.get_BoundingBox(null);
            //Outline outline = new Outline(bb.Min, bb.Max);
            //BoundingBoxIntersectsFilter bbFilter = new BoundingBoxIntersectsFilter(outline);
            //ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(wall);
            //var ducts = new FilteredElementCollector(doc).OfClass(typeof(Duct)).WherePasses(bbFilter).WherePasses(filter).ToElements();
            //foreach (var el in ducts)
            //{
            //    result += wall.Id.ToString() + ";" + el.Id.ToString() + Environment.NewLine;
            //    counter++;
            //}
            //TaskDialog.Show("Найдено пересечений " + counter.ToString(), result);

            ///Обработка геометрии Перекрытий
            //TaskDialog.Show("", "test:" + getCurveLoopWithoutHoles.IsValidObject.ToString());
            return Result.Succeeded;
        }
        public Solid GetElementSolid(Document doc, Element element, RevitLinkInstance revitLinkInstance)
        {
            dynamic geoElement = null;

            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.DetailLevel = ViewDetailLevel.Fine;
            geoElement = element.get_Geometry(opt);///добавить фильтрацию по солидам

            Solid solid = null;


            var linkDoc = revitLinkInstance.GetLinkDocument();
            Transform transform = revitLinkInstance.GetTotalTransform();

            // Get geometry object
            foreach (GeometryObject geoObject in geoElement)
            {
                solid = geoObject as Solid;
                if (null != solid)
                {
                    using (Transaction t = new Transaction(doc, "Create sphere direct shape"))
                    {
                        t.Start();
                        // create direct shape and assign the sphere shape
                        DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));

                        ds.ApplicationId = "Application id";
                        ds.ApplicationDataId = "Geometry object id";
                        ds.SetShape(new GeometryObject[] { solid });
                        t.Commit();
                    }

                    using (Transaction t = new Transaction(doc, "Create sphere direct shape"))
                    {
                        t.Start();
                        // create direct shape and assign the sphere shape
                        DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));

                        ds.ApplicationId = "Application id";
                        ds.ApplicationDataId = "Geometry object id";
                        ds.SetShape(new GeometryObject[] { SolidUtils.CreateTransformed(solid, transform) });
                        t.Commit();
                    }
                    solid = SolidUtils.CreateTransformed(solid, transform);
                }
            }
            return solid;
        }

        public Face GetSolidMainFace(Solid solid)
        {
            Face faceMaxSquare = null;
            var faces = solid.Faces;
            foreach (Face solidface in faces)
            {
                if (faceMaxSquare == null || faceMaxSquare.Area < solidface.Area)
                    faceMaxSquare = solidface;
            }
            ///Метод возвращает одну из поверхностей с макс Площадью
            ///Найти сторону в которую нужно выдавить поверхность
            return faceMaxSquare;
        }

        ///  Список линий по которым отстроить солид
        public CurveLoop GetCurveLoopWithoutHoles(Face faceWithHoles)
        {
            EdgeArrayArray AllFaceEdges = faceWithHoles.EdgeLoops;
            List<Curve> curentUnitedCurve = new List<Curve>();
            List<CurveLoop> CurentUnitedCurveLoopList = new List<CurveLoop>();
            CurveLoop CurentUnitedCurveLoop = null;

            foreach (EdgeArray AgesOfOneFace in AllFaceEdges)
            {
                List<Curve> UnitedCurve = new List<Curve>();

                foreach (Edge item in AgesOfOneFace)
                {

                    var Curve = item.AsCurve();
                    UnitedCurve.Add(Curve);
                }
                List<CurveLoop> curveLoopList = new List<CurveLoop>();
                CurveLoop curvesLoop = CurveLoop.Create(UnitedCurve);
                curveLoopList.Add(curvesLoop);
                if (CurentUnitedCurveLoop == null || ExporterIFCUtils
                    .ComputeAreaOfCurveLoops(curveLoopList) > ExporterIFCUtils
                    .ComputeAreaOfCurveLoops(CurentUnitedCurveLoopList))
                {
                    CurentUnitedCurveLoop = curvesLoop;
                    CurentUnitedCurveLoopList = new List<CurveLoop>();
                    CurentUnitedCurveLoopList.Add(curvesLoop);
                }
            }
            return CurentUnitedCurveLoop;

        }
        public CurveLoop GetSweepPath(Solid solid)
        {
            Edge edgeMin = null;
            var edges = solid.Edges;
            foreach (Edge solidEdge in edges)
            {
                if (edgeMin == null || edgeMin.AsCurve().Length > solidEdge.AsCurve().Length)
                    edgeMin = solidEdge;
            }
            List<Curve> Curve = new List<Curve>();
            Curve.Add(edgeMin.AsCurve());
            CurveLoop edgeMinCurvesLoop = CurveLoop.Create(Curve);
            return edgeMinCurvesLoop;
        }

        public Solid GetSolidWithoutHoles(CurveLoop getCurveLoopWithoutHoles, Solid elementSolid)
        {
            CurveLoop sweepPath = GetSweepPath(elementSolid);
            var pathAttachmentParam = sweepPath.First().GetEndParameter(0);
            List<CurveLoop> getCurveLoopWithoutHolesList = new List<CurveLoop>();
            getCurveLoopWithoutHolesList.Add(getCurveLoopWithoutHoles);
            SolidOptions solidOptions = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);
            //SolidOptions solidOptions = new SolidOptions(new ElementId(82788), new ElementId(31));
            Solid currentSolid = GeometryCreationUtilities.CreateSweptGeometry(sweepPath, 0, pathAttachmentParam, getCurveLoopWithoutHolesList, solidOptions);
            return currentSolid;
        }
    }

}