using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using BlackBoxControl.Models;
using BlackBoxControl.ViewModels;
using System.Collections.ObjectModel;

namespace BlackBoxControl.Services
{
    public class ProjectService
    {
        public static void SaveProject(string filePath, MainViewModel mainViewModel)
        {
            var projectData = new ProjectData
            {
                ProjectName = Path.GetFileNameWithoutExtension(filePath),
                LastModifiedDate = DateTime.Now
            };

            // Convert ViewModels to Data models
            foreach (var panelVM in mainViewModel.BlackBoxControlPanels)
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

                // Save Loops
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

                // Save Busses
                var bussesContainer = panelVM.Children
                    .OfType<TreeNodeViewModel>()
                    .FirstOrDefault(n => n.NodeType == TreeNodeType.BussesContainer);

                if (bussesContainer != null)
                {
                    foreach (var busVM in bussesContainer.Children.OfType<BusViewModel>())
                    {
                        var busData = new BusData
                        {
                            BusName = busVM.Bus.BusName
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
                            {
                                nodeData.Inputs.Add(new InputOutputData
                                {
                                    Type = input.Type,
                                    Description = input.Description
                                });
                            }

                            foreach (var output in node.Outputs)
                            {
                                nodeData.Outputs.Add(new InputOutputData
                                {
                                    Type = output.Type,
                                    Description = output.Description
                                });
                            }

                            busData.Nodes.Add(nodeData);
                        }

                        panelData.Busses.Add(busData);
                    }
                }

                // Save Cause & Effects
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

                        // Save Inputs
                        foreach (var input in ceVM.CauseEffect.Inputs)
                        {
                            var inputData = new CauseInputData();

                            if (input is DeviceInput di)
                            {
                                inputData.InputType = "Device";
                                inputData.DeviceId = di.DeviceId;
                                inputData.Type = di.Type;
                                inputData.LocationText = di.LocationText;
                                inputData.ImagePath = di.ImagePath;
                            }
                            else if (input is TimeOfDayInput tod)
                            {
                                inputData.InputType = "TimeOfDay";
                                inputData.StartTime = tod.StartTime.ToString();
                                inputData.EndTime = tod.EndTime.ToString();
                            }
                            else if (input is DateTimeInput dt)
                            {
                                inputData.InputType = "DateTime";
                                inputData.TriggerDateTime = dt.TriggerDateTime;
                            }
                            else if (input is ReceiveApiInput rai)
                            {
                                inputData.InputType = "ReceiveApi";
                                inputData.ListenUrl = rai.ListenUrl;
                                inputData.HttpMethod = rai.HttpMethod;
                                inputData.ExpectedPath = rai.ExpectedPath;
                                inputData.AuthToken = rai.AuthToken;
                            }

                            ceData.Inputs.Add(inputData);
                        }

                        // Save Outputs
                        foreach (var output in ceVM.CauseEffect.Outputs)
                        {
                            var outputData = new EffectOutputData();

                            if (output is DeviceOutput d)
                            {
                                outputData.OutputType = "Device";
                                outputData.DeviceId = d.DeviceId;
                                outputData.Type = d.Type;
                                outputData.LocationText = d.LocationText;
                                outputData.ImagePath = d.ImagePath;
                            }
                            else if (output is SendTextOutput st)
                            {
                                outputData.OutputType = "SendText";
                                outputData.PhoneNumber = st.PhoneNumber;
                                outputData.Message = st.Message;
                            }
                            else if (output is SendEmailOutput se)
                            {
                                outputData.OutputType = "SendEmail";
                                outputData.EmailAddress = se.EmailAddress;
                                outputData.Subject = se.Subject;
                                outputData.Body = se.Body;
                            }
                            else if (output is SendApiOutput sa)
                            {
                                outputData.OutputType = "SendApi";
                                outputData.ApiUrl = sa.ApiUrl;
                                outputData.HttpMethod = sa.HttpMethod;
                                outputData.ContentType = sa.ContentType;
                                outputData.RequestBody = sa.RequestBody;
                            }

                            ceData.Outputs.Add(outputData);
                        }

                        panelData.CauseAndEffects.Add(ceData);
                    }
                }

                projectData.BlackBoxControlPanels.Add(panelData);
            }

            // Serialize to JSON and save
            var json = JsonConvert.SerializeObject(projectData, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static MainViewModel LoadProject(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var projectData = JsonConvert.DeserializeObject<ProjectData>(json);

            var mainViewModel = new MainViewModel();

            // Convert Data models back to ViewModels
            foreach (var panelData in projectData.BlackBoxControlPanels)
            {
                var panel = new BlackBoxControlPanel
                {
                    PanelName = panelData.PanelName,
                    Location = panelData.Location,
                    PanelAddress = panelData.PanelAddress,
                    NumberOfLoops = panelData.NumberOfLoops,
                    NumberOfZones = panelData.NumberOfZones,
                    ConfigGood = panelData.ConfigGood,
                    FirmwareVersion = panelData.FirmwareVersion,
                    Loops = new ObservableCollection<Loop>()
                };

                // Load Loops
                foreach (var loopData in panelData.Loops)
                {
                    var loop = new Loop
                    {
                        LoopNumber = loopData.LoopNumber,
                        LoopName = loopData.LoopName,
                        Devices = new ObservableCollection<LoopDevice>()
                    };

                    foreach (var deviceData in loopData.Devices)
                    {
                        loop.Devices.Add(new LoopDevice
                        {
                            Address = deviceData.Address,
                            Type = deviceData.Type,
                            LocationText = deviceData.LocationText,
                            Zone = deviceData.Zone,
                            ImagePath = deviceData.ImagePath
                        });
                    }

                    panel.Loops.Add(loop);
                }

                var panelVM = new BlackBoxControlPanelViewModel(panel);

                // Load Busses
                var bussesContainer = panelVM.Children
                    .OfType<TreeNodeViewModel>()
                    .FirstOrDefault(n => n.NodeType == TreeNodeType.BussesContainer);

                if (bussesContainer != null)
                {
                    foreach (var busData in panelData.Busses)
                    {
                        var bus = new Bus
                        {
                            BusName = busData.BusName,
                            Nodes = new ObservableCollection<BusNode>()
                        };

                        foreach (var nodeData in busData.Nodes)
                        {
                            var node = new BusNode
                            {
                                Address = nodeData.Address,
                                Name = nodeData.Name,
                                LocationText = nodeData.LocationText,
                                ImagePath = nodeData.ImagePath,
                                Inputs = new ObservableCollection<BusNodeIO>(),
                                Outputs = new ObservableCollection<BusNodeIO>()
                            };

                            foreach (var inputData in nodeData.Inputs)
                            {
                                node.Inputs.Add(new BusNodeIO
                                {
                                    Type = inputData.Type,
                                    Description = inputData.Description
                                });
                            }

                            foreach (var outputData in nodeData.Outputs)
                            {
                                node.Outputs.Add(new BusNodeIO
                                {
                                    Type = outputData.Type,
                                    Description = outputData.Description
                                });
                            }

                            bus.Nodes.Add(node);
                        }

                        var busVM = new BusViewModel(bus);
                        bussesContainer.Children.Add(busVM);
                    }
                }

                // Load Cause & Effects
                var ceContainer = panelVM.Children
                    .OfType<TreeNodeViewModel>()
                    .FirstOrDefault(n => n.NodeType == TreeNodeType.CauseEffectsContainer);

                System.Diagnostics.Debug.WriteLine($"=== Loading C&E for panel: {panelData.PanelName} ===");
                System.Diagnostics.Debug.WriteLine($"CE Container found: {ceContainer != null}");
                System.Diagnostics.Debug.WriteLine($"CE Data count from file: {panelData.CauseAndEffects.Count}");

                if (ceContainer != null)
                {
                    System.Diagnostics.Debug.WriteLine($"CE Container has {ceContainer.Children.Count} children before loading");
                }

                if (ceContainer != null && panelData.CauseAndEffects.Count > 0)
                {
                    foreach (var ceData in panelData.CauseAndEffects)
                    {
                        System.Diagnostics.Debug.WriteLine($"Loading C&E: {ceData.Name}, Inputs: {ceData.Inputs.Count}, Outputs: {ceData.Outputs.Count}");

                        var ceVM = new CauseAndEffectViewModel(panel.Loops, GetBussesFromPanel(panelVM))
                        {
                            CauseEffect = new CauseAndEffect
                            {
                                Name = ceData.Name,
                                LogicGate = (LogicGate)Enum.Parse(typeof(LogicGate), ceData.LogicGate),
                                IsEnabled = ceData.IsEnabled,
                                Inputs = new ObservableCollection<CauseInput>(),
                                Outputs = new ObservableCollection<EffectOutput>()
                            }
                        };

                        // Load Inputs
                        foreach (var inputData in ceData.Inputs)
                        {
                            CauseInput input = null;

                            switch (inputData.InputType)
                            {
                                case "Device":
                                    input = new DeviceInput
                                    {
                                        DeviceId = inputData.DeviceId,
                                        Type = inputData.Type,
                                        LocationText = inputData.LocationText,
                                        ImagePath = inputData.ImagePath
                                    };
                                    break;
                                case "TimeOfDay":
                                    input = new TimeOfDayInput
                                    {
                                        StartTime = TimeSpan.Parse(inputData.StartTime),
                                        EndTime = TimeSpan.Parse(inputData.EndTime)
                                    };
                                    ceVM.TimeOfDayInputs.Add((TimeOfDayInput)input);
                                    break;
                                case "DateTime":
                                    input = new DateTimeInput
                                    {
                                        TriggerDateTime = inputData.TriggerDateTime ?? DateTime.Now
                                    };
                                    ceVM.DateTimeInputs.Add((DateTimeInput)input);
                                    break;
                                case "ReceiveApi":
                                    input = new ReceiveApiInput
                                    {
                                        ListenUrl = inputData.ListenUrl,
                                        HttpMethod = inputData.HttpMethod,
                                        ExpectedPath = inputData.ExpectedPath,
                                        AuthToken = inputData.AuthToken
                                    };
                                    ceVM.ReceiveApiInputs.Add((ReceiveApiInput)input);
                                    break;
                            }

                            if (input != null)
                                ceVM.CauseEffect.Inputs.Add(input);
                        }

                        // Load Outputs
                        foreach (var outputData in ceData.Outputs)
                        {
                            EffectOutput output = null;

                            switch (outputData.OutputType)
                            {
                                case "Device":
                                    output = new DeviceOutput
                                    {
                                        DeviceId = outputData.DeviceId,
                                        Type = outputData.Type,
                                        LocationText = outputData.LocationText,
                                        ImagePath = outputData.ImagePath
                                    };
                                    break;
                                case "SendText":
                                    output = new SendTextOutput
                                    {
                                        PhoneNumber = outputData.PhoneNumber,
                                        Message = outputData.Message
                                    };
                                    ceVM.SendTextOutputs.Add((SendTextOutput)output);
                                    break;
                                case "SendEmail":
                                    output = new SendEmailOutput
                                    {
                                        EmailAddress = outputData.EmailAddress,
                                        Subject = outputData.Subject,
                                        Body = outputData.Body
                                    };
                                    ceVM.SendEmailOutputs.Add((SendEmailOutput)output);
                                    break;
                                case "SendApi":
                                    output = new SendApiOutput
                                    {
                                        ApiUrl = outputData.ApiUrl,
                                        HttpMethod = outputData.HttpMethod,
                                        ContentType = outputData.ContentType,
                                        RequestBody = outputData.RequestBody
                                    };
                                    ceVM.SendApiOutputs.Add((SendApiOutput)output);
                                    break;
                            }

                            if (output != null)
                                ceVM.CauseEffect.Outputs.Add(output);
                        }

                        // Build the tree structure for inputs/outputs
                        ceVM.RebuildTreeChildren();

                        System.Diagnostics.Debug.WriteLine($"C&E '{ceData.Name}' rebuilt tree, has {ceVM.Children.Count} children");

                        // Add the C&E to the tree
                        ceContainer.Children.Add(ceVM);

                        System.Diagnostics.Debug.WriteLine($"Added C&E to container");
                    }

                    System.Diagnostics.Debug.WriteLine($"CE Container now has {ceContainer.Children.Count} children after loading");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"SKIPPED: ceContainer null={ceContainer == null}, count={panelData.CauseAndEffects.Count}");
                }

                System.Diagnostics.Debug.WriteLine($"=== End Loading C&E ===");

                mainViewModel.BlackBoxControlPanels.Add(panelVM);
            }

            return mainViewModel;
        }

        private static ObservableCollection<Bus> GetBussesFromPanel(BlackBoxControlPanelViewModel panelVM)
        {
            var bussesContainer = panelVM.Children
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
    }
}