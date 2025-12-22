using System.Windows;
using BlackBoxControl.ViewModels;

namespace BlackBoxControl.Views
{
    public partial class DownloadConfigurationDialog : Window
    {
        public DownloadConfigurationDialog(DownloadConfigurationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.RequestClose += () => this.Close();
        }
    }
}
