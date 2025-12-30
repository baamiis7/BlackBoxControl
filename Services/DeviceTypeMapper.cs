using System;

namespace BlackBoxControl.Services
{
    /// <summary>
    /// Single source of truth for device type code mappings
    /// </summary>
    public static class DeviceTypeMapper
    {
        public static byte GetDeviceTypeCode(string deviceType)
        {
            if (string.IsNullOrEmpty(deviceType))
                return 0xFF;

            // Normalize the input - remove spaces, underscores, convert to lowercase
            var normalized = deviceType.ToLower().Trim().Replace(" ", "").Replace("_", "");

            return normalized switch
            {
                // Detectors
                "smokedetector" => 0x01,
                "heatdetector" => 0x02,
                "opticaldetector" => 0x03,

                // Call Points
                "manualcallpoint" => 0x10,
                "callpoint" => 0x10,
                "wirelesscallpoint" => 0x11,

                // Sounders
                "basesounder" => 0x20,
                "sounder" => 0x20,
                "redsounder" => 0x21,
                "whitesounder" => 0x22,
                "sounderbeacon" => 0x23,
                "wirelessbasesounder" => 0x24,
                "wirelessbasewithled" => 0x25,

                // Beacons
                "beacon" => 0x30,

                // Modules
                "expandermodule" => 0x40,
                "inputmodule" => 0x41,
                "outputmodule" => 0x42,
                "remoteindicator" => 0x43,
                "wirelessexpandermodule" => 0x44,
                "wirelesstranslator" => 0x45,
                "wirelesstranslatormodule" => 0x46,
                "singlechannelbatterypoweredoutput" => 0x47,
                "wirelesssinglechannelinputmodule" => 0x48,

                // Wireless Detectors
                "wirelessheatdetector" => 0x50,
                "wirelessmultidetector" => 0x51,
                "wirelessopticaldetector" => 0x52,

                _ => 0xFF
            };
        }

        public static string GetDeviceTypeName(byte deviceTypeCode)
        {
            return deviceTypeCode switch
            {
                // Detectors
                0x01 => "Smoke Detector",
                0x02 => "Heat Detector",
                0x03 => "Optical Detector",

                // Call Points
                0x10 => "Manual Call Point",
                0x11 => "Wireless Callpoint",

                // Sounders
                0x20 => "Base Sounder",
                0x21 => "Red Sounder",
                0x22 => "White Sounder",
                0x23 => "Sounder Beacon",
                0x24 => "Wireless Base Sounder",
                0x25 => "Wireless Base With LED",

                // Beacons
                0x30 => "Beacon",

                // Modules
                0x40 => "Expander Module",
                0x41 => "Input Module",
                0x42 => "Output Module",
                0x43 => "Remote Indicator",
                0x44 => "Wireless Expander Module",
                0x45 => "Wireless Translator",
                0x46 => "Wireless Translator Module",
                0x47 => "Single Channel Battery Powered Output",
                0x48 => "Wireless Single Channel Input Module",
                

                // Wireless Detectors
                0x50 => "Wireless Heat Detector",
                0x51 => "Wireless Multi Detector",
                0x52 => "Wireless Optical Detector",

                _ => "Unknown Device"
            };
        }

        public static string GetDeviceImagePath(string deviceType)
        {
            if (string.IsNullOrEmpty(deviceType))
                return "/Images/Unknown_Device.png";

            var imageMap = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Detectors
                { "Smoke Detector", "/Images/Smoke_Detector.PNG" },
                { "Heat Detector", "/Images/Heat_Detector.PNG" },
                { "Optical Detector", "/Images/Optical_Detector.PNG" },
                
                // Call Points
                { "Manual Call Point", "/Images/Call_Point.PNG" },
                { "Call Point", "/Images/Call_Point.PNG" },
                { "Wireless Callpoint", "/Images/Wireless_Callpoint.PNG" },
                
                // Sounders
                { "Base Sounder", "/Images/Base_Sounder.PNG" },
                { "Sounder", "/Images/Base_Sounder.PNG" },
                { "Red Sounder", "/Images/Red_Sounder.PNG" },
                { "White Sounder", "/Images/WhiteSounder.PNG" },
                { "Sounder Beacon", "/Images/Sounder_Beacon.PNG" },
                { "Wireless Base Sounder", "/Images/Wireless_Base_Sounder.PNG" },
                { "Wireless Base With LED", "/Images/Wireless_Base_With_LED.PNG" },
                
                // Beacons
                { "Beacon", "/Images/Beacon.PNG" },
                
                // Modules
                { "Expander Module", "/Images/Expander_Module.PNG" },
                { "Input Module", "/Images/Wireless_Expander_Module.PNG" },
                { "Output Module", "/Images/Single_Channel_Battery_Powered_Output.PNG" },
                { "Remote Indicator", "/Images/Remote_Indicator.PNG" },
                { "Wireless Expander Module", "/Images/Wireless_Expander_Module.PNG" },
                { "Wireless Translator", "/Images/Wireless_Translator.PNG" },
                { "Wireless Translator Module", "/Images/Wireless_Translator_Module.PNG" },
                { "Single Channel Battery Powered Output", "/Images/Single_Channel_Battery_Powered_Output.PNG" },
                { "Wireless Single Channel Input Module", "/Images/Wireless_single_Channel_Input_Module.PNG" },
                
                
                // Wireless Detectors
                { "Wireless Heat Detector", "/Images/Wireless_Heat_Detector.PNG" },
                { "Wireless Multi Detector", "/Images/Wireless_Multi_Detector.PNG" },
                { "Wireless Optical Detector", "/Images/Wireless_Optical_Detector.PNG" }
            };

            if (imageMap.TryGetValue(deviceType, out string imagePath))
            {
                return imagePath;
            }

            return "/Images/Unknown_Device.png";
        }
    }
}
