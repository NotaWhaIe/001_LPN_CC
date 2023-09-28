// <copyright file="CreateHoleTasks.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Strana.Revit.HoleTask.RevitCommands
{
    /// <summary>
    /// Start Up HoleTask Plugin.
    /// </summary>
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
            throw new System.NotImplementedException();
        }
    }
}
