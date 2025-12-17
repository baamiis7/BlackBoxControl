using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackBoxControl.Models
{
    public static class CEConstants
    {
        // Cause and Effect Flags
        public const int CE_FLAG_LATCHING = 0x0001;
        public const int CE_FLAG_TRANSPARENT = 0x0002;
        public const int CE_FLAG_TIMED_OUTPUT = 0x0004;
        public const int CE_TEST_SOUNDERS_ON = 0x0008;
        public const int CE_TEST_PANEL_OUTPUTS_ON = 0x0010;
        public const int CE_TEST_LOOP_OUTPUTS_ON = 0x0020;
        public const int CE_TEST_INCLUDE_CALL_POINTS = 0x0040;
        public const int CE_TEST_SEND_TO_NETWORK = 0x0080;

        // Operators for Cause & Effect table
        public const int CE_OPERATOR_AND = 1;
        public const int CE_OPERATOR_OR = 2;
        public const int CE_OPERATOR_COINCIDENCE = 4;
        public const int CE_OPERATOR_BY_DEVICE = 16;
        public const int CE_OPERATOR_BY_DEVICE_INC_CP = 48; // Bits for 16 and 32 both set for this operator
    }

}
