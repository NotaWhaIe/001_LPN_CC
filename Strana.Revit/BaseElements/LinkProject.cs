// <copyright file="LinkProject.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Strana.Revit.Exstensions;

namespace Strana.Revit.BaseElements
{
    /// <summary> Class for program logic. </summary>
    public class LinkProject
    {
        private readonly RevitLinkInstance linkProject;

        private readonly List<IntersectingElement> intersectingElements;

        private readonly List<MepElement> mepElements;

        // communications lines in intersection direction.
        private readonly List<Line> intersectingLines;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkProject"/> class.
        /// This class contains revit link.
        /// </summary>
        /// <param name="linkProject">It is standart revit link.</param>
        /// <param name="mepElements">This is all mep elements used in creating Hole Tasks.</param>
        public LinkProject(RevitLinkInstance linkProject, ref List<MepElement> mepElements)
        {
            this.linkProject = linkProject;

            this.intersectingElements = linkProject.AllIntersectedElements()
                .Select(el => new IntersectingElement(el))
                .ToList();

            this.mepElements = mepElements;
        }

        /// <summary> Gets revit link Name.</summary>
        public string Name => this.linkProject.Name;
    }
}
