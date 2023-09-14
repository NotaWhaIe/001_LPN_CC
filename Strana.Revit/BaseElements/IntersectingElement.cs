// <copyright file="IntersectingElement.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;
using Strana.Revit.Exstensions;

namespace Strana.Revit.BaseElements
{
    /// <summary>
    /// This is a class for all elements, who intersects with communications (walls, floors).
    /// </summary>
    public class IntersectingElement
    {
        private readonly Element communicationElement;

        private readonly ElementType communicationType;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntersectingElement"/> class.
        /// </summary>
        /// <param name="communicationElement">This is communicaion. </param>
        /// <see cref="MepElement"/>
        public IntersectingElement(Element communicationElement)
        {
            this.communicationElement = communicationElement;
            this.communicationType = communicationElement.GetElementType();
        }

        /// <summary>
        /// Gets this communication.
        /// </summary>
        public Element CommunicationElement
        {
            get { return this.communicationElement; }
        }
    }
}