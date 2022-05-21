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

using System.Text;
using System.Xml;

namespace DDDSaveToolSharp.Core.Models
{
    public class Preset : SavXmlDocument
    {
        public XmlNode Appearance { get; set; }
        public XmlNode Stats { get; set; }

        public Preset()
        {
            Initialize();
        }

        public Preset(string presetFile)
        {
            Initialize();
            Load(presetFile);
        }

        void Initialize()
        {
            CreateRootData();
            CreateAppearanceData();
            CreateStatsData();
        }

        public override void Load(string xmlFile)
        {
            if (File.Exists(xmlFile))
            {
                var xmlString = File.ReadAllText(xmlFile);

                // Encode the XML string in a UTF-8 byte array
                // Reference: https://stackoverflow.com/questions/310669/why-does-c-sharp-xmldocument-loadxmlstring-fail-when-an-xml-header-is-included.
                byte[] encodedString = Encoding.UTF8.GetBytes(xmlString);

                // Put the byte array into a stream and rewind it to the beginning
                using (var utf8Stream = new MemoryStream(encodedString))
                {
                    utf8Stream.Flush();
                    utf8Stream.Position = 0;

                    // Build the XmlDocument from the MemorySteam of UTF-8 encoded bytes
                    base.Load(utf8Stream);

                    ParsePresetData();
                }
            }
        }

        void ParsePresetData()
        {
            Appearance = GetNode(".//AppearanceData");
            Stats = GetNode(".//StatsData");
        }

        void CreateRootData()
        {
            XmlDeclaration xmlDeclaration = CreateXmlDeclaration("1.0", "UTF-8", null);
            InsertBefore(xmlDeclaration, DocumentElement);

            var appearanceElement = CreateElement("PresetData");
            AppendChild(appearanceElement);
        }

        void CreateAppearanceData()
        {
            Appearance = CreateElement("AppearanceData");
            DocumentElement.AppendChild(Appearance);
        }

        void CreateStatsData()
        {
            Stats = CreateElement("StatsData");
            DocumentElement.AppendChild(Stats);
        }

        public void SetAppearanceData(XmlNode appearance)
        {
            if (appearance != null)
            {
                Appearance.RemoveAll();
                var node = Appearance.OwnerDocument.ImportNode(appearance, true);
                node.Attributes["name"].Value = "mUni";
                Appearance.AppendChild(node);
            }
        }

        public void SetStatsData(XmlNode stats)
        {
            if (stats != null)
            {
                Stats.RemoveAll();
                var node = Appearance.OwnerDocument.ImportNode(stats, true);
                Stats.AppendChild(node);
            }
        }

        public bool HasAppearanceData()
        {
            return Appearance != null && Appearance.Name.Equals("AppearanceData");
        }

        public bool HasStatsData()
        {
            return Stats != null && Stats.Name.Equals("StatsData");
        }

        public bool IsLoaded()
        {
            return HasAppearanceData() && Appearance.HasChildNodes && HasStatsData() && Stats.HasChildNodes;
        }

        public void Clear()
        {
            Appearance.RemoveAll();
            Stats.RemoveAll();
            RemoveAll();
        }

        public static Preset GenerateFromSavFile(Sav sav, PlayerType targetPlayer)
        {
            var preset = new Preset();
            XmlNode appearanceData = null;
            XmlNode statsData = null;

            switch (targetPlayer)
            {
                case PlayerType.Arisen:
                    appearanceData = sav.XmlData.GetPlayerAppearanceNode();
                    statsData = sav.XmlData.GetPlayerStatsNode();
                    break;

                case PlayerType.Pawn:
                    appearanceData = sav.XmlData.GetPawnApperanceNode();
                    statsData = sav.XmlData.GetPawnStatsNode();
                    break;
            }
            preset.SetAppearanceData(appearanceData);
            preset.SetStatsData(statsData);

            return preset;
        }
    }
}
