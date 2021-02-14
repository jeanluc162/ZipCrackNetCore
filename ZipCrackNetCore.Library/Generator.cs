﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace ZipCrackNetCore.Library
{
    /// <summary>
    /// Enumerable collection of strings of a specified length made out of a specified charset
    /// </summary>
    public class Generator : IEnumerable<String>
    {
        private Int32 _Length;
        private Char[] _Charset;
        /// <summary>
        /// Constructor for a Generator
        /// </summary>
        /// <param name="Charset">The charset used to create the strings</param>
        /// <param name="Length">The length of the strings</param>
        public Generator(IEnumerable<Char> Charset, Int32 Length)
        {
            if (Charset == null) throw new ArgumentNullException("Charset"); //Charset can't be null
            List<Char> CharsetList = new List<Char>();
            foreach (Char c in Charset) CharsetList.Add(c);
            if (CharsetList.Count < 1) throw new ArgumentException("'Charset' has to contain at least one Element", "Charset"); //Charset can't be empty
            _Charset = CharsetList.ToArray();
            
            if(Length < 1) throw new ArgumentException("'Length' has to be > 0", "Length"); //Strings can't be 0 characters long
            _Length = Length;
        }
        public IEnumerator<String> GetEnumerator()
        {
            return new GeneratorEnumerator(_Charset, _Length);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        /// <summary>
        /// Enumerator for a Generator
        /// </summary>
        public class GeneratorEnumerator : IEnumerator<String>
        {
            private Int32[] _Positions;
            private Char[] _Charset;
            /// <summary>
            /// Constructor for a GeneratorEnumerator
            /// </summary>
            /// <param name="Charset">The charset used to create the strings</param>
            /// <param name="Length">The length of the strings</param>
            public GeneratorEnumerator(Char[] Charset, Int32 Length)
            {
                if (Charset == null) throw new ArgumentNullException("Charset"); //Charset can't be null
                if (Length < 1) throw new ArgumentException("'Length' has to be > 0", "Length"); //Strings can't be 0 characters long
                _Positions = new Int32[Length];

                if (Charset.Length < 1) throw new ArgumentException("'Charset' has to contain at least one Element", "Charset"); //Charset can't be Empty
                _Charset = Charset;

                Reset();
            }
            public string Current
            {
                get
                {
                    if (_Positions[_Positions.Length - 1] == -1) throw new Exception();

                    String CurrState = "";
                    for (int i = 0; i < _Positions.Length; i++) CurrState += _Charset[_Positions[i]];
                    System.Diagnostics.Debug.WriteLine("CombinationGenerator.Generator.Current: Returned String: " + CurrState);
                    return CurrState;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                
            }

            public bool MoveNext()
            {
                for (Int32 i = _Positions.Length - 1; i >= 0; i--)
                {
                    if (_Positions[i] == _Charset.Length - 1)
                    {
                        if (i == 0)
                        {
                            for (int j = 0; j < _Positions.Length; j++) _Positions[j] = _Charset.Length - 1;
                            return false;
                        }
                        _Positions[i] = 0;
                    }
                    else
                    {
                        _Positions[i]++;
                        break;
                    }
                }
                return true;
            }

            public void Reset()
            {
                for (int i = 0; i < _Positions.Length - 1; i++)
                {
                    _Positions[i] = 0;
                }
                _Positions[_Positions.Length - 1] = -1;
            }
        }
    }   
}
