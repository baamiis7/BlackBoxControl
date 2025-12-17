using System.Windows.Controls;
using BlackBoxControl.ViewModels;

namespace BlackBoxControl
{
    public partial class CauseAndEffectForm : UserControl
    {
        /// <summary>
        /// Required by XAML. DO NOT REMOVE.
        /// </summary>
        public CauseAndEffectForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor used by your ViewModels.
        /// Assigns the provided CauseAndEffectViewModel.
        /// </summary>
        public CauseAndEffectForm(CauseAndEffectViewModel viewModel)
            : this()   // calls the default constructor first
        {
            this.DataContext = viewModel;
        }
    }
}
