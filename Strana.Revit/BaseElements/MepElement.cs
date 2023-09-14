// <copyright file="MepElement.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;
using Strana.Revit.Exstensions;

namespace Strana.Revit.BaseElements
{
    /// <summary>
    /// This lineal revit element abstract class (Pipes, Ducts).
    /// </summary>
    public class MepElement
    {
        private readonly Element element;

        private readonly ElementType elementType;

        private MepElementGeometryType geometryType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MepElement"/> class.
        /// </summary>
        /// <param name="element">This is base revit element (Walls, Floors, Roofs).</param>
        public MepElement(Element element)
        {
            this.element = element;
            this.elementType = element.GetElementType();
        }

        /// <summary>
        /// Gets the element incision type.
        /// </summary>
        public MepElementGeometryType GeometryType
        {
            get { return this.geometryType; }
        }

        /// <summary>
        /// Gets the revit Element.
        /// </summary>
        public Element Element
        {
            get { return this.element; }
        }
    }
}