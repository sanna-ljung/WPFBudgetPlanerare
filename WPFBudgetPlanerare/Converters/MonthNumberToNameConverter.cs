using System;
using System.Globalization;
using System.Windows.Data;

namespace WPFBudgetPlanerare.Converters
{
    public class MonthNumberToNameConverter : IValueConverter
    {
        private readonly string[] _monthNames = new[]
        {
            "Januari", "Februari", "Mars", "April", "Maj", "Juni",
            "Juli", "Augusti", "September", "Oktober", "November", "December"
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not int)
                return "-";

            if (value is int month && month >= 1 && month <= 12)
                return $"{month:00}. {_monthNames[month - 1]}";
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
