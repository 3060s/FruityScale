using System.Windows;

namespace FruityScale.Views
{
    public partial class Dashboard : Window
    {
        private readonly DashboardViewModel _viewModel;

        public Dashboard(DashboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}