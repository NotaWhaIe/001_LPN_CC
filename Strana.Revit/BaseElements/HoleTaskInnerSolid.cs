// <copyright file="HoleTaskInnerSolid.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;

namespace Strana.Revit.BaseElements
{
    /// <summary>
    /// This class create solid for HoleTask and get HoleTask solid.
    /// </summary>
    public class HoleTaskInnerSolid
    {
        private Solid innerSolid;

        /// <summary>
        /// Gets the solid for determine the HoleTask status.
        /// </summary>
        public Solid InnerSolid
        {
            get { return this.innerSolid; }
        }
    }
}