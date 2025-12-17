using System;
using System.Windows;
using System.Windows.Input;
using BlackBoxControl.Models;

namespace BlackBoxControl.ViewModels
{
    public class SendTextOutputViewModel : ViewModelBase
    {
        private SendTextOutput _output;
        private readonly CauseAndEffectViewModel _parentViewModel;

        public SendTextOutput Output
        {
            get => _output;
            set
            {
                _output = value;
                OnPropertyChanged(nameof(Output));
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public SendTextOutputViewModel(SendTextOutput output, CauseAndEffectViewModel parentViewModel)
        {
            Output = output;
            _parentViewModel = parentViewModel;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete);
        }

        private void Save()
        {
            _parentViewModel.RebuildTreeChildren();
            MessageBox.Show("SMS notification updated!", "Success", MessageBoxButton.OK);
        }

        private void Cancel()
        {
            // Optionally reload from saved state
        }

        private void Delete()
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete SMS notification to {Output.PhoneNumber}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _parentViewModel.SendTextOutputs.Remove(Output);
                _parentViewModel.CauseEffect.Outputs.Remove(Output);
                _parentViewModel.RebuildTreeChildren();
                _parentViewModel.NotifySelectionChanged();
                MessageBox.Show("SMS notification deleted!", "Success", MessageBoxButton.OK);
            }
        }
    }
}