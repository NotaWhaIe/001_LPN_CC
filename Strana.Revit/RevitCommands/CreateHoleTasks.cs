// <copyright file="CreateHoleTasks.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Strana.Revit.HoleTask.ElementCollections;
using Strana.Revit.HoleTask.Extension.RevitElement;
using Strana.Revit.HoleTask.Utils;

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
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            RevitLinkInstance linkDoc = LinkInstanseCollections.RevitLinks(doc).FirstOrDefault();
            Element floor = CollectionsOfIntersectingElements.AllFloors(linkDoc.GetLinkDocument()).FirstOrDefault();

            Solid floorSolid = floor.GetSolidWithoutHoles();
            SolidCreater.CreateSolid(doc, floorSolid);

            return Result.Succeeded;
        }
    }
}
