using System.Windows;
using BlackBoxControl.ViewModels;

namespace BlackBoxControl.Views
{
    public partial class UploadConfigurationDialog : Window
    {
        public UploadConfigurationDialog(UploadConfigurationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.RequestClose += () => this.Close();
        }
    }
}
