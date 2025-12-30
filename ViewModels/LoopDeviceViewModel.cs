using global::BlackBoxControl.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace BlackBoxControl.ViewModels
{
    public class LoopDeviceViewModel : TreeNodeViewModel, INotifyPropertyChanged
    {
        private LoopDevice _device;

        public LoopDevice Device
        {
            get => _device;
            set
            {
                _device = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NodeName));
                OnPropertyChanged(nameof(IconPath));
            }
        }

        // WHAT THE TREEVIEW DISPLAYS
        // WHAT THE TREEVIEW DISPLAYS
        public string NodeName
        {
            get
            {
                // Simply return the device type
                return Device?.Type ?? "Unknown Device";
            }
        }


        // ICON DISPLAYED IN TREEVIEW
        public string IconPath =>
            string.IsNullOrWhiteSpace(Device?.ImagePath)
                ? "/Assets/Icons/default-device.png"
                : Device.ImagePath;

        public LoopDeviceViewModel(LoopDevice device)
        {
            Device = device;
            NodeType = TreeNodeType.Device;  // IMPORTANT!
            SaveCommand = new RelayCommand(SaveChanges);
            CancelCommand = new RelayCommand(CancelChanges);
        }

        // Available input actions for ComboBox
        public ObservableCollection<string> InputActions { get; set; } =
            new ObservableCollection<string>
            {
                "None",
                "Activate",
                "Deactivate",
                "Alarm",
                "Reset"
            };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private void SaveChanges()
        {
            MessageBox.Show(
                "Changes saved successfully!",
                "Save",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void CancelChanges()
        {
            MessageBox.Show(
                "Changes canceled.",
                "Cancel",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other
    /// objects by invoking delegates. The default return value for the CanExecute
    /// method is 'true'. This class does not allow you to accept command parameters.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// A generic command whose sole purpose is to relay its functionality
    /// to other objects by invoking delegates. The default return value for the CanExecute
    /// method is 'true'.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            if (parameter == null && typeof(T).IsValueType)
                return _canExecute(default(T));

            return _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}


