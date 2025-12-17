using System;
using System.Windows;
using System.Windows.Input;
using BlackBoxControl.Models;

namespace BlackBoxControl.ViewModels
{
    public class SendApiOutputViewModel : ViewModelBase
    {
        private SendApiOutput _output;
        private readonly CauseAndEffectViewModel _parentViewModel;

        public SendApiOutput Output
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

        public SendApiOutputViewModel(SendApiOutput output, CauseAndEffectViewModel parentViewModel)
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
            MessageBox.Show("API request updated!", "Success", MessageBoxButton.OK);
        }

        private void Cancel()
        {
            // Optionally reload from saved state
        }

        private void Delete()
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete API request to {Output.ApiUrl}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _parentViewModel.SendApiOutputs.Remove(Output);
                _parentViewModel.CauseEffect.Outputs.Remove(Output);
                _parentViewModel.RebuildTreeChildren();
                _parentViewModel.NotifySelectionChanged();
                MessageBox.Show("API request deleted!", "Success", MessageBoxButton.OK);
            }
        }
    }
}