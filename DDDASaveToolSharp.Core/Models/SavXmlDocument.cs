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

using System.Xml;

namespace DDDSaveToolSharp.Core.Models
{
    public class SavXmlDocument : XmlDocument
    {
        XmlWriterSettings _WriterSettings = new XmlWriterSettings
        {
            Indent = true,
            //IndentChars = "  ",
            //NewLineChars = "\r\n",
            //NewLineHandling = NewLineHandling.Replace,
            //Encoding = new UTF8Encoding(false),
        };

        public XmlNode GetNode(string nodeName)
        {
            return SelectSingleNode(nodeName);
        }

        public XmlNodeList GetNodes(string nodeName)
        {
            return SelectNodes(nodeName);
        }

        public XmlAttribute GetNodeAttribute(string nodeName, string attributeName)
        {
            return GetNode(nodeName).Attributes[attributeName];
        }

        public void SetNodeAttribute(string nodeName, string attributeName, object value)
        {
            GetNode(nodeName).Attributes[attributeName].Value = value.ToString();
        }

        public XmlNode ReplaceNode(string nodeName, XmlNode replacement)
        {
            var foundNode = GetNode(nodeName);
            if (foundNode != null)
            {
                //foundNode.RemoveAll();
                var clone = ImportNode(replacement, true);
                foundNode.ParentNode.InsertBefore(clone, foundNode);
                foundNode.ParentNode.RemoveChild(foundNode);
            }
            return foundNode;
        }

        public XmlNode GetPlayerAppearanceNode()
        {
            return GetNode(StringGlobal.PlayerAppearanceNode);
        }

        public XmlNode GetPlayerStatsNode()
        {
            return GetNode(StringGlobal.PlayerStatsNode);
        }

        public XmlNode GetPlayerNameNode()
        {
            return GetNode($"{StringGlobal.PlayerAppearanceNode}/array[@name='(u8*)mNameStr']");
        }

        public XmlNode GetPawnApperanceNode()
        {
            return GetNode(StringGlobal.PawnAppearanceNode);
        }

        public XmlNode GetPawnStatsNode()
        {
            return GetNode(StringGlobal.PawnStatsNode);
        }

        public XmlNode GetPawnNameNode()
        {
            return GetNode($"{StringGlobal.PawnAppearanceNode}/array[@name='(u8*)mNameStr']");
        }


        public string GetPlayerName()
        {
            return string.Join("", GetLettersFromNameNode(GetPlayerNameNode()));
        }

        public string GetPawnName()
        {
            return string.Join("", GetLettersFromNameNode(GetPawnNameNode()));
        }

        public long GetSteamId()
        {
            var steamIdNode = GetNodeAttribute(StringGlobal.SteamIdNode, "value");
            long.TryParse(steamIdNode.Value, out var steamId);
            return steamId;
        }

        public bool SetSteamId(long steamId)
        {
            var steamIdNode = GetNodeAttribute(StringGlobal.SteamIdNode, "value");
            if (steamIdNode != null)
            {
                steamIdNode.Value = steamId.ToString();
                return steamIdNode.Value.Equals(steamId.ToString());
            }

            return false;
        }

        public bool SetPlayerName(string playerName)
        {
            if (playerName.Length > 25)
                return false;

            var nameNode = GetPlayerNameNode();
            if (nameNode != null) return SetName(playerName, nameNode) != null;

            return false;
        }

        public bool SetPawnName(string pawnName)
        {
            if (pawnName.Length > 25)
                return false;

            var nameNode = GetPawnNameNode();
            if (nameNode != null) return SetName(pawnName, nameNode) != null;
            return false;
        }

        public override void Save(string filename)
        {
            using (XmlWriter writer = XmlWriter.Create(filename, _WriterSettings))
            {
                Save(writer);
            }
        }

        XmlNode SetName(string name, XmlNode nameNode)
        {
            if (nameNode != null)
            {
                nameNode.RemoveAll();

                // Re-add removed attributes.
                var nameAttr = CreateAttribute("name");
                var typeAttr = CreateAttribute("type");
                var countAttr = CreateAttribute("count");

                nameAttr.Value = "(u8*)mNameStr";
                typeAttr.Value = "u8";
                countAttr.Value = Sav.MaxNameLength.ToString();
                nameNode.Attributes.Append(nameAttr);
                nameNode.Attributes.Append(typeAttr);
                nameNode.Attributes.Append(countAttr);

                var asciiName = GetAsciiNumericValueFromName(name).ToList();
                for (int i = 0; i < asciiName.Count; i++)
                {
                    var number = asciiName[i];
                    var u8 = CreateElement("u8");
                    u8.SetAttribute("name", number.ToString());

                    nameNode.AppendChild(u8);
                }
            }

            return nameNode;
        }

        IEnumerable<int> GetAsciiNumericValueFromName(string name)
        {
            try
            {
                var nameInInt = new List<int>();
                foreach (char letter in name)
                    nameInInt.Add(letter);

                if (nameInInt.Count < Sav.MaxNameLength)
                {
                    int missingPaddingAmount = Sav.MaxNameLength - nameInInt.Count;
                    for (int i = 0; i < missingPaddingAmount; i++)
                        nameInInt.Add(0);
                }
                return nameInInt;
            }
            catch (Exception)
            {
                return null;
            }
        }


        IEnumerable<char> GetLettersFromNameNode(XmlNode nameNode)
        {
            try
            {
                List<char> letters = new List<char>();
                foreach (XmlNode letterNode in nameNode)
                {
                    var valueAttribute = letterNode.Attributes["value"];
                    if (valueAttribute != null)
                    {
                        var letterValue = valueAttribute.Value;
                        int.TryParse(letterValue, out var letterInt);
                        if (letterInt != 0) letters.Add(Convert.ToChar(letterInt));
                        else break;
                    }
                }

                return letters;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
