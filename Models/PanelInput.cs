using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackBoxControl.Models
{
    public class PanelInput
    {
        public uint Type { get; set; } // Type of IO Module
        public int Zone { get; set; } // Zone associated with the IO Module
        public string LocationText { get; set; } // Location description text
        public bool IsAnOutput { get; set; } // True or False indicating if the device is an output
        public int FFlags { get; set; } // Flags for the device
        public byte InputAction { get; set; } // Input action type
        public byte InputActionMsg { get; set; } // Index to message displayed when input is operated
        public uint DelayStage1 { get; set; } // Input delay (0 - 180 seconds)
        public uint DelayStage2 { get; set; } // Second stage delay (0 - 300 seconds)
        public int Data1 { get; set; } // Pulsed output pulse time

        // Constructor
        public PanelInput(uint type, int zone, string locationText, bool isAnOutput, int fFlags,
                                byte inputAction, byte inputActionMsg, uint delayStage1,
                                uint delayStage2, int data1)
        {
            Type = type;
            Zone = zone;
            LocationText = locationText.Length <= Constants.DISPLAY_WIDTH ? locationText : locationText.Substring(0, Constants.DISPLAY_WIDTH);
            IsAnOutput = isAnOutput;
            FFlags = fFlags;
            InputAction = inputAction;
            InputActionMsg = inputActionMsg;
            DelayStage1 = delayStage1;
            DelayStage2 = delayStage2;
            Data1 = data1;
        }
    }        
    
}
