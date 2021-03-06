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

using System;

namespace DDDSaveToolSharp.Core.Utilities
{
    public static class SavUtility
    {
        /// <summary>
        /// Swap endian bytes for the given byte array.
        /// </summary>
        /// <param name="header">The byte array to swap around.</param>
        public static void SwapHeaderEndian(this byte[] header)
        {
            for (int i = 0; i < header.Length; i += 4)
            {
                byte c1 = header[i];
                byte c2 = header[i + 1];
                header[i] = header[i + 3];
                header[i + 1] = header[i + 2];
                header[i + 2] = c2;
                header[i + 3] = c1;
            }
        }

        /// <summary>
        /// Convert the given byte array into hex string.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] data)
        {
            return BitConverter.ToString(data, 0).Replace("-", " ");
        }

        /// <summary>
        /// Reference: https://localcoder.org/cast-negative-number-to-unsigned-types-ushort-uint-or-ulong
        /// </summary>
        /// <typeparam name="T">The generic type to cast to.</typeparam>
        /// <param name="number">The number to cast.</param>
        /// <returns></returns>
        public static object CastToUnsigned<T>(this T number) where T : struct
        {
            unchecked
            {
                switch (number)
                {
                    case long xlong: return (ulong)xlong;
                    case int xint: return (uint)xint;
                    case short xshort: return (ushort)xshort;
                    case sbyte xsbyte: return (byte)xsbyte;
                }
            }
            return number;
        }
    }
}
