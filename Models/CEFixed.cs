using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackBoxControl.Models
{
    public class CEFixed
    {
        private byte _ceType;
        private string _name;                // C# string to hold Name (you can limit the length if needed)
        private byte _operator;
        private byte _outputType;
        private ushort _firstCauseIndex;
        private ushort _lastCauseIndex;
        private ushort _firstEffectIndex;
        private ushort _lastEffectIndex;
        private ushort _ceFlags;
        private ushort _effectTime;
        private ushort _ceConfigID;

        public byte CEType
        {
            get { return _ceType; }
            set { _ceType = value; }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value.Length > Constants.DISPLAY_WIDTH) // Validate length if needed
                    throw new ArgumentException($"Name cannot exceed {Constants.DISPLAY_WIDTH} characters.");
                _name = value;
            }
        }

        public byte Operator
        {
            get { return _operator; }
            set { _operator = value; }
        }

        public byte OutputType
        {
            get { return _outputType; }
            set { _outputType = value; }
        }

        public ushort FirstCauseIndex
        {
            get { return _firstCauseIndex; }
            set { _firstCauseIndex = value; }
        }

        public ushort LastCauseIndex
        {
            get { return _lastCauseIndex; }
            set { _lastCauseIndex = value; }
        }

        public ushort FirstEffectIndex
        {
            get { return _firstEffectIndex; }
            set { _firstEffectIndex = value; }
        }

        public ushort LastEffectIndex
        {
            get { return _lastEffectIndex; }
            set { _lastEffectIndex = value; }
        }

        public ushort CEFlags
        {
            get { return _ceFlags; }
            set { _ceFlags = value; }
        }

        public ushort EffectTime
        {
            get { return _effectTime; }
            set { _effectTime = value; }
        }

        public ushort CEConfigID
        {
            get { return _ceConfigID; }
            set { _ceConfigID = value; }
        }
    }

}
