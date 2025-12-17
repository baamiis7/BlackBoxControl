using System.Collections.ObjectModel;

namespace BlackBoxControl.ViewModels
{
    public class InputNodeViewModel : TreeNodeViewModel
    {
        public CauseAndEffectViewModel ParentCE { get; }

        public InputNodeViewModel(CauseAndEffectViewModel ce)
        {
            ParentCE = ce;

            DisplayName = "Inputs";
            Icon = "📥";
            NodeType = TreeNodeType.CauseEffectInput;

            Children = new ObservableCollection<TreeNodeViewModel>();

            LoadInputDevices();
        }

        private void LoadInputDevices()
        {
            Children.Clear();

            foreach (var dev in ParentCE.InputDevices)
            {
                Children.Add(new DeviceItemViewModel(dev));
            }
        }

        public void Reload()
        {
            Children.Clear();

            foreach (var dev in ParentCE.InputDevices)
                Children.Add(new DeviceItemViewModel(dev));
        }

        // ⭐ Required so tree updates when input selection changes
        public void Refresh()
        {
            LoadInputDevices();
        }
    }
}
