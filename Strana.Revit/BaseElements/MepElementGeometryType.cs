// <copyright file="MepElementGeometryType.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

namespace Strana.Revit.BaseElements
{
    /// <summary>
    /// This type of geometry mep element. Can be rounded and rectangle.
    /// </summary>
    public enum MepElementGeometryType
    {
        /// <summary>
        /// Mep element incision type is round.
        /// </summary>
        Round,

        /// <summary>
        /// Mep element incision type is rectangle.
        /// </summary>
        Reactangle,
    }
}
