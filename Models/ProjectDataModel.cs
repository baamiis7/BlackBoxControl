using System;
using System.Collections.Generic;

namespace BlackBoxControl.Models
{
    [Serializable]
    public class ProjectData
    {
        public string ProjectName { get; set; }
        public string ProjectVersion { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public List<BlackBoxControlPanelData> BlackBoxControlPanels { get; set; }

        public ProjectData()
        {
            ProjectVersion = "1.0";
            CreatedDate = DateTime.Now;
            LastModifiedDate = DateTime.Now;
            BlackBoxControlPanels = new List<BlackBoxControlPanelData>();
        }
    }

    [Serializable]
    public class BlackBoxControlPanelData
    {
        public string PanelName { get; set; }
        public string Location { get; set; }
        public int PanelAddress { get; set; }
        public int NumberOfLoops { get; set; }
        public int NumberOfZones { get; set; }
        public int ConfigGood { get; set; }
        public string FirmwareVersion { get; set; }

        public List<LoopData> Loops { get; set; }
        public List<BusData> Busses { get; set; }
        public List<CauseAndEffectData> CauseAndEffects { get; set; }

        public BlackBoxControlPanelData()
        {
            Loops = new List<LoopData>();
            Busses = new List<BusData>();
            CauseAndEffects = new List<CauseAndEffectData>();

        }
    }

    [Serializable]
    public class LoopData
    {
        public int LoopNumber { get; set; }
        public string LoopName { get; set; }
        public List<LoopDeviceData> Devices { get; set; }

        public LoopData()
        {
            Devices = new List<LoopDeviceData>();
        }
    }

    [Serializable]
    public class LoopDeviceData
    {
        public int Address { get; set; }
        public string Type { get; set; }
        public string LocationText { get; set; }
        public int Zone { get; set; }
        public string ImagePath { get; set; }
    }

    [Serializable]
    public class BusData
    {
        public int BusNumber { get; set; } 
        public string BusName { get; set; }
        public string BusType { get; set; } 
        public List<BusNodeData> Nodes { get; set; }

        public BusData()
        {
            Nodes = new List<BusNodeData>();
        }
    }

    [Serializable]
    public class BusNodeData
    {
        public int Address { get; set; }
        public string Name { get; set; }
        public string LocationText { get; set; }
        public string ImagePath { get; set; }
        public List<InputOutputData> Inputs { get; set; }
        public List<InputOutputData> Outputs { get; set; }

        public BusNodeData()
        {
            Inputs = new List<InputOutputData>();
            Outputs = new List<InputOutputData>();
        }
    }

    [Serializable]
    public class InputOutputData
    {
        public string Type { get; set; }
        public string Description { get; set; }
    }

    [Serializable]
    public class CauseAndEffectData
    {
        public string Name { get; set; }
        public string LogicGate { get; set; }
        public bool IsEnabled { get; set; }
        public List<CauseInputData> Inputs { get; set; } = new List<CauseInputData>();    
        public List<EffectOutputData> Outputs { get; set; } = new List<EffectOutputData>(); 
    }

    [Serializable]
    public class CauseInputData
    {
        public string InputType { get; set; } // "Device", "TimeOfDay", "DateTime", "ReceiveApi"

        // For Device Input
        public string DeviceId { get; set; }
        public string Type { get; set; }
        public string LocationText { get; set; }
        public string ImagePath { get; set; }

        // For Time of Day
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        // For DateTime
        public DateTime? TriggerDateTime { get; set; }

        // For Receive API
        public string ListenUrl { get; set; }
        public string HttpMethod { get; set; }
        public string ExpectedPath { get; set; }
        public string AuthToken { get; set; }
    }

    [Serializable]
    public class EffectOutputData
    {
        public string OutputType { get; set; } // "Device", "SendText", "SendEmail", "SendApi"

        // For Device Output
        public string DeviceId { get; set; }
        public string Type { get; set; }
        public string LocationText { get; set; }
        public string ImagePath { get; set; }

        // For Send Text
        public string PhoneNumber { get; set; }
        public string Message { get; set; }

        // For Send Email
        public string EmailAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        // For Send API
        public string ApiUrl { get; set; }
        public string HttpMethod { get; set; }
        public string ContentType { get; set; }
        public string RequestBody { get; set; }
    }

}
