using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackBoxControl.Models
{
    internal class IOModuleConstants
    {
        // Maximum values
        public const int MAX_IO_MODULES = 32;   // Maximum number of modules fitted to a panel
        public const int NUM_IO_CHANNELS = 16;  // Number of channels per module

        // IO Module Text Length
        public const int IO_TEXT_LENGTH = 15;

        // IO Module Types
        public const byte IO_MOD_NOT_FITTED = 0;
        public const byte IO_MOD_MULTI_IO = 1;
        public const byte IO_MOD_NAC_IO = 2;
        public const byte IO_MOD_SOUNDER_BOARD = 3;
        public const byte IO_MOD_RELAY_BOARD = 4;
        public const byte IO_MOD_ZONE_IO = 5;
        public const byte IO_USA_ANNUNCIATOR = 6;
        public const byte IO_LISTEC = 7;
        public const byte IO_MOD_8ZONE_MIMIC = 8;
        public const byte IO_MOD_16ZONE_MIMIC = 9;
    }
}
