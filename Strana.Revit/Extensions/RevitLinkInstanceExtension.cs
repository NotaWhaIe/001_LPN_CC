// <copyright file="RevitLinkInstanceExtension.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

                /// Take walls and floors
                IEnumerable<Element> allIntersectingElements = CollectionsOfIntersectingElements.AllIntersectingElements(linkDoc);

                foreach (Element intersectingElement in allIntersectingElements)
                {
                    allHoleTaskByRevitLinkInstance = allHoleTaskByRevitLinkInstance
                        .Concat(intersectingElement
                        .CreateHoleTasksByIntersectedElements(linkInstance))
                        .ToList();
                }

                /// HoleTasksJoiner необходимо доработать, не возвращается список заданий у метода, Сделать Void!!!!!
                List<FamilyInstance> roundHoleTaskList = new HoleTasksJoiner().JoinAllHoleTask(allHoleTaskByRevitLinkInstance);

                ///округлить геометрию заданий
                //new HoleTasksRoundUpDimension().RoundUpAllHoleTask(roundHoleTaskList);

                ///округлить в плане

                ///растянуть по высоте
                new HoleTasksLineStretch().StretchLinesAllHoleTask(linkInstance);
                t.Commit();
            }
        }
    }
}
