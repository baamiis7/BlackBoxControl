using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using BlackBoxControl.Models;
using BlackBoxControl.ViewModels;

namespace BlackBoxControl.Services
{
    /// <summary>
    /// Single source of truth for converting between ViewModels and ProjectData DTOs.
    /// Eliminates the 4 duplicate copies previously spread across ProjectService and MenuViewModel.
    /// </summary>
    public static class ProjectMapper
    {
        // ─── ViewModel → DTO ────────────────────────────────────────────────

        public static ProjectData ToProjectData(
            string projectName,
            IEnumerable<BlackBoxControlPanelViewModel> panelVMs)
        {
            var projectData = new ProjectData
            {
                ProjectName = projectName,
                LastModifiedDate = DateTime.Now
            };

            foreach (var panelVM in panelVMs)
                projectData.BlackBoxControlPanels.Add(ToPanelData(panelVM));

            return projectData;
        }

        public static BlackBoxControlPanelData ToPanelData(BlackBoxControlPanelViewModel panelVM)
        {
            var panelData = new BlackBoxControlPanelData
            {
                PanelName = panelVM.Panel.PanelName,
                Location = panelVM.Panel.Location,
                PanelAddress = panelVM.Panel.PanelAddress,
                NumberOfLoops = panelVM.Panel.NumberOfLoops,
                NumberOfZones = panelVM.Panel.NumberOfZones,
                ConfigGood = panelVM.Panel.ConfigGood,
                FirmwareVersion = panelVM.Panel.FirmwareVersion
            };

            // Convert loops from the model
            if (panelVM.Panel.Loops != null)
            {
                foreach (var loop in panelVM.Panel.Loops)
                {
                    var loopData = new LoopData
                    {
                        LoopNumber = loop.LoopNumber,
                        LoopName = loop.LoopName
                    };

                    foreach (var device in loop.Devices)
                    {
                        loopData.Devices.Add(new LoopDeviceData
                        {
                            Address = device.Address,
                            Type = device.Type,
                            LocationText = device.LocationText,
                            Zone = device.Zone,
                            ImagePath = device.ImagePath
                        });
                    }

                    panelData.Loops.Add(loopData);
                }
            }

            // Convert buses from the tree ViewModel (preserves BusNumber / BusType)
            var bussesContainer = panelVM.Children
                .OfType<TreeNodeViewModel>()
                .FirstOrDefault(n => n.NodeType == TreeNodeType.BussesContainer);

            if (bussesContainer != null)
            {
                foreach (var busVM in bussesContainer.Children.OfType<BusViewModel>())
                {
                    var busData = new BusData
                    {
                        BusNumber = busVM.Bus.BusNumber,
                        BusName = busVM.Bus.BusName,
                        BusType = busVM.Bus.BusType
                    };

                    foreach (var node in busVM.Bus.Nodes)
                    {
                        var nodeData = new BusNodeData
                        {
                            Address = node.Address,
                            Name = node.Name,
                            LocationText = node.LocationText,
                            ImagePath = node.ImagePath
                        };

                        foreach (var input in node.Inputs)
                            nodeData.Inputs.Add(new InputOutputData { Type = input.Type, Description = input.Description });

                        foreach (var output in node.Outputs)
                            nodeData.Outputs.Add(new InputOutputData { Type = output.Type, Description = output.Description });

                        busData.Nodes.Add(nodeData);
                    }

                    panelData.Busses.Add(busData);
                }
            }

            // Convert Cause & Effects from the tree ViewModel
            var ceContainer = panelVM.Children
                .OfType<TreeNodeViewModel>()
                .FirstOrDefault(n => n.NodeType == TreeNodeType.CauseEffectsContainer);

            if (ceContainer != null)
            {
                foreach (var ceVM in ceContainer.Children.OfType<CauseAndEffectViewModel>())
                {
                    var ceData = new CauseAndEffectData
                    {
                        Name = ceVM.CauseEffect.Name,
                        LogicGate = ceVM.CauseEffect.LogicGate.ToString(),
                        IsEnabled = ceVM.CauseEffect.IsEnabled
                    };

                    foreach (var input in ceVM.CauseEffect.Inputs)
                    {
                        var inputData = ToCauseInputData(input);
                        if (inputData != null)
                            ceData.Inputs.Add(inputData);
                    }

                    foreach (var output in ceVM.CauseEffect.Outputs)
                    {
                        var outputData = ToEffectOutputData(output);
                        if (outputData != null)
                            ceData.Outputs.Add(outputData);
                    }

                    panelData.CauseAndEffects.Add(ceData);
                }
            }

            return panelData;
        }

        public static CauseInputData? ToCauseInputData(CauseInput input)
        {
            if (input == null) return null;

            var data = new CauseInputData();

            if (input is DeviceInput deviceInput)
            {
                data.InputType = "Device";
                data.DeviceId = deviceInput.DeviceId;
                data.Type = deviceInput.Type;
                data.LocationText = deviceInput.LocationText;
                data.ImagePath = deviceInput.ImagePath;
            }
            else if (input is TimeOfDayInput timeInput)
            {
                data.InputType = "TimeOfDay";
                data.StartTime = timeInput.StartTime.ToString(@"hh\:mm");
                data.EndTime = timeInput.EndTime.ToString(@"hh\:mm");
            }
            else if (input is DateTimeInput dateTimeInput)
            {
                data.InputType = "DateTime";
                data.TriggerDateTime = dateTimeInput.TriggerDateTime;
            }
            else if (input is ReceiveApiInput apiInput)
            {
                data.InputType = "ReceiveApi";
                data.ListenUrl = apiInput.ListenUrl;
                data.HttpMethod = apiInput.HttpMethod;
                data.ExpectedPath = apiInput.ExpectedPath;
                data.AuthToken = apiInput.AuthToken;
            }

            return data;
        }

        public static EffectOutputData? ToEffectOutputData(EffectOutput output)
        {
            if (output == null) return null;

            var data = new EffectOutputData();

            if (output is DeviceOutput deviceOutput)
            {
                data.OutputType = "Device";
                data.DeviceId = deviceOutput.DeviceId;
                data.Type = deviceOutput.Type;
                data.LocationText = deviceOutput.LocationText;
                data.ImagePath = deviceOutput.ImagePath;
            }
            else if (output is SendTextOutput textOutput)
            {
                data.OutputType = "SendText";
                data.PhoneNumber = textOutput.PhoneNumber;
                data.Message = textOutput.Message;
            }
            else if (output is SendEmailOutput emailOutput)
            {
                data.OutputType = "SendEmail";
                data.EmailAddress = emailOutput.EmailAddress;
                data.Subject = emailOutput.Subject;
                data.Body = emailOutput.Body;
            }
            else if (output is SendApiOutput apiOutput)
            {
                data.OutputType = "SendApi";
                data.ApiUrl = apiOutput.ApiUrl;
                data.HttpMethod = apiOutput.HttpMethod;
                data.ContentType = apiOutput.ContentType;
                data.RequestBody = apiOutput.RequestBody;
            }

            return data;
        }

        // ─── DTO → ViewModel ────────────────────────────────────────────────

        public static IReadOnlyList<BlackBoxControlPanelViewModel> ToPanelViewModels(ProjectData projectData)
        {
            var result = new List<BlackBoxControlPanelViewModel>();

            foreach (var panelData in projectData.BlackBoxControlPanels)
            {
                var panel = ToPanelModel(panelData);
                result.Add(new BlackBoxControlPanelViewModel(panel));
            }

            return result;
        }

        private static BlackBoxControlPanel ToPanelModel(BlackBoxControlPanelData data)
        {
            var panel = new BlackBoxControlPanel
            {
                PanelName = data.PanelName,
                Location = data.Location,
                PanelAddress = data.PanelAddress,
                NumberOfLoops = data.NumberOfLoops,
                NumberOfZones = data.NumberOfZones,
                ConfigGood = data.ConfigGood,
                FirmwareVersion = data.FirmwareVersion ?? "1.0.0",
                Loops = new ObservableCollection<Loop>(),
                Busses = new ObservableCollection<Bus>(),
                CauseAndEffects = new ObservableCollection<CauseAndEffect>()
            };

            // Convert loops
            if (data.Loops != null)
            {
                foreach (var loopData in data.Loops)
                {
                    var loop = new Loop
                    {
                        LoopNumber = loopData.LoopNumber,
                        LoopName = loopData.LoopName ?? string.Empty,
                        NumberOfDevices = loopData.Devices?.Count ?? 0,
                        Devices = new ObservableCollection<LoopDevice>()
                    };

                    foreach (var deviceData in loopData.Devices ?? new())
                    {
                        loop.Devices.Add(new LoopDevice
                        {
                            Address = (byte)deviceData.Address,
                            Type = deviceData.Type ?? string.Empty,
                            LocationText = deviceData.LocationText ?? string.Empty,
                            Zone = deviceData.Zone,
                            ImagePath = deviceData.ImagePath
                                ?? $"/Images/{deviceData.Type?.Replace(" ", "_")}.png",
                            SubAddresses = new ObservableCollection<SubAddress>()
                        });
                    }

                    panel.Loops.Add(loop);
                }
            }

            // Convert buses
            if (data.Busses != null)
            {
                foreach (var busData in data.Busses)
                {
                    var bus = new Bus
                    {
                        BusNumber = busData.BusNumber,
                        BusName = busData.BusName ?? string.Empty,
                        BusType = busData.BusType ?? "RS485",
                        NumberOfNodes = busData.Nodes?.Count ?? 0,
                        Nodes = new ObservableCollection<BusNode>()
                    };

                    foreach (var nodeData in busData.Nodes ?? new())
                    {
                        var node = new BusNode
                        {
                            Address = (byte)nodeData.Address,
                            Name = nodeData.Name ?? string.Empty,
                            LocationText = nodeData.LocationText ?? "",
                            ImagePath = nodeData.ImagePath
                                ?? $"/BusImages/{nodeData.Name?.Replace(" ", "_")}.png",
                            Inputs = new ObservableCollection<BusNodeIO>(),
                            Outputs = new ObservableCollection<BusNodeIO>()
                        };

                        foreach (var inputData in nodeData.Inputs)
                            node.Inputs.Add(new BusNodeIO { Type = inputData.Type, Description = inputData.Description });

                        foreach (var outputData in nodeData.Outputs)
                            node.Outputs.Add(new BusNodeIO { Type = outputData.Type, Description = outputData.Description });

                        bus.Nodes.Add(node);
                    }

                    panel.Busses.Add(bus);
                }
            }

            // Convert cause and effects
            if (data.CauseAndEffects != null)
            {
                foreach (var ceData in data.CauseAndEffects)
                {
                    var causeEffect = new CauseAndEffect
                    {
                        Name = ceData.Name,
                        IsEnabled = ceData.IsEnabled,
                        LogicGate = Enum.Parse<LogicGate>(ceData.LogicGate),
                        Inputs = new ObservableCollection<CauseInput>(),
                        Outputs = new ObservableCollection<EffectOutput>()
                    };

                    foreach (var inputData in ceData.Inputs)
                    {
                        var input = ToCauseInput(inputData);
                        if (input != null)
                            causeEffect.Inputs.Add(input);
                    }

                    foreach (var outputData in ceData.Outputs)
                    {
                        var output = ToEffectOutput(outputData);
                        if (output != null)
                            causeEffect.Outputs.Add(output);
                    }

                    panel.CauseAndEffects.Add(causeEffect);
                }
            }

            return panel;
        }

        public static CauseInput? ToCauseInput(CauseInputData data)
        {
            if (data == null) return null;

            return data.InputType switch
            {
                "Device" => new DeviceInput
                {
                    DeviceId = data.DeviceId ?? string.Empty,
                    Type = data.Type ?? string.Empty,
                    LocationText = data.LocationText ?? string.Empty,
                    ImagePath = data.ImagePath ?? string.Empty
                },
                "TimeOfDay" => new TimeOfDayInput
                {
                    StartTime = TimeSpan.Parse(data.StartTime ?? "00:00"),
                    EndTime = TimeSpan.Parse(data.EndTime ?? "00:00")
                },
                "DateTime" => new DateTimeInput
                {
                    TriggerDateTime = data.TriggerDateTime ?? DateTime.Now
                },
                "ReceiveApi" => new ReceiveApiInput
                {
                    ListenUrl = data.ListenUrl ?? string.Empty,
                    HttpMethod = data.HttpMethod ?? string.Empty,
                    ExpectedPath = data.ExpectedPath ?? string.Empty,
                    AuthToken = data.AuthToken ?? string.Empty
                },
                _ => null
            };
        }

        public static EffectOutput? ToEffectOutput(EffectOutputData data)
        {
            if (data == null) return null;

            return data.OutputType switch
            {
                "Device" => new DeviceOutput
                {
                    DeviceId = data.DeviceId ?? string.Empty,
                    Type = data.Type ?? string.Empty,
                    LocationText = data.LocationText ?? string.Empty,
                    ImagePath = data.ImagePath ?? string.Empty
                },
                "SendText" => new SendTextOutput
                {
                    PhoneNumber = data.PhoneNumber ?? string.Empty,
                    Message = data.Message ?? string.Empty
                },
                "SendEmail" => new SendEmailOutput
                {
                    EmailAddress = data.EmailAddress ?? string.Empty,
                    Subject = data.Subject ?? string.Empty,
                    Body = data.Body ?? string.Empty
                },
                "SendApi" => new SendApiOutput
                {
                    ApiUrl = data.ApiUrl ?? string.Empty,
                    HttpMethod = data.HttpMethod ?? string.Empty,
                    ContentType = data.ContentType ?? string.Empty,
                    RequestBody = data.RequestBody ?? string.Empty
                },
                _ => null
            };
        }
    }
}
