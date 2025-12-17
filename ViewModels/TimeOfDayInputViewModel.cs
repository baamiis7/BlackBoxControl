using System;
using System.Windows;
using System.Windows.Input;
using BlackBoxControl.Models;

namespace BlackBoxControl.ViewModels
{
    public class TimeOfDayInputViewModel : ViewModelBase
    {
        private TimeOfDayInput _input;
        private readonly CauseAndEffectViewModel _parentViewModel;

        public TimeOfDayInput Input
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

        public TimeOfDayInputViewModel(TimeOfDayInput input, CauseAndEffectViewModel parentViewModel)
        {
            Input = input;
            _parentViewModel = parentViewModel;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete);
        }

        private void Save()
        {
            // Update is automatic via binding
            _parentViewModel.RebuildTreeChildren();
            MessageBox.Show("Time of Day schedule updated!", "Success", MessageBoxButton.OK);
        }

        private void Cancel()
        {
            // Optionally reload from saved state
        }

        private void Delete()
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this Time of Day schedule?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _parentViewModel.TimeOfDayInputs.Remove(Input);
                _parentViewModel.CauseEffect.Inputs.Remove(Input);
                _parentViewModel.RebuildTreeChildren();
                _parentViewModel.NotifySelectionChanged();
                MessageBox.Show("Time of Day schedule deleted!", "Success", MessageBoxButton.OK);
            }
        }
    }
}