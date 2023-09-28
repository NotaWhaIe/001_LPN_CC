// <copyright file="SolidCreater.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace Strana.Revit.HoleTask.Utils
{
    /// <summary>
    /// ConteinsOne method to create solid in model.
    /// </summary>
    public static class SolidCreater
    {
        /// <summary>
        /// helping util for show solid in model.
        /// </summary>
        /// <param name="doc"><seealso cref="Document"/></param>
        /// <param name="solid"><seealso cref="Solid"/></param>
        public static void CreateSolid(Document doc, Solid solid)
        {
            using (Transaction t = new Transaction(doc, "создание солида"))
            {
                t.Start();

                // create direct shape and assign the sphere shape
                DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));

                ds.ApplicationId = "Application id";
                ds.ApplicationDataId = "Geometry object id";
                ds.SetShape(new GeometryObject[] { solid });

                t.Commit();
            }
        }

        public static void CreateSphereByPoint(Document doc, XYZ center)
        {
            List<Curve> profile = new List<Curve>();

            // first create sphere with 2' radius
            double radius = 2.0;
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
                using (Transaction t = new (doc, "SphereByPoint"))
                {
                    t.Start();

                    // create direct shape and assign the sphere shape
                    DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));

                    ds.ApplicationId = "Application id";
                    ds.ApplicationDataId = "Geometry object id";
                    ds.SetShape(new GeometryObject[] { sphere });
                    t.Commit();
                }
            }
        }
    }
}
