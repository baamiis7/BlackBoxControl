using System.Collections.ObjectModel;
using System.ComponentModel;
using BlackBoxControl.Models;

namespace BlackBoxControl.ViewModels
{
    /// <summary>
    /// Types of tree nodes
    /// </summary>
    public enum TreeNodeType
    {
        BlackBoxControlPanel,
        LoopsContainer,
        Loop,
        BussesContainer,
        Bus,
        BusNode,

        CauseEffectsContainer,
        CauseEffect,

        // NEW TYPES for the C&E hierarchy
        CauseInputsContainer,
        CauseOutputsContainer,
        CauseDeviceNode,

        Device,
        CauseEffectInput,
        CauseEffectOutput
    }


    /// <summary>
    /// Base class for tree view container nodes (Loops, Busses, C&E containers)
    /// </summary>
    public class TreeNodeViewModel : ViewModelBase
    {
        private string _displayName;
        private string _icon;
        private TreeNodeType _nodeType;
        private ObservableCollection<TreeNodeViewModel> _children;
        private bool _isExpanded;
        private object _tag;

        public TreeNodeViewModel()
        {
            Children = new ObservableCollection<TreeNodeViewModel>();
        }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }

        public TreeNodeType NodeType
        {
            get => _nodeType;
            set
            {
                _nodeType = value;
                OnPropertyChanged(nameof(NodeType));
            }
        }

        public ObservableCollection<TreeNodeViewModel> Children
        {
            get => _children;
            set
            {
                _children = value;
                OnPropertyChanged(nameof(Children));
            }
        }
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }
        public object Tag
        {
            get => _tag;
            set
            {
                _tag = value;
                OnPropertyChanged(nameof(Tag));
            }
        }

        // Make this public so CauseAndEffectViewModel can call it
        public new void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
        }
    }
}