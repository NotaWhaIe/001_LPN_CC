// <copyright file="RevitLinkExstensions.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace Strana.RevitPlugins.Exstensions
{
    /// <summary>
    /// This class conteins exstensions for revit link.
    /// </summary>
    public static class RevitLinkExstensions
    {
        /// <summary>
        /// This extension returns all walls from link document.
        /// </summary>
        /// <param name="revitLinkInstance">This is revit link.</param>
        /// <returns>List with all walls from link.</returns>
        public static List<Element> LinkWalls(this RevitLinkInstance revitLinkInstance)
        {
            return new FilteredElementCollector(revitLinkInstance.Document)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType().ToElements().ToList();
        }

        /// <summary>
        /// This extension returns all intersecting elements from from link document.
        /// </summary>
        /// <param name="revitLinkInstance">This is revit link.</param>
        /// <returns>List with all intersecting elements.</returns>
        public static List<Element> AllIntersectedElements(this RevitLinkInstance revitLinkInstance)
        {
            return new FilteredElementCollector(revitLinkInstance.Document)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType().ToElements().ToList();
        }
    }
}
