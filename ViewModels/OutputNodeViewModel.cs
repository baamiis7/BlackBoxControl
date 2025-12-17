using System.Collections.ObjectModel;

namespace BlackBoxControl.ViewModels
{
    public class OutputNodeViewModel : TreeNodeViewModel
    {
        public CauseAndEffectViewModel ParentCE { get; }

        public OutputNodeViewModel(CauseAndEffectViewModel ce)
        {
            ParentCE = ce;

            DisplayName = "Outputs";
            Icon = "📤";
            NodeType = TreeNodeType.CauseEffectOutput;

            Children = new ObservableCollection<TreeNodeViewModel>();

            LoadOutputDevices();
        }

        public void Reload()
        {
            Children.Clear();

            foreach (var dev in ParentCE.OutputDevices)
                Children.Add(new DeviceItemViewModel(dev));
        }


        private void LoadOutputDevices()
        {
            Children.Clear();

            foreach (var dev in ParentCE.OutputDevices)
                Children.Add(new DeviceItemViewModel(dev));
        }
    }
}
