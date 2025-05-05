using System.Windows.Controls;
using System.Windows.Data;

namespace VesselManagementClient.View
{
    public partial class ShipsListView : UserControl
    {
        public ShipsListView()
        {
            InitializeComponent();
        }

        public class BooleanNegationConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return !(bool)value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return !(bool)value;
            }
        }
    }
}
