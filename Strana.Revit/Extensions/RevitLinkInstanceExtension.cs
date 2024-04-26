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

                List<FamilyInstance> allHoleTaskByRevitLinkInstance = IntersectingElementExtension.CreateHoleTasksByIntersectedElements(
                    linkInstance).ToList();
                
                List<FamilyInstance> roundHoleTaskList = new HoleTasksJoiner().JoinAllHoleTask(allHoleTaskByRevitLinkInstance);

                new HoleTasksLineStretch().StretchLinesAllHoleTask(linkInstance);
                t.Commit();
            }

        }
    }
}
