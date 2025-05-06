using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace VesselManagementClient.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = false;
            if (value is bool b)
            {
                boolValue = b;
            }

            // Inverse visibility if parameter is "Inverse"
            if (parameter != null && parameter.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true)
            {
                boolValue = !boolValue;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ConvertBack is typically not needed for visibility bindings
            if (value is Visibility visibility)
            {
                bool boolValue = visibility == Visibility.Visible;
                if (parameter != null && parameter.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true)
                {
                    boolValue = !boolValue;
                }
                return boolValue;
            }
            return false;
        }
    }
}
