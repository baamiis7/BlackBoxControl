using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BlackBoxControl.Models
{
    public class BusNode : INotifyPropertyChanged
    {
        private int _nodeNumber;
        private string _name;
        private int _address;
        private string _locationText;
        private string _imagePath;
        private ObservableCollection<BusNodeIO> _inputs;
        private ObservableCollection<BusNodeIO> _outputs;

        public BusNode()
        {
            Inputs = new ObservableCollection<BusNodeIO>();
            Outputs = new ObservableCollection<BusNodeIO>();
        }

        public int NodeNumber
        {
            get => _nodeNumber;
            set { _nodeNumber = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public int Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        public string LocationText
        {
            get => _locationText;
            set { _locationText = value; OnPropertyChanged(); }
        }

        public string ImagePath
        {
            get => _imagePath;
            set { _imagePath = value; OnPropertyChanged(); }
        }

        public ObservableCollection<BusNodeIO> Inputs
        {
            get => _inputs;
            set { _inputs = value; OnPropertyChanged(); }
        }

        public ObservableCollection<BusNodeIO> Outputs
        {
            get => _outputs;
            set { _outputs = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

    // Add this new class for Bus Node Input/Output
    public class BusNodeIO : INotifyPropertyChanged
    {
        private string _type;
        private string _description;

        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}