// <copyright file="HoleTaskFamilyLoader.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using Autodesk.Revit.Creation;

namespace Strana.Revit.HoleTask.Utils
{
    /// <summary>
    /// This class contains metod to check are needed families in project.
    /// </summary>
    public class HoleTaskFamilyLoader
    {
        /// <summary>
        /// This class check are neaded to plugin families are loaded.
        /// If families not load this class load  families in current project.
        /// </summary>
        /// <param name="doc">Current revit document.<seealso cref="Document"/></param>
        public void LoadUnloadedHoleTaskFamilies(Document doc)
        {
            throw new NotSupportedException();
        }
    }
}
