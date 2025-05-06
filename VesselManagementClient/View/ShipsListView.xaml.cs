using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VesselManagementClient.Helpers;

namespace VesselManagementClient.View
{
    public partial class ShipsListView : UserControl
    {
        public ShipsListView()
        {
            InitializeComponent();
        }

        private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            var hitTestResult = VisualTreeHelper.HitTest(dataGrid, e.GetPosition(dataGrid));
            if (hitTestResult?.VisualHit == null)
            {
                dataGrid.SelectedItem = null;
                return;
            }

            // Use the helper class here
            var clickedRow = VisualParentHelper.FindVisualParent<DataGridRow>(hitTestResult.VisualHit);

            if (clickedRow == null)
            {
                dataGrid.SelectedItem = null;
            }
            else
            {
                if (!clickedRow.IsSelected)
                {
                    clickedRow.IsSelected = true;
                }
            }
        }
    }
}
