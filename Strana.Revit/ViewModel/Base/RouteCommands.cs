// <copyright file="RouteCommands.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Windows.Input;

#nullable enable
namespace Strana.Revit.NavisReportViewer.ViewModels.Base
{
    /// <summary> Base Route command. </summary>
    public class RouteCommands : ICommand
    {
        // The action to execute.
        private readonly Action? mAction = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteCommands"/> class.
        /// Default constructor. </summary>
        /// <param name="action">Action to execute. </param>
        public RouteCommands(Action? action)
        {
            this.mAction = action;
        }

        /// <summary> Can Execute Changed. </summary>
        public event EventHandler? CanExecuteChanged = (sender, e) => { };

        /// <summary>
        /// can execute bool.
        /// </summary>
        /// <param name="parameter">sender object.</param>
        /// <returns>true.</returns>
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        /// <summary>  Defines the methods to be calles when the command is invoked. </summary>
        /// <param name="parameter">sender object.</param>
        public void Execute(object? parameter)
        {
            this.mAction?.Invoke();
        }
    }
}
