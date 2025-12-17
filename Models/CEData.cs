using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackBoxControl.Models
{
    public class CEData
    {
        private int _ceFixedIndex;  // Index of CEFixed that this address belongs to
        private byte _addressType;
        private byte _data1;        // Panel, DOW, Start Hour
        private byte _data2;        // Loop, Zone, IOMod, Start Min, Local IO
        private byte _data3;        // Address, IO Chan, End Hour
        private byte _data4;        // Sub Address, End Min

        public int CEFixedIndex
        {
            get { return _ceFixedIndex; }
            set { _ceFixedIndex = value; }
        }

        public byte AddressType
        {
            get { return _addressType; }
            set { _addressType = value; }
        }

        public byte Data1
        {
            get { return _data1; }
            set { _data1 = value; }
        }

        public byte Data2
        {
            get { return _data2; }
            set { _data2 = value; }
        }

        public byte Data3
        {
            get { return _data3; }
            set { _data3 = value; }
        }

        public byte Data4
        {
            get { return _data4; }
            set { _data4 = value; }
        }
    }

}
