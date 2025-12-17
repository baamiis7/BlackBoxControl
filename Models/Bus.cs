using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BlackBoxControl.Models
{
    public class Bus : INotifyPropertyChanged
    {
        private int _busNumber;
        private string _busName;
        private string _busType;
        private int _numberOfNodes;
        private ObservableCollection<BusNode> _nodes;

        public Bus()
        {
            Nodes = new ObservableCollection<BusNode>();
        }

        public int BusNumber
        {
            get => _busNumber;
            set
            {
                _busNumber = value;
                OnPropertyChanged();
            }
        }

        public string BusName
        {
            get => _busName;
            set
            {
                _busName = value;
                OnPropertyChanged();
            }
        }

        public string BusType
        {
            get => _busType;
            set
            {
                _busType = value;
                OnPropertyChanged();
            }
        }

        public int NumberOfNodes
        {
            get => _numberOfNodes;
            set
            {
                _numberOfNodes = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<BusNode> Nodes
        {
            get => _nodes;
            set
            {
                _nodes = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
