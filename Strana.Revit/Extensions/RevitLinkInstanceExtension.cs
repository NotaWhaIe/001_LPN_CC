// <copyright file="RevitLinkInstanceExtension.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using Autodesk.Revit.DB;
using Strana.Revit.HoleTask.ElementCollections;
using Strana.Revit.HoleTask.Extension.RevitElement;
using Strana.Revit.HoleTask.Utils;

namespace Strana.Revit.HoleTask.Extensions
{
    public static class RevitLinkInstanceExtension
    {
        public static void CreateHoleTasksByCurrentLink(this RevitLinkInstance linkInstance)
        {
            using (var t = new Transaction(linkInstance.Document, "Create all instances of hole task"))
            {
                t.Start();
                Document linkDoc = linkInstance.GetLinkDocument();

                // Взять стены и перекрытия
                IEnumerable<Element> allIntersectingElements = CollectionsOfIntersectingElements.AllIntersectingElements(linkDoc);

                foreach (Element intersectingElement in allIntersectingElements)
                {
                    intersectingElement.CreateHoleTasksByIntersectedElements(linkInstance);
                }

                t.Commit();
            }
        }
    }
}
