using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackBoxControl.Models
{
    
    public class BlackBoxControlPanel
    {
        public int PanelAddress { get; set; }
        public int NumberOfLoops { get; set; }
        public string PanelName { get; set; }
        public string Location { get; set; }

        // 0x55 = good, anything else bad
        public int ConfigGood { get; set; }

        public byte LPSDefVol { get; set; }
        public byte LPSDefTone { get; set; }
        public ushort RingMode { get; set; }  // WORD in C++ -> ushort in C#
        public ushort TSYear { get; set; }
        public ushort TSMonth { get; set; }
        public ushort TSDay { get; set; }
        public ushort TSHour { get; set; }
        public ushort TSMinute { get; set; }

        public short NumSubDevices { get; set; } // INT16 in C++ -> short in C#
        public short PanelProtocol { get; set; }
        public short[] NightStart { get; set; } = new short[7];
        public short[] DayStart { get; set; } = new short[7];
        public short CalibrationTime { get; set; }
        public short NumZones { get; set; }

        public string SupplierName { get; set; } = new string(' ', Constants.DISPLAY_WIDTH + 1); // DISPLAY_WIDTH should be defined
        public string NodeName { get; set; } = new string(' ', Constants.NODE_NAME_LEN + 1); // NODE_NAME_LEN should be defined
        public string Access3Password { get; set; } = new string(' ', Constants.ACCESS_PASS_LEN + 1); // ACCESS_PASS_LEN should be defined
        public string Access2Password { get; set; } = new string(' ', Constants.ACCESS_PASS_LEN + 1);

        public byte NumLoops { get; set; }
        public byte NumIOModules { get; set; }
        public short NumCE { get; set; }
        public short NumCECauses { get; set; }
        public short NumCEEffects { get; set; }
        public short NodeAddress { get; set; }  // Network address
        public short NetCardAddress { get; set; }  // Network card address. Equals NodeAddress if card fitted, else 0

        public bool ISDevices { get; set; } // Intrinsically safe devices fitted
        public byte LPSToneFire { get; set; }
        public byte LPSToneEvac { get; set; }
        public byte LPSToneAlert { get; set; }

        public byte LPSTonePreAl { get; set; }
        public byte LPSToneTechAl { get; set; }
        public byte LPSToneFault { get; set; }
        public byte LPSToneSecurity { get; set; }

        public bool ModemAttachedFlash { get; set; }

        public short LoopOffset { get; set; }
        public byte GraphicsSupport { get; set; }
        public byte[] Kentec { get; set; } = new byte[Constants.COPYRIGHT_LEN]; // COPYRIGHT_LEN should be defined
        public short ProgCheckSum { get; set; }
        public byte DayLightSavingEnabled { get; set; }
        public byte CoinMode { get; set; }
        public byte SilCoinMode { get; set; }
        public byte DisplayInvert { get; set; }
        public int ZoneOffset { get; set; }
        public byte FirstIgnoredNode { get; set; }
        public byte SecondIgnoredNode { get; set; }
        public int MaxZoneLEDs { get; set; }
        public byte DelayActiveOnInit { get; set; }
        public byte ShowDisEventForDelay { get; set; }
        public byte AbandonShipMode { get; set; }
        public byte MasterNode { get; set; }
        public byte SilOtherMode { get; set; }
        public int NumberOfPanelInputs { get; set; }
        public int NumberOfPanelOutputs { get; set; }
        public int NumberOfCauseAndEffects { get; set; }
        public int NumberOfZones { get; set; }
        public string WiFiSSID { get; set; }
        public string WiFiPassword { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
        public string FirmwareVersion { get; set; }
        public string ImagePath { get; set; }
        public ObservableCollection<Loop> Loops { get; set; }
        public ObservableCollection<CEFixed> CauseAndEffects { get; set; }
        public ObservableCollection<PanelInput> PanelInputs { get; set; }
        public ObservableCollection<PanelOutput> PanelOutputs { get; set; }
        public ObservableCollection<InputOutputModule> InputOutputModules { get; set; }
        public string IPAddress { get; internal set; }
    }
}
