// <copyright file="BaseViewModel.cs" company="Strana">
// Copyright (c) Strana. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.ComponentModel;

#nullable enable
namespace Strana.Revit.NavisReportViewer.ViewModels.Base
{
    /// <summary> Base view model. </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary> Occurs when a property value changes. </summary>
        public event PropertyChangedEventHandler? PropertyChanged = (sender, e) => { };

        /// <summary>  Call this method to raise <see cref="PropertyChanged"/> event. </summary>
        /// <param name="name">Event when parameter was edited.</param>
        public void OnPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
