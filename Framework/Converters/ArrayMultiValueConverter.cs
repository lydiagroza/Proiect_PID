using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Avalonia.Data.Converters;

namespace Framework.Converters
{
    public class ArrayMultiValueConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.ToArray();
        }
    }
}
