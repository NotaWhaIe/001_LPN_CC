// <copyright file="RevitLinkInstanceExtension.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using FirstRevitPlugin.FailuresProcessing;
using Strana.Revit.HoleTask.ElementCollections;
using Strana.Revit.HoleTask.Extensions.RevitElement;
using Strana.Revit.HoleTask.Utils;

namespace Strana.Revit.HoleTask.Extensions
{
    public static class RevitLinkInstanceExtension
    {
        public static void CreateHoleTasksByCurrentLink(this RevitLinkInstance linkInstance)
        {
            using (var t = new Transaction(linkInstance.Document, "Create all instances of hole task"))
            {
                TransactionHandler.SetWarningResolver(t);

                t.Start();
                Document linkDoc = linkInstance.GetLinkDocument();
                Document doc = linkInstance.Document;
                List<FamilyInstance> allHoleTaskByRevitLinkInstance = new();
                HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Стены_Прямоугольное", allHoleTaskByRevitLinkInstance);
                HoleTasksGetter.AddFamilyInstancesToList(doc, "(Отв_Задание)_Перекрытия_Прямоугольное", allHoleTaskByRevitLinkInstance);


                /// Take walls and floors
                IEnumerable<Element> allIntersectingElements = CollectionsOfIntersectingElements.AllIntersectingElements(linkDoc);

                ///метод округляет при задании геометрических размеров:
                ///округлить геометрию заданий
                ///округлить в плане

                foreach (Element intersectingElement in allIntersectingElements)
                {
                    allHoleTaskByRevitLinkInstance = allHoleTaskByRevitLinkInstance
                        .Concat(intersectingElement
                        .CreateHoleTasksByIntersectedElements(linkInstance))
                        .ToList();
                }

                /// HoleTasksJoiner 
                List<FamilyInstance> roundHoleTaskList = new HoleTasksJoiner().JoinAllHoleTask(allHoleTaskByRevitLinkInstance);

                ///растянуть по высоте
                new HoleTasksLineStretch().StretchLinesAllHoleTask(linkInstance);
                t.Commit();
            }
        }
    }
}
