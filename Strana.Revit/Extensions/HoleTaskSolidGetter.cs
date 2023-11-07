// <copyright file="HoleTaskSolidGetter.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;

namespace Strana.Revit.HoleTask.Extensions
{
    /// <summary>
    /// This class contains metod set up a HoleTasks familySybol.
    /// </summary>
    public static class HoleTaskSolidGetter
    {
        public static Solid GetHoleTaskSolidWithDelta(this FamilyInstance holeTaskItem, double delta = 0)
        {
            if (delta == 0)
            {
                return null;
            }

            return null;
        }
    }
}
