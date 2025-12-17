using System;
using System.Windows;
using System.Windows.Input;
using BlackBoxControl.Models;

namespace BlackBoxControl.ViewModels
{
    public class DateTimeInputViewModel : ViewModelBase
    {
        private DateTimeInput _input;
        private readonly CauseAndEffectViewModel _parentViewModel;

        public DateTimeInput Input
        {
            get => _input;
            set
            {
                _input = value;
                OnPropertyChanged(nameof(Input));
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public DateTimeInputViewModel(DateTimeInput input, CauseAndEffectViewModel parentViewModel)
        {
            Input = input;
            _parentViewModel = parentViewModel;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete);
        }

        private void Save()
        {
            _parentViewModel.RebuildTreeChildren();
            MessageBox.Show("Date/Time trigger updated!", "Success", MessageBoxButton.OK);
        }

        private void Cancel()
        {
            // Optionally reload from saved state
        }

        private void Delete()
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this Date/Time trigger?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _parentViewModel.DateTimeInputs.Remove(Input);
                _parentViewModel.CauseEffect.Inputs.Remove(Input);
                _parentViewModel.RebuildTreeChildren();
                _parentViewModel.NotifySelectionChanged();
                MessageBox.Show("Date/Time trigger deleted!", "Success", MessageBoxButton.OK);
            }
        }
    }
}