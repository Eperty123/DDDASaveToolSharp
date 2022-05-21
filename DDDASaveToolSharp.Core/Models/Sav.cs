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
using System;
using System.IO;
using System.Text;

namespace DDDSaveToolSharp.Core.Models
{
    public class Sav
    {
        /// <summary>
        /// The version of the save data (21 for DDDA console and DDDA PC, and 5 for original DD on console).
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// The real file size of the save data (xml) uncompressed.
        /// </summary>
        public uint RealSize { get; set; }

        /// <summary>
        /// The compressed file size of the save data (xml).
        /// </summary>
        public uint CompressedSize { get; set; }

        /// <summary>
        /// Always 860693325.
        /// </summary>
        public uint Unk2 { get; set; }

        /// <summary>
        /// Always 0.
        /// </summary>
        public uint Unk3 { get; set; }

        /// <summary>
        /// Always 860700740.
        /// </summary>
        public uint Unk4 { get; set; }

        /// <summary>
        /// Checksum of the compressed save data.
        /// </summary>
        public uint Hash { get; set; }

        /// <summary>
        /// Always 1079398965.
        /// </summary>
        public uint Unk5 { get; set; }

        /// <summary>
        /// The compressed byte array data.
        /// </summary>
        public byte[] CompressedData { get; set; }

        /// <summary>
        /// The decompressed byte array data.
        /// </summary>
        public byte[] DecompressedData { get; set; }

        /// <summary>
        /// The parsed header in bytes.
        /// </summary>
        public byte[] Header { get; set; }

        /// <summary>
        /// The parsed xml data.
        /// </summary>
        public SavXmlDocument XmlData { get; set; }

        /// <summary>
        /// The size of the header. Always 32 it seems.
        /// </summary>
        public static readonly int HeaderSize = 32;

        /// <summary>
        /// The total* size of the save data.
        /// </summary>
        public static readonly int SaveSize = 524288;

        /// <summary>
        /// The maximum length of any character names.
        /// </summary>
        public static readonly int MaxNameLength = 25;

        /// <summary>
        /// The checksum calculator for when repacking the decompressed xml into .sav.
        /// </summary>
        SavCrc _Checksum;

        /// <summary>
        /// Load a sav file from the specified path.
        /// </summary>
        /// <param name="file">The sav file to load.</param>
        public bool Load(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    using (var br = new BinaryReader(fs))
                    {
                        // Header size is 32 bytes.
                        Header = br.ReadBytes(HeaderSize);

                        // Start over.
                        br.BaseStream.Seek(0, SeekOrigin.Begin);

                        Version = br.ReadUInt32();
                        RealSize = br.ReadUInt32();
                        CompressedSize = br.ReadUInt32();
                        Unk2 = br.ReadUInt32();
                        Unk3 = br.ReadUInt32();
                        Unk4 = br.ReadUInt32();
                        Hash = br.ReadUInt32();
                        Unk5 = br.ReadUInt32();

                        // If not a pc save meaning an xbox save!
                        // Placeholder for now until I figure out what this actually does.
                        if (Version != 21)
                        {
                            Header.SwapHeaderEndian();
                            using (var ms = new MemoryStream(Header))
                            using (var hr = new BinaryReader(ms))
                            {
                                var swappedVersion = hr.ReadUInt32();
                                Version = swappedVersion;
                            }
                        }

                        _Checksum = new SavCrc();
                        CompressedData = br.ReadBytes((int)CompressedSize);
                        DecompressedData = CompressedData.Decompress();
                        ParseXml();

                        return true;
                    }
                }
            }
            catch (Exception)
            {

            }

            return false;
        }

        /// <summary>
        /// Repack and save the sav file.
        /// </summary>
        /// <param name="outputFile">The path to where to save the file.</param>
        public bool Save(string outputFile)
        {
            try
            {
                using (var fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                using (var bw = new BinaryWriter(fs))
                {
                    // Update all variables & data.
                    UpdateValues();

                    // Now finally write to file.
                    bw.Write(Header);
                    bw.Write(CompressedData);

                    // Then the padding.
                    var paddingSize = SaveSize - Header.Length - CompressedData.Length;
                    var paddingBytes = new byte[paddingSize];

                    bw.Write(paddingBytes);

                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        /// <summary>
        /// Get the embedded Steam Id.
        /// </summary>
        /// <returns></returns>
        public long GetSteamId()
        {
            return XmlData.GetSteamId();
        }

        /// <summary>
        /// Get the embedded player name.
        /// </summary>
        /// <returns></returns>
        public string GetPlayerName()
        {
            return XmlData.GetPlayerName();
        }

        /// <summary>
        /// Get the embedded Pawn name.
        /// </summary>
        /// <returns></returns>
        public string GetPawnName()
        {
            return XmlData.GetPawnName();
        }

        /// <summary>
        /// Set the sav file's Steam id.
        /// </summary>
        /// <param name="steamId">The Steam id to use.</param>
        /// <returns></returns>
        public bool SetSteamId(long steamId)
        {
            return XmlData.SetSteamId(steamId);
        }

        /// <summary>
        /// Set the player name.
        /// </summary>
        /// <param name="steamId">The name of the player to use.</param>
        /// <returns></returns>
        public bool SetPlayerName(string playerName)
        {
            return XmlData.SetPlayerName(playerName);
        }

        /// <summary>
        /// Set the pawn's name.
        /// </summary>
        /// <param name="steamId">The name of the pawn to use.</param>
        /// <returns></returns>
        public bool SetPawnName(string pawnName)
        {
            return XmlData.SetPawnName(pawnName);
        }

        /// <summary>
        /// Export the sav file as xml.
        /// </summary>
        /// <param name="outputFile">The path to save the xml.</param>
        /// <returns></returns>
        public bool ExportXml(string outputFile)
        {
            if (XmlData != null)
            {
                XmlData.Save(outputFile);
                return true;
            }
            return false;
        }

        public bool IsLoaded()
        {
            return XmlData != null;
        }

        /// <summary>
        /// Parse the decompressed xml data.
        /// </summary>
        void ParseXml()
        {
            if (DecompressedData != null && DecompressedData.Length > 0)
            {
                using (var ms = new MemoryStream(DecompressedData))
                using (var br = new BinaryReader(ms))
                {
                    var xmlString = Encoding.UTF8.GetString(br.ReadBytes(DecompressedData.Length));

                    // Encode the XML string in a UTF-8 byte array
                    // Reference: https://stackoverflow.com/questions/310669/why-does-c-sharp-xmldocument-loadxmlstring-fail-when-an-xml-header-is-included.
                    byte[] encodedString = Encoding.UTF8.GetBytes(xmlString);

                    // Put the byte array into a stream and rewind it to the beginning
                    using (var utf8Stream = new MemoryStream(encodedString))
                    {
                        utf8Stream.Flush();
                        utf8Stream.Position = 0;

                        // Build the XmlDocument from the MemorySteam of UTF-8 encoded bytes
                        XmlData = new SavXmlDocument();
                        XmlData.Load(utf8Stream);
                    }
                }
            }
        }

        /// <summary>
        /// Update variables.
        /// </summary>
        void UpdateValues()
        {
            // Get xml data as bytes.
            var xmlBytes = GetXmlDataAsBytes();
            RealSize = (uint)xmlBytes.Length;

            // Now prepare for repack.
            CompressedData = xmlBytes.Compress();
            DecompressedData = xmlBytes;

            CompressedSize = (uint)CompressedData.Length;

            // Update all header values.
            Version = 21;
            Unk2 = 860693325;
            Unk3 = 0;
            Unk4 = 860700740;
            Unk5 = 1079398965;
            Hash = _Checksum.CalculateChecksum(CompressedData, CompressedData.Length);
            Header = GetHeaderAsBytes();

            // Swap the header endian if not a pc save.
            // Automated to make things easier!
            if (Version != 21) Header.SwapHeaderEndian();
        }

        /// <summary>
        /// Generate header as byte array.
        /// </summary>
        /// <returns></returns>
        byte[] GetHeaderAsBytes()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(Version);
                bw.Write(RealSize);
                bw.Write(CompressedSize);
                bw.Write(Unk2);
                bw.Write(Unk3);
                bw.Write(Unk4);
                bw.Write(Hash);
                bw.Write(Unk5);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Get the xml data as byte array.
        /// </summary>
        /// <returns></returns>
        byte[] GetXmlDataAsBytes()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                XmlData.Save(ms);
                return ms.ToArray();
            }
        }

    }
}