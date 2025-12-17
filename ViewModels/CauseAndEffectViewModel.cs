using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using BlackBoxControl.Models;

namespace BlackBoxControl.ViewModels
{
    public class CauseAndEffectViewModel : TreeNodeViewModel, INotifyPropertyChanged
    {
            // --- BACKING FIELDS ---
        private CauseAndEffect _causeEffect;
        private string _inputSearchText;
        private string _outputSearchText;

        private ObservableCollection<SelectableDevice> _inputDevices = new ObservableCollection<SelectableDevice>();
        private ObservableCollection<SelectableDevice> _outputDevices = new ObservableCollection<SelectableDevice>();

        private ObservableCollection<GroupedDevice> _groupedInputDevices = new ObservableCollection<GroupedDevice>();
        private ObservableCollection<GroupedDevice> _groupedOutputDevices = new ObservableCollection<GroupedDevice>();
        public ObservableCollection<SendApiOutput> SendApiOutputs { get; set; }
        public ObservableCollection<ReceiveApiInput> ReceiveApiInputs { get; set; }
        private readonly IEnumerable<Loop> _loops;
        private readonly IEnumerable<Bus> _busses;

        // --- EXTRA TREE NODE PROPERTIES ---
        public string DisplayName
        {
            get => CauseEffect?.Name ?? "Cause & Effect";
            set
            {
                if (CauseEffect == null) return;
                CauseEffect.Name = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string Icon => "⚡";   // Replace with image path if you prefer

        // --- ADVANCED INPUTS/OUTPUTS ---
        public ObservableCollection<TimeOfDayInput> TimeOfDayInputs { get; set; }
        public ObservableCollection<DateTimeInput> DateTimeInputs { get; set; }
        public ObservableCollection<SendTextOutput> SendTextOutputs { get; set; }
        public ObservableCollection<SendEmailOutput> SendEmailOutputs { get; set; }

        // --- GROUPED DEVICES (for the form lists) ---
        public ObservableCollection<GroupedDevice> GroupedInputDevices
        {
            get => _groupedInputDevices;
            set
            {
                _groupedInputDevices = value ?? new ObservableCollection<GroupedDevice>();
                OnPropertyChanged();
            }
        }

        public ObservableCollection<GroupedDevice> GroupedOutputDevices
        {
            get => _groupedOutputDevices;
            set
            {
                _groupedOutputDevices = value ?? new ObservableCollection<GroupedDevice>();
                OnPropertyChanged();
            }
        }

        // --- COMMANDS ---
        public ICommand AddTimeOfDayInputCommand { get; }
        public ICommand RemoveTimeOfDayInputCommand { get; }
        public ICommand AddDateTimeInputCommand { get; }
        public ICommand RemoveDateTimeInputCommand { get; }
        public ICommand AddSendTextOutputCommand { get; }
        public ICommand RemoveSendTextOutputCommand { get; }
        public ICommand AddSendEmailOutputCommand { get; }
        public ICommand RemoveSendEmailOutputCommand { get; }
        public ICommand AddSendApiOutputCommand { get; }
        public ICommand RemoveSendApiOutputCommand { get; }
        public ICommand AddReceiveApiInputCommand { get; }
        public ICommand RemoveReceiveApiInputCommand { get; }

        // =====================================================================
        // Constructor — this ViewModel is also a TreeNodeViewModel
        // =====================================================================
        public CauseAndEffectViewModel(IEnumerable<Loop> loops, IEnumerable<Bus> busses)
            {
                NodeType = TreeNodeType.CauseEffect;

                // Tree children for this C&E node
                Children = new ObservableCollection<TreeNodeViewModel>();

                // Core model (caller often overwrites this with an existing instance)
                CauseEffect = new CauseAndEffect();

                // Base collections
                InputDevices = new ObservableCollection<SelectableDevice>();
                OutputDevices = new ObservableCollection<SelectableDevice>();

                GroupedInputDevices = new ObservableCollection<GroupedDevice>();
                GroupedOutputDevices = new ObservableCollection<GroupedDevice>();

                // Advanced items
                TimeOfDayInputs = new ObservableCollection<TimeOfDayInput>();
                DateTimeInputs = new ObservableCollection<DateTimeInput>();
                SendTextOutputs = new ObservableCollection<SendTextOutput>();
                SendEmailOutputs = new ObservableCollection<SendEmailOutput>();
                SendApiOutputs = new ObservableCollection<SendApiOutput>();
                ReceiveApiInputs = new ObservableCollection<ReceiveApiInput>();

                // Commands
                SaveCommand = new RelayCommand(Save, CanSave);
                CancelCommand = new RelayCommand(Cancel);
                TestConfigCommand = new RelayCommand(TestConfiguration);

                SelectAllInputsCommand = new RelayCommand(SelectAllInputs);
                ClearAllInputsCommand = new RelayCommand(ClearAllInputs);

                SelectAllOutputsCommand = new RelayCommand(SelectAllOutputs);
                ClearAllOutputsCommand = new RelayCommand(ClearAllOutputs);

                AddTimeOfDayInputCommand = new RelayCommand(AddTimeOfDayInput);
                RemoveTimeOfDayInputCommand = new RelayCommand<TimeOfDayInput>(RemoveTimeOfDayInput);

                AddDateTimeInputCommand = new RelayCommand(AddDateTimeInput);
                RemoveDateTimeInputCommand = new RelayCommand<DateTimeInput>(RemoveDateTimeInput);

                AddSendTextOutputCommand = new RelayCommand(AddSendTextOutput);
                RemoveSendTextOutputCommand = new RelayCommand<SendTextOutput>(RemoveSendTextOutput);

                AddSendEmailOutputCommand = new RelayCommand(AddSendEmailOutput);
                RemoveSendEmailOutputCommand = new RelayCommand<SendEmailOutput>(RemoveSendEmailOutput);

                AddSendApiOutputCommand = new RelayCommand(AddSendApiOutput);
                RemoveSendApiOutputCommand = new RelayCommand<SendApiOutput>(RemoveSendApiOutput);
            AddReceiveApiInputCommand = new RelayCommand(AddReceiveApiInput);
            RemoveReceiveApiInputCommand = new RelayCommand<ReceiveApiInput>(RemoveReceiveApiInput);


            // Store loops and buses for device loading
            _loops = loops;
                _busses = busses;

                // Load devices for the form (grouped lists)
                LoadDevices();

                // Initially the tree has empty Inputs/Outputs children
                RebuildTreeChildren();
            }

        #region Properties

        public CauseAndEffect CauseEffect
        {
            get => _causeEffect;
            set
            {
                // Unsubscribe from old instance
                if (_causeEffect != null)
                    _causeEffect.PropertyChanged -= CauseEffect_PropertyChanged;

                _causeEffect = value;

                // Subscribe to new instance
                if (_causeEffect != null)
                    _causeEffect.PropertyChanged += CauseEffect_PropertyChanged;

                OnPropertyChanged(nameof(CauseEffect));
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(LogicGateDescription));
            }
        }

        // Add this new method
        private void CauseEffect_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CauseAndEffect.LogicGate))
            {
                OnPropertyChanged(nameof(LogicGateDescription));
            }
        }

        public ObservableCollection<SelectableDevice> InputDevices
            {
                get => _inputDevices;
                set
                {
                    _inputDevices = value ?? new ObservableCollection<SelectableDevice>();
                    OnPropertyChanged();
                }
            }

            public ObservableCollection<SelectableDevice> OutputDevices
            {
                get => _outputDevices;
                set
                {
                    _outputDevices = value ?? new ObservableCollection<SelectableDevice>();
                    OnPropertyChanged();
                }
            }

            public string InputSearchText
            {
                get => _inputSearchText;
                set { _inputSearchText = value; OnPropertyChanged(); FilterInputDevices(); }
            }

            public string OutputSearchText
            {
                get => _outputSearchText;
                set { _outputSearchText = value; OnPropertyChanged(); FilterOutputDevices(); }
            }

        public int SelectedInputsCount =>(InputDevices?.Count(d => d.IsSelected) ?? 0) + (TimeOfDayInputs?.Count ?? 0) + 
            (DateTimeInputs?.Count ?? 0) + (ReceiveApiInputs?.Count ?? 0);
        public int SelectedOutputsCount => (OutputDevices?.Count(d => d.IsSelected) ?? 0) + (SendTextOutputs?.Count ?? 0) + 
            (SendEmailOutputs?.Count ?? 0) + (SendApiOutputs?.Count ?? 0);

        // For the list summary cards / Tree text
        public int InputCount => SelectedInputsCount;
            public int OutputCount => SelectedOutputsCount;

            #endregion

            #region Commands

            public ICommand SaveCommand { get; }
            public ICommand CancelCommand { get; }
            public ICommand TestConfigCommand { get; }

            public ICommand SelectAllInputsCommand { get; }
            public ICommand ClearAllInputsCommand { get; }

            public ICommand SelectAllOutputsCommand { get; }
            public ICommand ClearAllOutputsCommand { get; }

            #endregion

            #region Device Loading

            private void LoadDevices()
            {
                LoadInputDevices();
                LoadOutputDevices();
            }

            private void LoadInputDevices()
            {
                if (GroupedInputDevices == null)
                    GroupedInputDevices = new ObservableCollection<GroupedDevice>();
                if (InputDevices == null)
                    InputDevices = new ObservableCollection<SelectableDevice>();

                GroupedInputDevices.Clear();
                InputDevices.Clear(); // still used for selection counters

                string fallbackImage = "/Assets/Nodes/default.png";

                // GROUP BY LOOPS
                if (_loops != null)
                {
                    foreach (var loop in _loops)
                    {
                        var group = new GroupedDevice(loop.LoopName ?? $"Loop {loop.LoopNumber}");

                        foreach (var device in loop.Devices)
                        {
                            var selectable = new SelectableDevice
                            {
                                DeviceId = $"{device.Type}_{device.Address}",
                                Type = device.Type,
                                Address = device.Address.ToString(),
                                LocationText = device.LocationText,
                                ImagePath = string.IsNullOrWhiteSpace(device.ImagePath)
                                            ? fallbackImage
                                            : device.ImagePath,
                                SelectedChanged = NotifySelectionChanged
                            };

                            group.Devices.Add(selectable);
                            InputDevices.Add(selectable);
                        }

                        GroupedInputDevices.Add(group);
                    }
                }

                // GROUP BY BUSSES (Nodes)
                if (_busses != null)
                {
                    foreach (var bus in _busses)
                    {
                        var group = new GroupedDevice(bus.BusName);

                        foreach (var node in bus.Nodes)
                        {
                            var selectable = new SelectableDevice
                            {
                                DeviceId = $"BusNode_{node.Address}",
                                Type = "RS485 Node",
                                Address = node.Address.ToString(),
                                LocationText = node.LocationText,
                                ImagePath = string.IsNullOrWhiteSpace(node.ImagePath)
                                            ? fallbackImage
                                            : node.ImagePath,
                                SelectedChanged = NotifySelectionChanged
                            };

                            group.Devices.Add(selectable);
                            InputDevices.Add(selectable);
                        }

                        GroupedInputDevices.Add(group);
                    }
                }
            }

            private void LoadOutputDevices()
            {
                if (GroupedOutputDevices == null)
                    GroupedOutputDevices = new ObservableCollection<GroupedDevice>();
                if (OutputDevices == null)
                    OutputDevices = new ObservableCollection<SelectableDevice>();

                GroupedOutputDevices.Clear();
                OutputDevices.Clear();

                string fallbackImage = "/Assets/Nodes/default.png";

                // LOOP DEVICES
                if (_loops != null)
                {
                    foreach (var loop in _loops)
                    {
                        var group = new GroupedDevice(loop.LoopName ?? $"Loop {loop.LoopNumber}");

                        foreach (var device in loop.Devices)
                        {
                            var selectable = new SelectableDevice
                            {
                                DeviceId = $"{device.Type}_{device.Address}",
                                Type = device.Type,
                                Address = device.Address.ToString(),
                                LocationText = device.LocationText,
                                ImagePath = string.IsNullOrWhiteSpace(device.ImagePath)
                                            ? fallbackImage
                                            : device.ImagePath,
                                SelectedChanged = NotifySelectionChanged
                            };

                            group.Devices.Add(selectable);
                            OutputDevices.Add(selectable);
                        }

                        GroupedOutputDevices.Add(group);
                    }
                }

                // BUS NODES
                if (_busses != null)
                {
                    foreach (var bus in _busses)
                    {
                        var group = new GroupedDevice(bus.BusName);

                        foreach (var node in bus.Nodes)
                        {
                            var selectable = new SelectableDevice
                            {
                                DeviceId = $"BusNode_{node.Address}",
                                Type = "RS485 Node",
                                Address = node.Address.ToString(),
                                LocationText = node.LocationText,
                                ImagePath = string.IsNullOrWhiteSpace(node.ImagePath)
                                            ? fallbackImage
                                            : node.ImagePath,
                                SelectedChanged = NotifySelectionChanged
                            };

                            group.Devices.Add(selectable);
                            OutputDevices.Add(selectable);
                        }

                        GroupedOutputDevices.Add(group);
                    }
                }
            }

            #endregion

            #region Save / Test / Filters / Selection

            public void NotifySelectionChanged()
            {
                OnPropertyChanged(nameof(SelectedInputsCount));
                OnPropertyChanged(nameof(SelectedOutputsCount));
                OnPropertyChanged(nameof(InputCount));
                OnPropertyChanged(nameof(OutputCount));

                // Re-evaluate SaveCommand.CanExecute
                CommandManager.InvalidateRequerySuggested();
            }

            private bool CanSave()
            {
                if (string.IsNullOrWhiteSpace(CauseEffect?.Name)) return false;
                if (SelectedInputsCount == 0) return false;
                if (SelectedOutputsCount == 0) return false;
                return true;
            }

        private void Save()
        {
            // Clear existing inputs/outputs from the model
            CauseEffect.Inputs.Clear();
            CauseEffect.Outputs.Clear();

            // Add selected device inputs
            foreach (var device in InputDevices.Where(d => d.IsSelected))
            {
                CauseEffect.Inputs.Add(new DeviceInput
                {
                    DeviceId = device.DeviceId,
                    Type = device.Type,
                    LocationText = device.LocationText,
                    ImagePath = device.ImagePath
                });
            }

            // Add advanced inputs
            foreach (var input in TimeOfDayInputs)
                CauseEffect.Inputs.Add(input);

            foreach (var input in DateTimeInputs)
                CauseEffect.Inputs.Add(input);

            // Add selected device outputs
            foreach (var device in OutputDevices.Where(d => d.IsSelected))
            {
                CauseEffect.Outputs.Add(new DeviceOutput
                {
                    DeviceId = device.DeviceId,
                    Type = device.Type,
                    LocationText = device.LocationText,
                    ImagePath = device.ImagePath
                });
            }

            // Add advanced outputs
            foreach (var output in SendTextOutputs)
                CauseEffect.Outputs.Add(output);

            foreach (var output in SendEmailOutputs)
                CauseEffect.Outputs.Add(output);

            foreach (var output in SendApiOutputs)
                CauseEffect.Outputs.Add(output);

            foreach (var input in ReceiveApiInputs)  
                CauseEffect.Inputs.Add(input);

            // Update counters
            OnPropertyChanged(nameof(SelectedInputsCount));
            OnPropertyChanged(nameof(SelectedOutputsCount));
            OnPropertyChanged(nameof(InputCount));
            OnPropertyChanged(nameof(OutputCount));

            // Rebuild the tree: Inputs / Outputs children under this C&E node
            RebuildTreeChildren();

            MessageBox.Show("Cause and Effect saved!", "Success", MessageBoxButton.OK);
        }
        private void Cancel()
        {
            // Optional: reset selections, etc.
        }

        private void TestConfiguration()
        {
            MessageBox.Show(
                $"Inputs: {SelectedInputsCount}\nOutputs: {SelectedOutputsCount}",
                "Test C&E",
                MessageBoxButton.OK);
        }

        private void FilterInputDevices() { /* TODO: implement search filter */ }
        private void FilterOutputDevices() { /* TODO: implement search filter */ }

        private void SelectAllInputs()
        {
            foreach (var device in InputDevices)
                device.IsSelected = true;

            NotifySelectionChanged();
        }

        private void ClearAllInputs()
        {
            foreach (var device in InputDevices)
                device.IsSelected = false;

            NotifySelectionChanged();
        }

        private void SelectAllOutputs()
        {
            foreach (var device in OutputDevices)
                device.IsSelected = true;

            NotifySelectionChanged();
        }

        private void ClearAllOutputs()
        {
            foreach (var device in OutputDevices)
                device.IsSelected = false;

            NotifySelectionChanged();
        }

        private void AddTimeOfDayInput()
        {
            var newInput = new TimeOfDayInput
            {
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(17, 0, 0)
            };

            TimeOfDayInputs.Add(newInput);
            CauseEffect.Inputs.Add(newInput);

            NotifySelectionChanged();
        }
        private void RemoveTimeOfDayInput(TimeOfDayInput input)
        {
            if (input == null) return;

            TimeOfDayInputs.Remove(input);
            CauseEffect.Inputs.Remove(input);

            NotifySelectionChanged();
        }
        private void AddDateTimeInput()
        {
            var newInput = new DateTimeInput
            {
                TriggerDateTime = DateTime.Now.AddDays(1)
            };

            DateTimeInputs.Add(newInput);
            CauseEffect.Inputs.Add(newInput);

            NotifySelectionChanged();
        }
        private void RemoveDateTimeInput(DateTimeInput input)
        {
            if (input == null) return;

            DateTimeInputs.Remove(input);
            CauseEffect.Inputs.Remove(input);

            NotifySelectionChanged();
        }
        private void AddSendTextOutput()
        {
            var newOutput = new SendTextOutput
            {
                PhoneNumber = "123-456-7890",
                Message = "Alarm triggered!"
            };

            SendTextOutputs.Add(newOutput);
            CauseEffect.Outputs.Add(newOutput);

            NotifySelectionChanged();
        }
        private void RemoveSendTextOutput(SendTextOutput output)
        {
            if (output == null) return;

            SendTextOutputs.Remove(output);
            CauseEffect.Outputs.Remove(output);

            NotifySelectionChanged();
        }
        private void AddSendEmailOutput()
        {
            var newOutput = new SendEmailOutput
            {
                EmailAddress = "test@example.com",
                Subject = "Alarm",
                Body = "An alarm was triggered in the system."
            };

            SendEmailOutputs.Add(newOutput);
            CauseEffect.Outputs.Add(newOutput);

            NotifySelectionChanged();
        }
        private void RemoveSendEmailOutput(SendEmailOutput output)
        {
            if (output == null) return;

            SendEmailOutputs.Remove(output);
            CauseEffect.Outputs.Remove(output);

            NotifySelectionChanged();
        }
        // Add these methods
        private void AddSendApiOutput()
        {
            var newOutput = new SendApiOutput
            {
                ApiUrl = "https://api.example.com/alert",
                HttpMethod = "POST",
                ContentType = "application/json",
                RequestBody = "{ \"alert\": \"Fire alarm triggered\" }"
            };

            SendApiOutputs.Add(newOutput);
            CauseEffect.Outputs.Add(newOutput);

            NotifySelectionChanged();
        }

        private void RemoveSendApiOutput(SendApiOutput output)
        {
            if (output == null) return;

            SendApiOutputs.Remove(output);
            CauseEffect.Outputs.Remove(output);

            NotifySelectionChanged();
        }
        private void AddReceiveApiInput()
        {
            var newInput = new ReceiveApiInput
            {
                ListenUrl = "http://localhost:8080/webhook",
                HttpMethod = "POST",
                ExpectedPath = "/fire-alarm/trigger",
                AuthToken = ""
            };

            ReceiveApiInputs.Add(newInput);
            CauseEffect.Inputs.Add(newInput);

            NotifySelectionChanged();
        }

        private void RemoveReceiveApiInput(ReceiveApiInput input)
        {
            if (input == null) return;

            ReceiveApiInputs.Remove(input);
            CauseEffect.Inputs.Remove(input);

            NotifySelectionChanged();
        }

        #endregion

        #region Tree building: Inputs / Outputs and device children

        public string LogicGateDescription
        {
            get
            {
                switch (CauseEffect?.LogicGate)
                {
                    case LogicGate.OR:
                        return "Triggers when ANY input activates";
                    case LogicGate.AND:
                        return "Triggers when ALL inputs activate";
                    case LogicGate.XOR:
                        return "Triggers when ONLY ONE input activates";
                    default:
                        return "Select a logic gate";
                }
            }
        }

        /// <summary>
        /// Rebuild the TreeView children of this C&E node based on CauseEffect.Inputs/Outputs
        /// </summary>
        public void RebuildTreeChildren()
        {
            // DO NOT REPLACE THE COLLECTION – ONLY CLEAR IT
            Children.Clear();

            // --------------------------
            // INPUTS NODE
            // --------------------------
            var inputsNode = new TreeNodeViewModel
            {
                DisplayName = $"Inputs ({CauseEffect.Inputs.Count})",
                Icon = "📥",
                NodeType = TreeNodeType.CauseEffectInput,
                Children = new ObservableCollection<TreeNodeViewModel>()
            };

            foreach (var input in CauseEffect.Inputs)
            {
                inputsNode.Children.Add(new TreeNodeViewModel
                {
                    DisplayName = GetInputDisplayName(input),
                    Icon = "🔸",
                    NodeType = TreeNodeType.Device,
                    Children = new ObservableCollection<TreeNodeViewModel>(),
                    Tag = input // STORE REFERENCE TO THE ACTUAL INPUT OBJECT
                });
            }

            // --------------------------
            // OUTPUTS NODE
            // --------------------------
            var outputsNode = new TreeNodeViewModel
            {
                DisplayName = $"Outputs ({CauseEffect.Outputs.Count})",
                Icon = "📤",
                NodeType = TreeNodeType.CauseEffectOutput,
                Children = new ObservableCollection<TreeNodeViewModel>()
            };

            foreach (var output in CauseEffect.Outputs)
            {
                outputsNode.Children.Add(new TreeNodeViewModel
                {
                    DisplayName = GetOutputDisplayName(output),
                    Icon = "🔸",
                    NodeType = TreeNodeType.Device,
                    Children = new ObservableCollection<TreeNodeViewModel>(),
                    Tag = output // STORE REFERENCE TO THE ACTUAL INPUT OBJECT
                });
            }

            // Add to tree
            Children.Add(inputsNode);
            Children.Add(outputsNode);

            // Force UI refresh
            OnPropertyChanged(nameof(Children));
        }



        private string GetInputDisplayName(CauseInput input)
        {
            switch (input)
            {
                case DeviceInput di:
                    return $"✅ {di.Type} @ {di.LocationText}";
                case TimeOfDayInput tod:
                    return $"🕐 Time: {tod.StartTime:hh\\:mm}–{tod.EndTime:hh\\:mm}";
                case DateTimeInput dt:
                    return $"📅 On {dt.TriggerDateTime:g}";
                case ReceiveApiInput rai:
                    return $"🌐 API: {rai.HttpMethod} {rai.ExpectedPath}";
                default:
                    return input?.GetType().Name ?? "Input";
            }
        }

        private string GetOutputDisplayName(EffectOutput output)
        {
            switch (output)
            {
                case DeviceOutput d:
                    return $"✅ {d.Type} @ {d.LocationText}";
                case SendTextOutput st:
                    return $"📱 SMS to {st.PhoneNumber}";
                case SendEmailOutput se:
                    return $"📧 Email to {se.EmailAddress}";
                case SendApiOutput sa:
                    return $"🌐 API: {sa.HttpMethod} {sa.ApiUrl}";
                default:
                    return output?.GetType().Name ?? "Output";
            }
        }
        public void RefreshTreeView()
        {
            // Force the TreeView to re-evaluate the entire tree
            OnPropertyChanged(nameof(Children));

            // Also notify each child node
            foreach (var child in Children)
            {
                child.OnPropertyChanged(nameof(child.Children));

                // Recursively refresh their children too
                foreach (var grandchild in child.Children)
                {
                    grandchild.OnPropertyChanged(nameof(grandchild.Children));
                }
            }
        }

        #endregion

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    



    // Helper classes stay the same...
    #region Helper Classes

    public class SelectableDevice : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string DeviceId { get; set; }
        public string Type { get; set; }
        public string Address { get; set; }
        public string LocationText { get; set; }
        public string ImagePath { get; set; }
        public string GroupName { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));

                // Notify C&E VM so Save button re-evaluates CanSave()
                (Application.Current.MainWindow.DataContext as MainViewModel)?
                    .RefreshCauseEffectSaveState();
            }
        }
        public Action SelectedChanged { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // --- UPDATED CAUSE AND EFFECT MODEL ---
    public class CauseAndEffect : INotifyPropertyChanged
    {
        private string _name;
        private LogicGate _logicGate;
        private bool _isEnabled;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public LogicGate LogicGate
        {
            get { return _logicGate; }
            set
            {
                _logicGate = value;
                OnPropertyChanged(nameof(LogicGate));
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public string Status
        {
            get { return IsEnabled ? "Active" : "Inactive"; }
        }

        // --- UPDATED: Use new item-based collections ---
        public ObservableCollection<CauseInput> Inputs { get; set; }
        public ObservableCollection<EffectOutput> Outputs { get; set; }

        // --- REMOVED: Old properties are no longer needed ---
        // public System.Collections.Generic.List<string> InputDeviceIds { get; set; }
        // public System.Collections.Generic.List<string> OutputDeviceIds { get; set; }

        public CauseAndEffect()
        {
            LogicGate = LogicGate.OR;
            IsEnabled = true;

            // Initialize the new collections
            Inputs = new ObservableCollection<CauseInput>();
            Outputs = new ObservableCollection<EffectOutput>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum LogicGate
    {
        OR,
        AND,
        XOR
    }

    #endregion
}