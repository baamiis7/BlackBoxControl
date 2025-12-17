using System;
using System.Windows;
using System.Windows.Input;
using BlackBoxControl.Models;

namespace BlackBoxControl.ViewModels
{
    public class ReceiveApiInputViewModel : ViewModelBase
    {
        private ReceiveApiInput _input;
        private readonly CauseAndEffectViewModel _parentViewModel;

        public ReceiveApiInput Input
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

        public ReceiveApiInputViewModel(ReceiveApiInput input, CauseAndEffectViewModel parentViewModel)
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
            MessageBox.Show("API webhook input updated!", "Success", MessageBoxButton.OK);
        }

        private void Cancel()
        {
            // Optionally reload from saved state
        }

        private void Delete()
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete API webhook listening at {Input.ListenUrl}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _parentViewModel.ReceiveApiInputs.Remove(Input);
                _parentViewModel.CauseEffect.Inputs.Remove(Input);
                _parentViewModel.RebuildTreeChildren();
                _parentViewModel.NotifySelectionChanged();
                MessageBox.Show("API webhook input deleted!", "Success", MessageBoxButton.OK);
            }
        }
    }
}