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
        public string LoopName { get; set; }
        public string LoopProtocol { get; set; }
        public string ImagePath { get; set; }
        public ObservableCollection<LoopDevice> Devices { get; set; } = new ObservableCollection<LoopDevice>();  // Keep this as List
        public int NumberOfDevices { get; set; }
    }
}
