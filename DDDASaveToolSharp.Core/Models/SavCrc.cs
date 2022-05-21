/*
/* MIT License
/* 
/* Copyright (c) 2016-2022 Eperty123, FluffyQuack
/* 
/* Permission is hereby granted, free of charge, to any person obtaining a copy
/* of this software and associated documentation files (the "Software"), to deal
/* in the Software without restriction, including without limitation the rights
/* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/* copies of the Software, and to permit persons to whom the Software is
/* furnished to do so, subject to the following conditions:
/* 
/* The above copyright notice and this permission notice shall be included in all
/* copies or substantial portions of the Software.
/* 
/* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/* SOFTWARE.
*/

using DDDSaveToolSharp.Core.Utilities;

namespace DDDSaveToolSharp.Core.Models
{
    public class SavCrc
    {
        uint[] _Crc32Table = new uint[256];
        uint _Polynomial = 0xEDB88320;

        public SavCrc()
        {
            Generate();
        }

        /// <summary>
        /// Calculate a checksum from a given byte array and size.
        /// </summary>
        /// <param name="block">The input byte array to calculate checksum from.</param>
        /// <param name="size">The size of the bytes to calculate checksum for.</param>
        /// <returns></returns>
        public uint CalculateChecksum(byte[] block, int size)
        {
            Generate();

            uint x = (uint)SavUtility.CastToUnsigned(-1); //initial value
            uint c = 0;

            while (c < size)
            {
                x = ((x >> 8) ^ _Crc32Table[(x ^ block[c]) & 255]);
                c++;
            }
            return x;
        }

        /// <summary>
        /// Generate checksum.
        /// </summary>
        void Generate()
        {
            uint x;
            uint c;
            uint b;
            c = 0;

            while (c <= 255)
            {
                x = c;
                b = 0;
                while (b <= 7)
                {
                    if ((x & 1) != 0)
                    {
                        x = ((x >> 1) ^ _Polynomial); //polynomial
                    }
                    else
                    {
                        x = (x >> 1);
                    }
                    b++;
                }
                _Crc32Table[c] = x;
                c++;
            }
        }
    }
}
