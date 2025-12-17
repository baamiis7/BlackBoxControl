using BlackBoxControl.ViewModels;
using System.Drawing;


namespace BlackBoxControl.ViewModels
{
    public class InputsContainerViewModel : TreeNodeViewModel
    {
        public InputsContainerViewModel()
        {
            DisplayName = "Inputs";
            NodeType = TreeNodeType.CauseInputsContainer;
            Icon = "📥";

            Children = new System.Collections.ObjectModel.ObservableCollection<TreeNodeViewModel>();
        }
    }

}

