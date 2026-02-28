using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlackBoxControl.Models;
using BlackBoxControl.Properties;
using BlackBoxControl.ViewModels;

namespace BlackBoxControl
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void LoadTheme(string themeFileName)
        {
            Application.Current.Resources.MergedDictionaries.Clear();

            var theme = new ResourceDictionary
            {
                Source = new System.Uri(
                    $"pack://application:,,,/BlackBoxControl;component/Themes/{themeFileName}",
                    System.UriKind.Absolute)
            };
            Application.Current.Resources.MergedDictionaries.Add(theme);

            Settings.Default.DefaultTheme = themeFileName;
            Settings.Default.Save();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel viewModel)
                viewModel.SelectedNode = e.NewValue;
        }

        private void Device_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                if (sender is System.Windows.Controls.StackPanel stackPanel &&
                    stackPanel.DataContext is LoopDevice device)
                {
                    DragDrop.DoDragDrop(stackPanel, device, DragDropEffects.All);
                }
            }
        }

        private void Device_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(typeof(LoopDevice))
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private void Device_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (e.Data.GetDataPresent(typeof(LoopDevice)) &&
                e.Data.GetData(typeof(LoopDevice)) is LoopDevice droppedDevice &&
                DataContext is MainViewModel viewModel)
            {
                viewModel.AddDeviceToSelectedItem(droppedDevice);
            }
        }

        private void Device_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel) return;
            if (sender is System.Windows.Controls.Image image &&
                image.DataContext is LoopDevice device)
            {
                viewModel.AddDeviceToSelectedItem(device);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void btnMaximise_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Maximized;

        private void btnClose_Click(object sender, RoutedEventArgs e) =>
            Application.Current.Shutdown();
    }
}
