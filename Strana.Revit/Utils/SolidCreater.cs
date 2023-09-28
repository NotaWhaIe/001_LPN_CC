// <copyright file="SolidCreater.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;

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
    }
}
