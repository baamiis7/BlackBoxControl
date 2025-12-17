using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BlackBoxControl.Models;

namespace BlackBoxControl.ViewModels
{
    public class CauseAndEffectsListViewModel : ViewModelBase
    {
        private ObservableCollection<CauseAndEffectViewModel> _causeEffects;
        private string _searchText;
        private string _filterStatus;
        private string _filterLogicGate;
        private readonly TreeNodeViewModel _containerNode;

        // Event to notify the MainViewModel to display a specific C&E form
        public event EventHandler<CauseAndEffectViewModel> RequestEditForm;

        public CauseAndEffectsListViewModel()
        {
            CauseEffects = new ObservableCollection<CauseAndEffectViewModel>();

            // Initialize commands
            AddNewCauseEffectCommand = new RelayCommand(AddNewCauseEffect);
            EditCauseEffectCommand = new RelayCommand<CauseAndEffectViewModel>(EditCauseEffect);
            DeleteCauseEffectCommand = new RelayCommand<CauseAndEffectViewModel>(DeleteCauseEffect);
            TestCauseEffectCommand = new RelayCommand<CauseAndEffectViewModel>(TestCauseEffect);

            // Default filter values
            FilterStatus = "All Status";
            FilterLogicGate = "All Gates";
        }

        public CauseAndEffectsListViewModel(TreeNodeViewModel containerNode) : this()
        {
            _containerNode = containerNode;
            LoadCauseEffects();
        }

        #region Properties

        public ObservableCollection<CauseAndEffectViewModel> CauseEffects
        {
            get { return _causeEffects; }
            set
            {
                _causeEffects = value;
                OnPropertyChanged(nameof(CauseEffects));
                UpdateStatistics();
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterCauseEffects();
            }
        }

        public string FilterStatus
        {
            get { return _filterStatus; }
            set
            {
                _filterStatus = value;
                OnPropertyChanged(nameof(FilterStatus));
                FilterCauseEffects();
            }
        }

        public string FilterLogicGate
        {
            get { return _filterLogicGate; }
            set
            {
                _filterLogicGate = value;
                OnPropertyChanged(nameof(FilterLogicGate));
                FilterCauseEffects();
            }
        }

        // Statistics
        public int TotalEntries => CauseEffects?.Count ?? 0;
        public int ActiveEntries => CauseEffects?.Count(ce => ce.CauseEffect.IsEnabled) ?? 0;
        public int InactiveEntries => CauseEffects?.Count(ce => !ce.CauseEffect.IsEnabled) ?? 0;

        public string MostUsedGate
        {
            get
            {
                if (CauseEffects == null || CauseEffects.Count == 0)
                    return "N/A";

                var gateGroups = CauseEffects
                    .GroupBy(ce => ce.CauseEffect.LogicGate)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                return gateGroups?.Key.ToString() ?? "N/A";
            }
        }

        #endregion

        #region Commands

        public ICommand AddNewCauseEffectCommand { get; }
        public ICommand EditCauseEffectCommand { get; }
        public ICommand DeleteCauseEffectCommand { get; }
        public ICommand TestCauseEffectCommand { get; }

        #endregion

        #region Methods

        private void LoadCauseEffects()
        {
            if (_containerNode?.Children != null)
            {
                CauseEffects = new ObservableCollection<CauseAndEffectViewModel>(
                    _containerNode.Children.OfType<CauseAndEffectViewModel>()
                );
            }
        }


        /// <summary>
        /// Adds a new Cause and Effect entry
        /// </summary>
        public void AddNewCauseEffect()
        {
            if (_containerNode == null) return;

            // Find parent panel so we can get loops and busses
            var parentPanel = FindParentPanelViewModel(_containerNode);
            if (parentPanel == null) return;

            // Get loops from the fire panel
            var loops = parentPanel.Panel?.Loops
                        ?? new ObservableCollection<Loop>();

            // Get busses from the fire panel
            var busses = GetBussesFromPanel(parentPanel);

            // Create new Cause & Effect VM with proper parameters
            var newCE = new CauseAndEffectViewModel(loops, busses)
            {
                CauseEffect = new CauseAndEffect()   // new model instance
                {
                    Name = "New Cause & Effect",
                    IsEnabled = true,
                    LogicGate = LogicGate.AND
                }
            };

            // Add to the C&E container in the panel tree
            _containerNode.Children.Add(newCE);

            // Add to local observable collection for UI list
            CauseEffects.Add(newCE);

            // Refresh statistics
            UpdateStatistics();

            // Optionally auto-open form
            RequestEditForm?.Invoke(this, newCE);
        }


        private void EditCauseEffect(CauseAndEffectViewModel ceViewModel)
        {
            if (ceViewModel == null) return;

            // --- CORRECTED SECTION ---
            // Get the parent panel to pass its loops and buses
            var parentPanel = FindParentPanelViewModel(_containerNode);
            var loops = parentPanel?.Panel.Loops ?? new ObservableCollection<Loop>();
            var busses = GetBussesFromPanel(parentPanel);

            // Create a new, fully-populated ViewModel for editing
            var editViewModel = new CauseAndEffectViewModel(loops, busses) // This line should now work
            {
                CauseEffect = ceViewModel.CauseEffect
            };

            // Raise the event with the fully-populated view model
            RequestEditForm?.Invoke(this, editViewModel);
        }

        private void DeleteCauseEffect(CauseAndEffectViewModel ceViewModel)
        {
            if (ceViewModel == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{ceViewModel.CauseEffect.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Remove from container node to persist the change
                _containerNode?.Children.Remove(ceViewModel);

                // Remove from local collection
                CauseEffects.Remove(ceViewModel);
                UpdateStatistics();
            }
        }

        private void TestCauseEffect(CauseAndEffectViewModel ceViewModel)
        {
            if (ceViewModel == null) return;

            // CORRECTED: Use the new collection-based properties from the ViewModel
            int inputCount = ceViewModel.CauseEffect?.Inputs?.Count ?? 0;
            int outputCount = ceViewModel.CauseEffect?.Outputs?.Count ?? 0;

            string message = $"Test Configuration:\n\n" +
                           $"Name: {ceViewModel.CauseEffect.Name}\n" +
                           $"Status: {ceViewModel.CauseEffect.Status}\n" +
                           $"Logic Gate: {ceViewModel.CauseEffect.LogicGate}\n" +
                           $"Input Devices: {inputCount}\n" +
                           $"Output Devices: {outputCount}\n\n" +
                           "This would simulate the logic evaluation.";

            MessageBox.Show(message, "Test C&E Configuration", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void FilterCauseEffects()
        {
            // TODO: Implement filtering logic using CollectionViewSource or LINQ
            // This is a placeholder for the filtering implementation
        }

        private void UpdateStatistics()
        {
            OnPropertyChanged(nameof(TotalEntries));
            OnPropertyChanged(nameof(ActiveEntries));
            OnPropertyChanged(nameof(InactiveEntries));
            OnPropertyChanged(nameof(MostUsedGate));
        }

        // --- ADDED HELPER METHOD ---
        /// <summary>
        /// Helper method to get all Bus models from a panel's ViewModel.
        /// </summary>
        private ObservableCollection<Bus> GetBussesFromPanel(BlackBoxControlPanelViewModel parentPanel)
        {
            if (parentPanel == null) return new ObservableCollection<Bus>();

            var bussesContainer = parentPanel.Children
                .OfType<TreeNodeViewModel>()
                .FirstOrDefault(n => n.NodeType == TreeNodeType.BussesContainer);

            if (bussesContainer != null)
            {
                return new ObservableCollection<Bus>(
                    bussesContainer.Children.OfType<BusViewModel>().Select(bvm => bvm.Bus)
                );
            }
            return new ObservableCollection<Bus>();
        }

        // --- ADDED HELPER METHOD ---
        /// <summary>
        /// Helper method to find the parent BlackBoxControlPanelViewModel for a given TreeNodeViewModel.
        /// </summary>
        private BlackBoxControlPanelViewModel FindParentPanelViewModel(TreeNodeViewModel childNode)
        {
            // This is a bit of a workaround. In a real app, you might use a service or DI container.
            // For now, we'll find the main window and get its ViewModel.
            if (System.Windows.Application.Current.MainWindow is System.Windows.Window mainWindow)
            {
                var mainVM = mainWindow.DataContext as MainViewModel;
                return mainVM?.BlackBoxControlPanels.FirstOrDefault(p => p.Children.Contains(childNode));
            }
            return null;
        }

        #endregion
    }
}