// <copyright file="HoleTaskStatus.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

namespace Strana.RevitPlugins.BaseElements
{
    /// <summary>
    /// This is a Status enum for HoleTask.
    /// </summary>
    public enum HoleTaskStatus
    {
        /// <summary>
        /// It is HoleTask need was be seen by engineer.
        /// </summary>
        Actual,

        /// <summary>
        /// This TaskHole status for not actual, this element not interesting for engineer.
        /// </summary>
        NotActual,

        /// <summary>
        /// This HoleTask for element who dont intersect with other element.
        /// </summary>
        WorkedOut,
    }
}
