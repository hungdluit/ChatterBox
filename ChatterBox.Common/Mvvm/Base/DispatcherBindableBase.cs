﻿using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Core;

namespace ChatterBox.Common.Mvvm.Base
{
    /// <summary>
    /// Provides ability to run the UI updates in UI thread.
    /// </summary>
    internal abstract class DispatcherBindableBase : BindableBase
    {
        // The UI dispatcher
        private readonly CoreDispatcher _uiDispatcher;

        /// <summary>
        /// Creates a DispatcherBindableBase instance.
        /// </summary>
        /// <param name="uiDispatcher">Core event message dispatcher.</param>
        protected DispatcherBindableBase(CoreDispatcher uiDispatcher)
        {
            _uiDispatcher = uiDispatcher;
        }

        /// <summary>
        /// Overrides the BindableBase's OnPropertyChanged method.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected override void OnPropertyChanged(string propertyName)
        {
            RunOnUiThread(() => base.OnPropertyChanged(propertyName));
        }

        /// <summary>
        /// Schedules the provided callback on the UI thread from a worker thread, and
        //  returns the results asynchronously.
        /// </summary>
        /// <param name="fn">The function to execute</param>
        protected void RunOnUiThread(Action fn)
        {
            _uiDispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(fn));
        }
    }
}
