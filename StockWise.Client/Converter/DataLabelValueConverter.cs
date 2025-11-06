using Microsoft.Maui.Controls;
using StockWise.Client.Models;
using System.Globalization;

namespace StockWise.Client.Converter
{
    public class DataLabelValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is CategoryChartData categoryChartData)
            {
                switch (parameter?.ToString())
                {
                    case "Title":
                        return categoryChartData.Title;

                    case "Count":
                        return categoryChartData.Count;
                }
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
