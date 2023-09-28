// <copyright file="MepElementCollections.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

namespace Strana.Revit.HoleTask.ElementCollections
{
    /// <summary>
    /// Class contains all mep elements in revit model.
    /// </summary>
    public static class MepElementCollections
    {
        /// <summary>
        /// Gets all ducts to check intersecting.
        /// </summary>
        /// <param name="doc"><seealso cref="Document"/></param>
        /// <returns>Collections of Revit duct elements.</returns>
        public static IEnumerable<Element> AllDucts(Document doc)
        {
            return new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_DuctCurves)
                        .OfClass(typeof(Duct))
                        .WhereElementIsNotElementType();
        }

        /// <summary>
        /// Gets all pipes to check intersecting.
        /// </summary>
        /// <param name="doc"><seealso cref="Document"/></param>
        /// <returns>Collections of Revit pipe elements.</returns>
        public static IEnumerable<Element> AllPipes(Document doc)
        {
            return new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_PipeCurves)
                        .OfClass(typeof(Pipe))
                        .WhereElementIsNotElementType();
        }

        /// <summary>
        /// Gets all cable trays to check intersecting.
        /// </summary>
        /// <param name="doc"><seealso cref="Document"/></param>
        /// <returns>Collections of Revit cable tray elements.</returns>
        public static IEnumerable<Element> AllCableTrays(Document doc)
        {
            return new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_CableTray)
                        .OfClass(typeof(CableTray))
                        .WhereElementIsNotElementType();
        }

        /// <summary>
        /// Gets all MEP elements to check intersecting (ducts, pipes, cable trays).
        /// </summary>
        /// <param name="doc"><seealso cref="Document"/></param>
        /// <returns>Collections of Revit MEP elements.</returns>
        public static IEnumerable<Element> AllMepElements(Document doc)
        {
            return AllDucts(doc).Concat(AllPipes(doc)).Concat(AllCableTrays(doc));
        }
    }
}
