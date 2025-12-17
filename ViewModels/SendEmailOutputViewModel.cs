using System;
using System.Windows;
using System.Windows.Input;
using BlackBoxControl.Models;

namespace BlackBoxControl.ViewModels
{
    public class SendEmailOutputViewModel : ViewModelBase
    {
        private SendEmailOutput _output;
        private readonly CauseAndEffectViewModel _parentViewModel;

        public SendEmailOutput Output
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

        public SendEmailOutputViewModel(SendEmailOutput output, CauseAndEffectViewModel parentViewModel)
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
            MessageBox.Show("Email notification updated!", "Success", MessageBoxButton.OK);
        }

        private void Cancel()
        {
            // Optionally reload from saved state
        }

        private void Delete()
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete email notification to {Output.EmailAddress}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _parentViewModel.SendEmailOutputs.Remove(Output);
                _parentViewModel.CauseEffect.Outputs.Remove(Output);
                _parentViewModel.RebuildTreeChildren();
                _parentViewModel.NotifySelectionChanged();
                MessageBox.Show("Email notification deleted!", "Success", MessageBoxButton.OK);
            }
        }
    }
}