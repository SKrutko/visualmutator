﻿namespace VisualMutator.Views.Converters
{
    #region

    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    #endregion

    [ValueConversion(typeof(Freezable), typeof(Freezable))]
    internal class CloningConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Freezable)value).Clone();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}