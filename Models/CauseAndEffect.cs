using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BlackBoxControl.Models
{
    public enum LogicGate
    {
        OR,
        AND,
        XOR
    }

    public class CauseAndEffect : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private LogicGate _logicGate;
        private bool _isEnabled;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public LogicGate LogicGate
        {
            get => _logicGate;
            set
            {
                _logicGate = value;
                OnPropertyChanged(nameof(LogicGate));
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public string Status => IsEnabled ? "Active" : "Inactive";

        public ObservableCollection<CauseInput> Inputs { get; set; }
        public ObservableCollection<EffectOutput> Outputs { get; set; }

        public CauseAndEffect()
        {
            LogicGate = LogicGate.OR;
            IsEnabled = true;
            Inputs = new ObservableCollection<CauseInput>();
            Outputs = new ObservableCollection<EffectOutput>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
