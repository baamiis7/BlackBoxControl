using System;
using System.ComponentModel;

namespace BlackBoxControl.Models
{
    public class SelectableDevice : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string DeviceId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string LocationText { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;

        public Action? SelectedChanged { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                SelectedChanged?.Invoke();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
