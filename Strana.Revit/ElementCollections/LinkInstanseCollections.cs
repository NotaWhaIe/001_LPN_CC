// <copyright file="LinkInstanseCollections.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace Strana.Revit.HoleTask.ElementCollections
{
    /// <summary>
    /// Helper to return collections with all document links.
    /// </summary>
    public static class LinkInstanseCollections
    {
        /// <summary>
        /// Collections with all revit links.
        /// </summary>
        /// <param name="doc"><seealso cref="Document"/></param>
        /// <returns>collection with revit links.</returns>
        public static IEnumerable<RevitLinkInstance> RevitLinks(Document doc, bool isReternSelected = true)
        {
            var revitLinksCollector = new FilteredElementCollector(doc)
                  .OfClass(typeof(RevitLinkInstance))
                  .Cast<RevitLinkInstance>();

            if (isReternSelected)
            {
                string revitLinks = Confing.Default.revitLinks;
                List<string> revitLinksList = revitLinks.Split(';').ToList();
                var selectedRevitLinks = revitLinksCollector.Where(link => revitLinksList.Any(link.Name.Contains));
                return selectedRevitLinks;
            }
            else
            {
                return revitLinksCollector;
            }

        }
    }
}
