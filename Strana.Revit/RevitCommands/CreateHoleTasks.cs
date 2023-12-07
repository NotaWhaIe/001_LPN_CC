﻿// <copyright file="CreateHoleTasks.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Strana.Revit.HoleTask.ElementCollections;
using Strana.Revit.HoleTask.Extension.RevitElement;
using Strana.Revit.HoleTask.Extensions;
using Strana.Revit.HoleTask.Utils;
using Strana.Revit.HoleTask.ViewModel;

namespace Strana.Revit.HoleTask.RevitCommands
{
    /// <summary>
    /// Start Up HoleTask Plugin.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class CreateHoleTasks : IExternalCommand
    {
        /// <summary>
        /// Executed when buttoon clicked.
        /// </summary>
        /// <param name="commandData"><seealso cref="ExternalCommandData"/></param>
        /// <param name="message">revit message.</param>
        /// <param name="elements">revit elements set.</param>
        /// <returns>voiding.</returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            docsaver.doc = doc;

            HoleTaskView taskView = new (doc);
            taskView.ShowDialog();
            HoleTaskViewModel tt = taskView.DataContext as HoleTaskViewModel;


            using (var gt = new TransactionGroup(doc, "HoleTasks"))
            {
                gt.Start();

                foreach (RevitLinkInstance linkInstance in LinkInstanseCollections.RevitLinks(doc))
                {
                    /// Разместить метод по парсингу связи
                    linkInstance.CreateHoleTasksByCurrentLink();
                }

                gt.Assimilate();
            }

            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            TaskDialog.Show("Время работы", elapsedTime.TotalSeconds.ToString() + " сек.");

            return Result.Succeeded;
        }
    }
    public static class docsaver
    {
        public static Document doc { get; set; }
    }
}
