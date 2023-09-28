// <copyright file="CollectionsOfIntersectingElements.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace Strana.Revit.HoleTask.ElementCollections
{
    /// <summary>
    /// Contains All Collections With intersecting elements.
    /// </summary>
    public static class CollectionsOfIntersectingElements
    {
        /// <summary>
        /// Gets all Walls to check intersecting.
        /// </summary>
        /// <param name="doc"><seealso cref="Document"/></param>
        /// <returns>Collections revit wall elements.</returns>
        public static IEnumerable<Element> AllWalls(Document doc)
        {
            return new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_Walls)
                        .OfClass(typeof(Wall))
                        .WhereElementIsNotElementType();
        }

        /// <summary>
        /// Gets all floors to check intersecting.
        /// </summary>
        /// <param name="doc"><seealso cref="Document"/></param>
        /// <returns>Collections revit floors elements.</returns>
        public static IEnumerable<Element> AllFloors(Document doc)
        {
            return new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_Floors)
                        .OfClass(typeof(Floor))
                        .WhereElementIsNotElementType();
        }

        /// <summary>
        /// Gets all elements to check intersecting (floors and walls).
        /// </summary>
        /// <param name="doc"><seealso cref="Document"/></param>
        /// <returns>Collections revit floors and walls elements.</returns>
        public static IEnumerable<Element> AllIntersectingElements(Document doc)
        {
            return AllWalls(doc).Concat(AllFloors(doc));
        }
    }
}
