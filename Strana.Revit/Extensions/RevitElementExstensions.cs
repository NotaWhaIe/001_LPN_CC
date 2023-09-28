// <copyright file="RevitElementExstensions.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;

namespace Strana.Revit.Extension
{
    /// <summary>
    /// This class conteins exstensions for revit element.
    /// </summary>
    public static class RevitElementExstensions
    {
        /// <summary>
        /// Return elementType for element.
        /// </summary>
        /// <param name="element">Revit base element.</param>
        /// <returns>return element type for element.</returns>
        public static ElementType GetElementType(this Element element)
        {
            Document doc = element.Document;
            ElementId elementTypeId = element.GetTypeId();
            ElementType elementType = doc.GetElement(elementTypeId) as ElementType;

            return elementType;
        }
    }
}
