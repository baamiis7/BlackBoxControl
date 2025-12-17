using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackBoxControl.Models
{
    public class InputOutputModule
    {
        public byte Type { get; set; } // IO module type
        public string Name { get; set; } // IO module name, limited to 15 characters
        public byte[] Dummy { get; set; } = new byte[14]; // Padding to align the structure to 32 bytes

        // Constructor
        public InputOutputModule(byte type, string name)
        {
            Type = type;
            Name = name.Length <= IOModuleConstants.IO_TEXT_LENGTH ? name : name.Substring(0, IOModuleConstants.IO_TEXT_LENGTH);
        }
        public string ImagePath { get; set; }
        public ObservableCollection<PanelInput> PanelInputs { get; set; }
        public ObservableCollection<PanelOutput> PanelOutputs { get; set; }
    }
}
