﻿using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ChatterBox.Client.Presentation.Shared.MVVM
{
    /// <summary>
    ///     The BindableBase class of the MVVM pattern.
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        ///     Returns the name of a property identified by a lambda expression.
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="propertyExpression">A lambda expression selecting the property.</param>
        /// <returns>The name of the property accessed by expression. </returns>
        private static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null) throw new ArgumentNullException(nameof(propertyExpression));
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null) throw new ArgumentException(nameof(propertyExpression));
            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null) throw new ArgumentException(nameof(propertyExpression));
            if (propertyInfo.GetMethod.IsStatic) throw new ArgumentException(nameof(propertyExpression));
            return memberExpression.Member.Name;
        }

        /// <summary>
        ///     Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Raises this object's PropertyChanged event.
        /// </summary>
        /// <typeparam name="T">The type of the property that has a new value.</typeparam>
        /// <param name="propertyExpression">A Lambda expression representing the property that has a new value.</param>
        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            OnPropertyChanged(GetPropertyName(propertyExpression));
        }

        /// <summary>
        ///     Checks if a property already matches a desired value. Sets the property and notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.</param>
        /// <returns>True if the value was changed, false if the existing value matched the desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}