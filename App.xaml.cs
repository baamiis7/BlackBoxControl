using System.Windows;
using BlackBoxControl.Services;
using BlackBoxControl.ViewModels;

namespace BlackBoxControl
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var projectService = new ProjectService();
            var mainViewModel = new MainViewModel(projectService);
            var mainWindow = new MainWindow(mainViewModel);
            mainWindow.Show();
        }
    }
}
