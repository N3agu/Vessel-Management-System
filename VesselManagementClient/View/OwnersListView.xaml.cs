using System.Windows.Controls;
using System.Windows.Media;
using VesselManagementClient.Helpers;

namespace VesselManagementClient.View
{
    public partial class OwnersListView : UserControl
    {
        public OwnersListView()
        {
            InitializeComponent();
        }

        private void ListView_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null) return;

            var hitTestResult = VisualTreeHelper.HitTest(listView, e.GetPosition(listView));
            if (hitTestResult?.VisualHit == null)
            {
                listView.SelectedItem = null;
                return;
            }

            var clickedItem = VisualParentHelper.FindVisualParent<ListViewItem>(hitTestResult.VisualHit);

            if (clickedItem == null)
            {
                listView.SelectedItem = null;
            }
        }

    }
}
