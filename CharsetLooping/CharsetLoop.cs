using System;
using System.Collections.Generic;
using System.Text;

namespace CharsetLooping
{
    public class CharsetLoop
    {
        public static readonly char[] CharsetAlphabetLowerCase = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        public static readonly char[] CharsetAlphabetUpperCase = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        public static readonly char[] CharsetNumbers = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        private Char[] _Charset;
        private Int32[] _Positions;
        public Char[] Charset
        {
            get
            {
                return _Charset;
            }
        }
        public String StateStart
        {
            get;
            private set;
        }
        public String State
        {
            get
            {
                String CurrState = "";
                for (int i = 0; i < _Positions.Length; i++) CurrState += _Charset[_Positions[i]];
                return CurrState;
            }
        }
        public CharsetLoop(Char[] Charset, Int32 StringLength)
        {
            if (Charset == null) throw new ArgumentNullException("Charset");
            if (Charset.Length < 1) throw new ArgumentException("Length of \"Charset\" must be greater than 0!");
            if (StringLength < 1) throw new ArgumentException("\"StringLength\" must be greater than 0!");

            _Charset = Charset;
            _Positions = new Int32[StringLength];

            for (Int32 i = 0; i < _Positions.Length; i++) _Positions[i] = 0;

            StateStart = State;
        }
        public String Loop(Int32 LoopCount = 1)
        {
            for(Int32 i = 0; i < LoopCount; i++)
            {
                for(Int32 j = _Positions.Length -1; j >= 0; j--)
                {
                    if (_Positions[j] == _Charset.Length - 1) _Positions[j] = 0;
                    else
                    {
                        _Positions[j]++;
                        break;
                    }
                }
            }

            return State;
        }
    }
}
