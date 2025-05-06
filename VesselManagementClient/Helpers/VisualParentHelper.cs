using System.Windows;
using System.Windows.Media;

namespace VesselManagementClient.Helpers
{
    public static class VisualParentHelper
    {
        public static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T? parent = parentObject as T;
            return parent ?? FindVisualParent<T>(parentObject);
        }
    }
}
