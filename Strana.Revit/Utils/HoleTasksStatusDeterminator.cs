// <copyright file="HoleTasksStatusDeterminator.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace Strana.Revit.HoleTask.Utils
{
    internal class HoleTasksStatusDeterminator
    {
        public HoleTasksStatusDeterminator()
        {
        }

        /// <summary>
        /// This method create statuses for all hole tasks.
        /// </summary>
        /// <param name="allHoleTasks">all hole tasks.</param>
        /// <param name="allIntersectedElements"> all intersected items (walls and floors).</param>
        public void DeterminateAllStatuses(List<FamilyInstance> allHoleTasks, IEnumerable<Element> allIntersectedElements)
        {
            // do nothing.
        }
    }
}