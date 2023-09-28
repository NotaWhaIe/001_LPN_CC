// <copyright file="SolidGetter.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;

namespace Strana.Revit.HoleTask.Extension.RevitElement
{
    /// <summary>
    /// This extension for geting element solids.
    /// </summary>
    public static class SolidGetter
    {
        private static readonly Options Opt = new ()
        {
            ComputeReferences = true,
            DetailLevel = ViewDetailLevel.Fine,
        };

        /// <summary>
        /// Get from wall and floor solid without holes.
        /// </summary>
        /// <param name="element">This is fooor or wall.</param>
        /// <returns><seealso cref="Solid"/></returns>
        /// <remarks>now return solid with holes.</remarks>
        public static Solid GetSolidWithoutHoles(this Element element)
        {
            return element.GetSolidWithHoles();
        }

        private static Solid GetSolidWithHoles(this Element element)
        {
            GeometryElement geometryElement = element.get_Geometry(Opt);
            Solid elementSolid = null;

            foreach (GeometryObject geometry in geometryElement)
            {
                elementSolid = geometry as Solid;
            }

            return elementSolid;
        }
    }
}
