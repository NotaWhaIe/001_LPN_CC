// <copyright file="CurrentProject.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Strana.Revit.Exstensions;

namespace Strana.Revit.BaseElements
{
    /// <summary>
    /// This Base project model.
    /// </summary>
    public class CurrentProject
    {
        private readonly Document doc;

        private readonly List<LinkProject> linksProjects;

        private List<MepElement> communications;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentProject"/> class.
        /// default constructor.
        /// </summary>
        /// <param name="doc">This revit current doc.</param>
        public CurrentProject(Document doc)
        {
            this.doc = doc;

            this.communications = doc.Communications()
                .Select(el => new MepElement(el))
                .ToList();

            this.linksProjects = doc.GetAllRevitLinkInstances()
                .Select(el => new LinkProject(el, ref this.communications))
                .ToList();
        }
    }
}
