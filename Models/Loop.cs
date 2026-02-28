using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackBoxControl.Models
{
    public class Loop
    {
        public int LoopNumber { get; set; }
        public string LoopName { get; set; } = string.Empty;
        public string LoopProtocol { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public ObservableCollection<LoopDevice> Devices { get; set; } = new ObservableCollection<LoopDevice>();  // Keep this as List
        public int NumberOfDevices { get; set; }
    }
}
