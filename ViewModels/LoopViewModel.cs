using BlackBoxControl.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace BlackBoxControl.ViewModels
{
    public class LoopViewModel : TreeNodeViewModel, INotifyPropertyChanged
    {
        private Loop _loop;

        public Loop Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        // This is what the TreeView shows for the loop
        public string DisplayName => Loop?.LoopName ?? "Loop";

        // Loop icon (optional)
        public string Icon => "🔁";

        public ObservableCollection<string> Protocols { get; set; } =
            new ObservableCollection<string>
            {
                "None",
                "Argus",
                "Hochiki",
                "Apollo",
                "System Sensor"
            };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public LoopViewModel(Loop loop)
        {
            Loop = loop;
            NodeType = TreeNodeType.Loop;

            // EVERY node must have Children for the TreeView
            Children = new ObservableCollection<TreeNodeViewModel>();

            // Load devices already in the model
            if (Loop?.Devices != null)
            {
                foreach (var dev in Loop.Devices)
                {
                    Children.Add(new LoopDeviceViewModel(dev));
                }
            }

            SaveCommand = new RelayCommand(SaveChanges);
            CancelCommand = new RelayCommand(CancelChanges);
        }

        private void SaveChanges()
        {
            MessageBox.Show("Loop saved successfully!", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelChanges()
        {
            MessageBox.Show("Changes canceled.", "Cancel", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
