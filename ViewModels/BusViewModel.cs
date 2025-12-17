using BlackBoxControl.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace BlackBoxControl.ViewModels
{
    public class BusViewModel : TreeNodeViewModel
    {
        private Bus _bus;

        public Bus Bus
        {
            get => _bus;
            set
            {
                _bus = value;
                OnPropertyChanged();
            }
        }

        public BusViewModel(Bus bus) : base()
        {
            Bus = bus;
            DisplayName = bus.BusName;
            Icon = "🔌";
            NodeType = TreeNodeType.Bus;
            Children = new ObservableCollection<TreeNodeViewModel>();

            // Build initial tree children from existing nodes
            RebuildTreeChildren();

            // Subscribe to collection changes
            if (Bus.Nodes != null)
            {
                Bus.Nodes.CollectionChanged += (s, e) =>
                {
                    RebuildTreeChildren();
                };
            }
        }

        /// <summary>
        /// Rebuilds the tree children to reflect the current nodes in the bus
        /// </summary>
        public void RebuildTreeChildren()
        {
            Children.Clear();

            if (Bus?.Nodes != null)
            {
                foreach (var node in Bus.Nodes)
                {
                    var nodeVM = new BusNodeViewModel(node);
                    Children.Add(nodeVM);
                }
            }
        }
    }
}