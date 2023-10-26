// <copyright file="HoleTaskFamilyLoader.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;

namespace Strana.Revit.HoleTask.Utils
{
    /// <summary>
    /// This class contains metod to check are needed families in project.
    /// </summary>
    public class HoleTaskFamilyLoader
    {
        private readonly Document doc;
        private FamilySymbol floorFamilySymbol;
        private FamilySymbol wallFamilySymbol;

        /// <summary>
        /// Initializes a new instance of the <see cref="HoleTaskFamilyLoader"/> class.
        /// Load families into a Revit document.
        /// </summary>
        /// <param name="doc"><seealso cref="Document"/></param>
        public HoleTaskFamilyLoader(Document doc)
        {
            this.doc = doc;
        }

        /// <summary>
        /// Get familySymbol for floor from repository or carrent document.
        /// </summary>
        public FamilySymbol FloorFamilySymbol
        {
            get
            {
                if (this.floorFamilySymbol == null)
                {
                    var familySymbol = new FilteredElementCollector(this.doc)
                                         .OfClass(typeof(FamilySymbol))
                                         .WhereElementIsElementType()
                                         .Cast<FamilySymbol>();
                    foreach (var checkFamilySymbol in familySymbol)
                    {
                        if (checkFamilySymbol.Name != Path.GetFileNameWithoutExtension(Confing.Default.floorHoleTaskPath))
                        {
                            continue;
                        }

                        this.floorFamilySymbol = checkFamilySymbol;
                        break;
                    }

                    if (this.floorFamilySymbol == null)
                    {
                        this.doc.LoadFamilySymbol(
                            Confing.Default.floorHoleTaskPath,
                            Path.GetFileNameWithoutExtension(Confing.Default.floorHoleTaskPath),
                            out this.floorFamilySymbol);
                    }
                }

                this.floorFamilySymbol.Activate();

                return this.floorFamilySymbol;
            }
        }

        /// <summary>
        /// Get familySymbol for wall from repository or carrent document.
        /// </summary>
        public FamilySymbol WallFamilySymbol
        {
            get
            {
                if (this.wallFamilySymbol == null)
                {
                    var familySymbol = new FilteredElementCollector(this.doc)
                                         .OfClass(typeof(FamilySymbol))
                                         .WhereElementIsElementType()
                                         .Cast<FamilySymbol>();
                    foreach (var checkFamilySymbol in familySymbol)
                    {
                        if (checkFamilySymbol.Name != Path.GetFileNameWithoutExtension(Confing.Default.wallHoleTaskPath))
                        {
                            continue;
                        }

                        this.wallFamilySymbol = checkFamilySymbol;
                        break;
                    }

                    if (this.wallFamilySymbol == null)
                    {
                        this.doc.LoadFamilySymbol(
                            Confing.Default.wallHoleTaskPath,
                            Path.GetFileNameWithoutExtension(Confing.Default.wallHoleTaskPath),
                            out this.wallFamilySymbol);
                    }
                }

                this.wallFamilySymbol.Activate();

                return this.wallFamilySymbol;
            }
        }
    }
}