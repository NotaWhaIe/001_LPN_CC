using Autodesk.Revit.DB;
using Strana.Revit.HoleTask.RevitCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strana.Revit.HoleTask.Utils
{
    internal class SphereByPoint
    {
        ///*Вынести в отдельный вспомогательный класс этот метод:
        /// <summary>
        /// Create DirectShape sphere For test.
        /// </summary>
        /// <param name="doc"><seealso cref="Document"/></param>
        /// <param name="center"><seealso cref="XYZ"/></param>
        public static void CreateSphereByPoint(XYZ center, Document doc)
        {

            List<Curve> profile = [];

            // first create sphere with 2' radius
            //diameter = 0.5;
            //double radius = diameter/2;
            double radius = 0.2;
            XYZ profilePlus = center + new XYZ(0, radius, 0);
            XYZ profileMinus = center - new XYZ(0, radius, 0);

            profile.Add(Line.CreateBound(profilePlus, profileMinus));
            profile.Add(Arc.Create(profileMinus, profilePlus, center + new XYZ(radius, 0, 0)));

            CurveLoop curveLoop = CurveLoop.Create(profile);
            SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

            Frame frame = new Frame(center, XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);
            if (Frame.CanDefineRevitGeometry(frame))
            {
                Solid sphere = GeometryCreationUtilities.CreateRevolvedGeometry(frame, new CurveLoop[] { curveLoop }, 0, 2 * Math.PI, options);
                //using (Transaction t = new(doc, "create SphereByPoint"))
                //{
                //t.Start();
                DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Furniture));
                ds.ApplicationId = "Application id";
                ds.ApplicationDataId = "Geometry object id";
                ds.SetShape(new GeometryObject[] { sphere });
                //t.Commit();
                //}
            }
        }

    }
}
