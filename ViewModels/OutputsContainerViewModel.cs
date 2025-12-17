using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackBoxControl.ViewModels
{
    public class OutputsContainerViewModel : TreeNodeViewModel
    {
        public OutputsContainerViewModel()
        {
            DisplayName = "Outputs";
            NodeType = TreeNodeType.CauseOutputsContainer;
            Icon = "📤";

            Children = new System.Collections.ObjectModel.ObservableCollection<TreeNodeViewModel>();
        }
    }
}
