using System.Xml;

namespace DDDSaveToolSharp.Core.Models
{
    public class PresetManager
    {
        public Preset Preset { get; set; }
        public Sav Sav { get; set; }

        public PresetManager()
        {
            Initialize();
        }

        void Initialize()
        {
            Preset = new Preset();
        }

        public void LoadSav(string savFile)
        {
            if (File.Exists(savFile))
                Sav.Load(savFile);
        }

        public void LoadPreset(string presetFile)
        {
            if (File.Exists(presetFile))
                Preset.Load(presetFile);
        }

        public void SetSav(Sav sav)
        {
            Sav = sav;
        }

        public void SetPreset(Preset preset)
        {
            Preset = preset;
        }

        public bool ReplaceAppearance(PlayerType target, XmlNode replacement)
        {
            bool result = false;

            if (replacement != null && Sav != null)
            {

                switch (target)
                {
                    case PlayerType.Arisen:
                        var playerAppearanceNode = Sav.XmlData.GetPlayerAppearanceNode();
                        var clone = Sav.XmlData.ImportNode(replacement, true);

                        if (clone.Attributes.Count > 0)
                            clone.Attributes["name"].Value = "mEditPl";
                        playerAppearanceNode.ParentNode.InsertBefore(clone, playerAppearanceNode);
                        playerAppearanceNode.ParentNode.RemoveChild(playerAppearanceNode);


                        result = true;
                        break;

                    case PlayerType.Pawn:
                        var pawnAppearanceNode = Sav.XmlData.GetPawnApperanceNode();
                        clone = Sav.XmlData.ImportNode(replacement, true);

                        if (clone.Attributes.Count > 0)
                            clone.Attributes["name"].Value = "mEditPawn";
                        pawnAppearanceNode.ParentNode.InsertBefore(clone, pawnAppearanceNode);
                        pawnAppearanceNode.ParentNode.RemoveChild(pawnAppearanceNode);

                        result = true;
                        break;
                }
            }

            return result;
        }

        public bool ReplaceStats(PlayerType target, XmlNode replacement)
        {
            bool result = false;

            if (replacement != null && Sav != null)
            {
                switch (target)
                {
                    case PlayerType.Arisen:
                        var playerStatsNode = Sav.XmlData.GetPlayerStatsNode();
                        var clone = Sav.XmlData.ImportNode(replacement, true);
                        playerStatsNode.ParentNode.InsertBefore(clone, playerStatsNode);
                        playerStatsNode.ParentNode.RemoveChild(playerStatsNode);

                        result = true;
                        break;

                    case PlayerType.Pawn:
                        var pawnStatsNode = Sav.XmlData.GetPawnStatsNode();
                        clone = Sav.XmlData.ImportNode(replacement, true);
                        pawnStatsNode.ParentNode.InsertBefore(clone, pawnStatsNode);
                        pawnStatsNode.ParentNode.RemoveChild(pawnStatsNode);

                        result = true;
                        break;
                }
            }

            return result;
        }

        public bool ApplyPreset(PlayerType target, ReplacementType replacementType)
        {
            bool result = false;

            if (Sav != null && Preset != null)
            {
                var appearanceNode = Preset.Appearance.SelectSingleNode(StringGlobal.UniAppearanceNode);
                var statsNode = Preset.Stats.SelectSingleNode(StringGlobal.UniStatsParamNode);

                switch (replacementType)
                {
                    case ReplacementType.All:
                        ReplaceAppearance(target, appearanceNode);
                        ReplaceStats(target, statsNode);

                        result = true;
                        break;

                    case ReplacementType.AppearanceOnly:
                        result = ReplaceAppearance(target, appearanceNode);
                        break;

                    case ReplacementType.StatsOnly:
                        result = ReplaceStats(target, statsNode);
                        break;
                }
            }
            return result;
        }
    }
}
