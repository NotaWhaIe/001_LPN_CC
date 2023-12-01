// <copyright file="CommandHandler.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Windows.Input;

#nullable enable
namespace Strana.Revit.NavisReportViewer.ViewModels.Base
{
    /// <summary> Base command handler. </summary>
    public class CommandHandler : ICommand
    {
        private readonly Action<object> action;
        private readonly Func<object, bool> canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class. Creates instance of the command handler.
        /// </summary>
        /// <param name="action"> This is action in app.</param>
        /// <param name="canExecute"> Are can we execute action or not.</param>
        public CommandHandler(Action<object> action, Func<object, bool> canExecute)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.canExecute = canExecute ?? (x => true);
        }

        /// <summary> Wires CanExecuteChanged event </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Refrash command.
        /// </summary>
        public static void Refresh()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>  Forcess checking if execute is allowed. </summary>
        /// <returns> Are can execute command. </returns>
        /// <param name="parameter"> Parameter using hor executing commands. </param>
        public bool CanExecute(object? parameter)
        {
            return parameter != null && this.canExecute(parameter);
        }

        /// <summary>
        /// Execute this command.
        /// </summary>
        /// <param name="parameter">Command parameter.</param>
        public void Execute(object? parameter)
        {
            if (parameter is not null)
            {
                this.action?.Invoke(parameter);
            }
        }
    }
}
