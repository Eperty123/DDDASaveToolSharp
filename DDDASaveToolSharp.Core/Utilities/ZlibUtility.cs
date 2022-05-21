/*
/* MIT License
/* 
/* Copyright (c) 2016-2022 Eperty123
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

using Ionic.Zlib;
using System.IO;

namespace DDDSaveToolSharp.Core.Utilities
{
    public static class ZlibUtility
    {
        public static byte[] Compress(this byte[] uncompressed, CompressionLevel compressionLevel = CompressionLevel.Default)
        {
            using (var compressedStream = new MemoryStream())
            using (var zlibStream = new ZlibStream(compressedStream, CompressionMode.Compress, compressionLevel))
            {
                zlibStream.Write(uncompressed, 0, uncompressed.Length);
                zlibStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static byte[] Decompress(this byte[] compressed)
        {
            using (var resultStream = new MemoryStream())
            using (var compressedStream = new MemoryStream(compressed))
            using (var zlibStream = new ZlibStream(compressedStream, CompressionMode.Decompress))
            {
                zlibStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }
    }
}
