using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackBoxControl.Models
{
    public class SubAddress
    {
        public string Type { get; set; }
        public double AnalogValue { get; set; }
        public double DeviceThreshold { get; set; }
        public double DeviceDaySensitivity { get; set; }
        public double DeviceNightSensitivity { get; set; }
        public string DeviceInputAction { get; set; }
        public string DeviceActionMessage { get; set; }
        public string ImagePath { get; set; } // Add this property for the image path
        public int Zone { get; set; }               // INT in C, which maps to int in C#
        public byte Loop { get; set; }              // BYTE in C, which maps to byte in C#
        public byte Address { get; set; }           // BYTE in C, which maps to byte in C#
        public char[] LocationText { get; set; }    // CHAR array in C, represented as char[] in C#
        public int FFlags { get; set; }             // INT in C, which maps to int in C# (Extra F added to avoid conflict)
        public byte SubType { get; set; }           // BYTE in C, which maps to byte in C#
        public byte InputAction { get; set; }       // BYTE in C, which maps to byte in C#
        public byte InputActionMsg { get; set; }    // BYTE in C, which maps to byte in C# (Index to message displayed)
        public byte Xplorer { get; set; }           // BYTE in C, which maps to byte in C#
        public uint DelayStage1 { get; set; }       // UINT in C, which maps to uint in C# (0-300 seconds)
        public uint DelayStage2 { get; set; }       // UINT in C, which maps to uint in C# (0-300 seconds)
        public uint DaySensitivity { get; set; }    // UINT in C, which maps to uint in C# (Day sensitivity)
        public uint NightSensitivity { get; set; }  // UINT in C, which maps to uint in C# (Night sensitivity)
        public byte Data1 { get; set; }             // BYTE in C, which maps to byte in C#
        public byte Data2 { get; set; }             // BYTE in C, which maps to byte in C#
        public byte Data3 { get; set; }             // BYTE in C, which maps to byte in C#
    }
}
