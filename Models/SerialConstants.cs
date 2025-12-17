using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackBoxControl.Models
{
    public static class SerialConstants
    {
        // Transmission and reception lengths
        public const int ConsoleTxLength = 2500; // Length of transmission buffer
        public const int ConsoleRxLength = 255;  // Length of reception buffer (must be less than 256)

        // Printer power timeout
        public const int PrinterPowerTimeout = 30; // Time in seconds before printer power is switched off

        // Communication destinations
        public const int DestinationPrinter = 0;
        public const int DestinationPC = 1;
        public const int DestinationModem = 2;
        public const int DestinationHeart = 3;

        // Debug mode can be routed to printer or PC
        public const int DestinationDebug = DestinationPrinter;
    }

    // Enum for serial receive states
    public enum SerRxState
    {
        WaitSync,    // Waiting for sync signal
        MsgId,       // Waiting for message ID
        NodeId,      // Waiting for node ID
        PacketType,  // Waiting for packet type
        Length,      // Waiting for length information
        Data,        // Receiving data
        Checksum,    // Checking packet integrity
        Process      // Processing received data
    }
}


