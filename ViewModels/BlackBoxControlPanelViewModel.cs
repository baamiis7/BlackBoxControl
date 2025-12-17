using global::BlackBoxControl.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace BlackBoxControl.ViewModels
{
    public class BlackBoxControlPanelViewModel : INotifyPropertyChanged
    {
        private BlackBoxControlPanel _panel;
        public BlackBoxControlPanel Panel
        {
            get => _panel;
            set
            {
                _panel = value;
                OnPropertyChanged();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private ObservableCollection<object> _children;

        // Available input actions for ComboBox
        public ObservableCollection<string> Protocol { get; set; } = new ObservableCollection<string>
        {
            "None",
            "Argus",
            "Hochiki",
            "Apollo",
            "System Sensor"
        };

        // Save and Cancel commands
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Constructor
        public BlackBoxControlPanelViewModel(BlackBoxControlPanel panel)
        {
            Panel = panel;
            Children = new ObservableCollection<object>();

            SaveCommand = new RelayCommand(SaveChanges);
            CancelCommand = new RelayCommand(CancelChanges);

            // Build the tree structure
            BuildTreeStructure();
        }

        private void BuildTreeStructure()
        {
            // Add Loops container
            var loopsContainer = new TreeNodeViewModel
            {
                DisplayName = "Loops",
                Icon = "🔄",
                NodeType = TreeNodeType.LoopsContainer,
                Children = new ObservableCollection<TreeNodeViewModel>()
            };

            // Add each loop
            if (Panel.Loops != null)
            {
                foreach (var loop in Panel.Loops)
                {
                    var loopVM = new LoopViewModel(loop);
                    loopsContainer.Children.Add(loopVM);
                }
            }

            Children.Add(loopsContainer);

            // Add Busses container
            var bussesContainer = new TreeNodeViewModel
            {
                DisplayName = "Busses",
                Icon = "🔌",
                NodeType = TreeNodeType.BussesContainer,
                Children = new ObservableCollection<TreeNodeViewModel>()
            };

            // Add default busses (Bus 1 and Bus 2)
            for (int i = 1; i <= 2; i++)
            {
                var bus = new Bus
                {
                    BusNumber = i,
                    BusName = "Bus " + i,
                    BusType = "RS485",
                    Nodes = new ObservableCollection<BusNode>()
                };
                var busVM = new BusViewModel(bus);
                bussesContainer.Children.Add(busVM);
            }

            Children.Add(bussesContainer);

            // Add Cause and Effects container
            var ceContainer = new TreeNodeViewModel
            {
                DisplayName = "Cause and Effects",
                Icon = "⚡",
                NodeType = TreeNodeType.CauseEffectsContainer,
                Children = new ObservableCollection<TreeNodeViewModel>()
            };

            Children.Add(ceContainer);
        }

        // Save logic
        private void SaveChanges()
        {
            // Logic to save changes, e.g., write to database or update the view model
            MessageBox.Show("Changes saved successfully!", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Cancel logic
        private void CancelChanges()
        {
            // Logic to discard changes or reset fields
            MessageBox.Show("Changes canceled.", "Cancel", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public ObservableCollection<object> Children
        {
            get { return _children; }
            set
            {
                _children = value;
                OnPropertyChanged(nameof(Children));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}