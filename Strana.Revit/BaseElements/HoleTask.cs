// <copyright file="HoleTask.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

namespace Strana.Revit.BaseElements
{
    /// <summary>
    /// This is HoleTask. This element engenier will give to other enginier.
    /// </summary>
    public class HoleTask
    {
        private readonly HoleTaskInnerSolid innerSolid;

        /// <summary>
        /// Gets inner solid. This item used fo demention status.
        /// </summary>
        public HoleTaskInnerSolid InnerSolid
        {
            get { return this.innerSolid; }
        }

        /// <summary>
        /// Create a Task Hole by his geometry.
        /// </summary>
        public void CreateHoletask()
        {
            throw new System.NotSupportedException();
        }
    }
}