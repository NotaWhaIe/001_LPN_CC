// <copyright file="RevitLinkInstanceExtension.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Strana.Revit.HoleTask.ElementCollections;
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
                List<FamilyInstance> allHoleTaskByRevitLinkInstance = new();
                Document linkDoc = linkInstance.GetLinkDocument();

                // Взять стены и перекрытия
                IEnumerable<Element> allIntersectingElements = CollectionsOfIntersectingElements.AllIntersectingElements(linkDoc);

                foreach (Element intersectingElement in allIntersectingElements)
                {
                    allHoleTaskByRevitLinkInstance = allHoleTaskByRevitLinkInstance
                        .Concat(intersectingElement
                        .CreateHoleTasksByIntersectedElements(linkInstance))
                        .ToList();
                }

                List<FamilyInstance> joinHoleTaskList= new HoleTasksJoiner().JoinAllHoleTask(allHoleTaskByRevitLinkInstance);

                // Тест создания солида с дельтой.
                foreach (FamilyInstance f in joinHoleTaskList)
                {
                    Solid s = f.GetHoleTaskSolidWithDelta(20.0/304.8);
                }

                t.Commit();
            }
        }
    }
}
