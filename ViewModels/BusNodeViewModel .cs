using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using BlackBoxControl.Models;
using Microsoft.Win32;

namespace BlackBoxControl.ViewModels
{
    public class BusNodeViewModel : TreeNodeViewModel, INotifyPropertyChanged
    {
        private BusNode _node;

        public BusNode Node
        {
            get => _node;
            set
            {
                _node = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(Icon));
            }
        }

        public string DisplayName
        {
            get => Node?.Name ?? "Node";
            set
            {
                if (Node != null)
                {
                    Node.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Icon
        {
            get => Node?.ImagePath ?? "/Assets/Nodes/default.png";
            set
            {
                if (Node != null)
                {
                    Node.ImagePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseImageCommand { get; }

        public BusNodeViewModel(BusNode node)
        {
            Node = node;
            NodeType = TreeNodeType.BusNode;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            BrowseImageCommand = new RelayCommand(BrowseImage);
        }

        private void Save()
        {
            MessageBox.Show("Node saved successfully.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Cancel()
        {
            MessageBox.Show("Changes cancelled.", "Cancel", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BrowseImage()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Select Node Icon"
            };

            if (dlg.ShowDialog() == true)
            {
                Node.ImagePath = dlg.FileName;
                OnPropertyChanged(nameof(Icon));
            }
        }
    }
}
