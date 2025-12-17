using System.Windows.Controls;
using BlackBoxControl.ViewModels;

namespace BlackBoxControl
{
    /// <summary>
    /// Interaction logic for CauseAndEffectsListForm.xaml
    /// </summary>
    public partial class CauseAndEffectsListForm : UserControl
    {
        public CauseAndEffectsListForm()
        {
            InitializeComponent();
        }

        // Constructor that accepts the container node
        public CauseAndEffectsListForm(TreeNodeViewModel containerNode)
        {
            InitializeComponent();
            this.DataContext = new CauseAndEffectsListViewModel(containerNode);
        }
    }
}