// ArmorySetInfo.cs "$Revision: 2537 $" "$Date: 2019-08-22 20:44:20 +0300 (to, 22 elo 2019) $"
// https://www.ownedcore.com/forums/diablo-3/turbohud/turbohud-community-plugins/774800-8-0-armorysetinfo.html
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.JarJar.DefaultUI
{
    internal class ArmorySetInfo : BasePlugin, IKeyEventHandler, IInGameTopPainter
    {
        private const string LEFT = "\u25C0";                       // black left-pointing pointer

        // Matching options                                         // In addition to equipped items:
        public bool MatchPlayerSkills { get; set; } = true;         // - match Player Skills (6)
        public bool MatchCubePowers { get; set; } = true;           // - match Cube Powers (3)
        public bool MatchPassivePowers { get; set; } = true;        // - match Passive Skills (4)

        // Keyboard.
        public bool UseToggleLabelsKey { get; set; } = true;        // Are "labels" toggleable.
        public Key ToggleLabelsKey { get; set; } = Key.Divide;      // Key to use toggle "ShowArmorySetNumberLabels".

        // Item borders.
        public bool EquippedBorderEnabled { get; set; } = true;     // Equipped items get light (almost invisible) border if in armory and default border if not.
        public bool StashBorderEnabled { get; set; } = true;        // Armory set items in stash get red border.
        public bool InventoryBorderEnabled { get; set; } = true;    // Armory set items in inventory get teal border.
        
        // Item labels (toggleable).
        public bool ShowArmorySetNumberLabels { get; set; } = true; // Show Armory Set number labels under items.

        // Armory Set name list in left side of the screen (toggleable).
        public bool ShowArmorySetNamesLeft { get; set; } = true;    // Show Armory Set name list on left side of the screen.

        // Armory Set name(s) in Inventory header.
        public bool ShowArmorySetNamesHeader { get; set; } = true;  // Show Armory Set names that have equipped items.
        public bool ShowOnlyMatchingArmorySetNames { get; set; } = true; // Show only matching Armory Set names.
        public string ArmorySetLabelFormat { get; set; } = "{0:00} {1} ({2})";  // Armory Set number, name and equipped item count.
        public string LeftSideLabelZero { get; set; } = "-";        // Zero (0) character for left side labels.

        // Armory Set name selection filter.
        public uint ArmorySetMinNameLen { get; set; } = 2;          // Armory set names shorter than this will be ignored.
        public int ArmorySetMinCountToShow { get; set; } = 5;       // Minimimun Armory Set item count to show.

        public string MatchingSetIndicator { get; set; } = LEFT;    // Matching Armory Set indicator.

        public float ArmorySetHeaderRatioX { get; set; } = 0.62f;   // % X-offset for "ShowArmorySetNamesHeader".
        public float ArmorySetHeaderRatioY { get; set; } = 0.07f;   // % Y-offset baseline.

        public float ArmorySetLabelRatioX { get; set; } = 0.005f;   // % X-offset for "ShowArmorySetNamesLeft" and "ShowGemNamesLeft".
        public float ArmorySetLabelRatioY { get; set; } = 0.35f;    // % Y-offset.

        public IFont ArmorySetLabelFont { get; set; }
        public IFont ArmorySetNameFont { get; set; }

        public IBrush EquippedBorderBrush { get; set; }
        public IBrush StashBorderBrush { get; set; }
        public IBrush InventoryBorderBrush { get; set; }
        public IBrush NotArmorySetBorderBrush { get; set; }

        private SimpleLabel labelArmorySetNamesLeft;
        private StringBuilder labelTextHeader = new StringBuilder();
        private StringBuilder labelTextLeft = new StringBuilder();
        private int stashPage, stashTab, stashTabAbs;
        private float rv;
        private StringBuilder builder = new StringBuilder();
        private List<int> setNumbers = new List<int>(16);
        private uint requiredItemCount;
        private uint matchingSetCount;
        private uint[] equippedItemCount;
        private bool[] isMatchingSet;
        private bool[] isMatchingSkills;
        private bool[] isMatchingPowers;
        private bool[] isMatchingPassiveSkills;

        public ArmorySetInfo() { Enabled = true; }

        public override void Load(IController hud)
        {
            base.Load(hud);

            var len = Hud.Game.Me.ArmorySets.Length;
            equippedItemCount = new uint[len];
            isMatchingSet = new bool[len];
            isMatchingSkills = new bool[len];
            isMatchingPowers = new bool[len];
            isMatchingPassiveSkills = new bool[len];

            var x = Hud.Window.Size.Width * ArmorySetLabelRatioX;
            var y = Hud.Window.Size.Height * ArmorySetLabelRatioY;
            labelArmorySetNamesLeft = new SimpleLabel(Hud, "tahoma", 7, SharpDX.Color.PaleTurquoise).WithPosition(x, y);

            ArmorySetLabelFont = Hud.Render.CreateFont("arial", 7, 255, 192, 192, 192, true, false, 220, 32, 32, 32, true); // light grey on drak grey
            ArmorySetNameFont = Hud.Render.CreateFont("tahoma", 7, 255, 0xff, 0xd7, 0x00, false, false, true);              // gold

            EquippedBorderBrush = Hud.Render.CreateBrush(150, 238, 232, 170, -1.6f);    // pale golden rod (quite light)
            StashBorderBrush = Hud.Render.CreateBrush(150, 0, 192, 128, -1.6f);         // teal - but lighter
            InventoryBorderBrush = Hud.Render.CreateBrush(200, 255, 0, 0, -1.6f);       // red
            NotArmorySetBorderBrush = Hud.Render.CreateBrush(100, 255, 51, 0, -1.6f);   // orangish transparent
        }

        private void clearMatches()
        {
            Array.Clear(equippedItemCount, 0, equippedItemCount.Length);
            Array.Clear(isMatchingSet, 0, isMatchingSet.Length);
            Array.Clear(isMatchingSkills, 0, isMatchingSkills.Length);
            Array.Clear(isMatchingPowers, 0, isMatchingPowers.Length);
            Array.Clear(isMatchingPassiveSkills, 0, isMatchingPassiveSkills.Length);
        }

        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (UseToggleLabelsKey && keyEvent.IsPressed && keyEvent.Key == ToggleLabelsKey)
            {
                ShowArmorySetNumberLabels = !ShowArmorySetNumberLabels;
                ShowArmorySetNamesLeft = !ShowArmorySetNamesLeft;
            }
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.Inventory) return;

            var uiInv = Hud.Inventory.InventoryMainUiElement;
            if (!uiInv.Visible) return;

            stashTab = Hud.Inventory.SelectedStashTabIndex;
            stashPage = Hud.Inventory.SelectedStashPageIndex;
            stashTabAbs = stashTab + stashPage * Hud.Inventory.MaxStashTabCountPerPage;

            rv = 32.0f / 600.0f * Hud.Window.Size.Height;

            // Reset data.
            requiredItemCount = 13;
            matchingSetCount = 0;
            clearMatches();
            // Checked all items.
            var items = Hud.Game.Items.Where(x => x.Location != ItemLocation.Merchant && x.Location != ItemLocation.Floor);
            foreach (var item in items)
            {
                if (item.Location == ItemLocation.Stash)
                {
                    var tabIndex = item.InventoryY / 10;
                    if (tabIndex != stashTabAbs) continue;
                }
                if ((item.InventoryX < 0) || (item.InventoryY < 0)) continue;

                var rect = Hud.Inventory.GetItemRect(item);
                if (rect == System.Drawing.RectangleF.Empty) continue;
                if (item.Location == ItemLocation.Stash || item.Location == ItemLocation.Inventory)
                {
                    if (item.Unidentified ||
                        item.SnoItem.Kind == ItemKind.gem ||
                        item.SnoItem.Kind == ItemKind.uberstuff ||
                        item.SnoItem.Kind == ItemKind.potion ||
                        item.SnoItem.MainGroupCode == "gems_unique" ||
                        item.SnoItem.MainGroupCode == "potion" ||
                        item.SnoItem.MainGroupCode == "healthpotions" ||
                        item.SnoItem.MainGroupCode == "consumable" ||
                        item.SnoItem.MainGroupCode == "horadriccache")
                    {
                        continue;
                    }
                }
                checkForArmorySet(item, rect);
            }
            // All items checked.
            if (ShowArmorySetNamesLeft || ShowArmorySetNamesHeader)
            {
                var stash = Hud.Inventory.StashMainUiElement;
                labelTextHeader.Clear();
                labelTextLeft.Clear();
                for (var i = 0; i < Hud.Game.Me.ArmorySets.Length; ++i)
                {
                    var armorySet = Hud.Game.Me.ArmorySets[i];
                    if (armorySet != null && armorySet.Name.Length >= ArmorySetMinNameLen)
                    {
                        var index = armorySet.Index + 1;
                        // Inventory header.
                        if (ShowArmorySetNamesHeader && equippedItemCount[i] > 0)
                        {
                            if (isMatchingSet[i] || !ShowOnlyMatchingArmorySetNames)
                            {
                                if (equippedItemCount[i] >= ArmorySetMinCountToShow)
                                {
                                    labelTextHeader.AppendFormat(ArmorySetLabelFormat, index, armorySet.Name, equippedItemCount[i]);
                                    if (isMatchingSet[i])
                                    {
                                        if (!string.IsNullOrWhiteSpace(MatchingSetIndicator))
                                        {
                                            labelTextHeader.Append(' ').Append(MatchingSetIndicator);
                                        }
                                    }
                                    labelTextHeader.AppendLine();
                                }
                            }
                        }
                        if (!stash.Visible)
                        {
                            // Left side Armory Set list.
                            if (ShowArmorySetNamesLeft)         // Toggleable with ToggleLabelsKey
                            {
                                if (equippedItemCount[i] >= ArmorySetMinCountToShow)
                                {
                                    labelTextLeft.AppendFormat(ArmorySetLabelFormat, index, armorySet.Name, formatEquippedItemCount(equippedItemCount[i]));
                                    if (isMatchingSet[i])
                                    {
                                        if (!string.IsNullOrWhiteSpace(MatchingSetIndicator))
                                        {
                                            labelTextLeft.Append(' ').Append(MatchingSetIndicator);
                                        }
                                    }
                                    labelTextLeft.AppendLine();
                                }
                            }
                        }
                    }
                }
                if (labelTextHeader.Length > 0)
                {
                    // Vertically centered from "ArmorySetHeaderRatioY".
                    var layout = ArmorySetNameFont.GetTextLayout(labelTextHeader.ToString());
                    var x = uiInv.Rectangle.Left + (uiInv.Rectangle.Width * ArmorySetHeaderRatioX);
                    var y = uiInv.Rectangle.Top + (uiInv.Rectangle.Height * ArmorySetHeaderRatioY) - (layout.Metrics.Height / 2f);
                    if (y < uiInv.Rectangle.Top) y = uiInv.Rectangle.Top;
                    ArmorySetNameFont.DrawText(layout, x, y);
                }
                if (labelTextLeft.Length > 0)
                {
                    labelArmorySetNamesLeft.PaintLeft(labelTextLeft.ToString());
                }
            }
        }

        private string formatEquippedItemCount(uint count)
        {
            return count == 0 ? LeftSideLabelZero : count.ToString();
        }

        private void checkForArmorySet(IItem item, System.Drawing.RectangleF rect)
        {
            setNumbers.Clear();
            var player = Hud.Game.Me;
            for (var i = 0; i < player.ArmorySets.Length; ++i)
            {
                var armorySet = player.ArmorySets[i];
                if (armorySet != null && armorySet.Name.Length >= ArmorySetMinNameLen)
                {
                    if (armorySet.ContainsItem(item))
                    {
                        setNumbers.Add(armorySet.Index + 1);
                        if (item.Location >= ItemLocation.Head && item.Location <= ItemLocation.Neck)
                        {
                            equippedItemCount[i] += 1;
                            if (item.Location == ItemLocation.LeftHand && item.SnoItem.MainGroupCode == "2h" && requiredItemCount == 13)
                            {
                                requiredItemCount = 12;     // Left hand is empty!
                            }
                            if (equippedItemCount[i] == requiredItemCount)
                            {
                                // Save matching armory set - it it truly matches.
                                matchingSetCount += 1;
                                if (MatchPlayerSkills || MatchCubePowers || MatchPassivePowers)
                                {
                                    isMatchingSkills[i] = MatchPlayerSkills ? hasSameSkills(player, armorySet) : true;
                                    isMatchingPowers[i] = isMatchingSkills[i] && MatchCubePowers ? hasSameCubePowers(player, armorySet) : true;
                                    isMatchingPassiveSkills[i] = isMatchingPowers[i] && MatchPassivePowers ? hasSamePassivePowers(player, armorySet) : true;
                                    isMatchingSet[i] = isMatchingPassiveSkills[i];
                                }
                                else
                                {
                                    isMatchingSet[i] = true;
                                }
                            }
                        }
                    }
                }
            }
            if (setNumbers.Count == 0)
            {
                if (EquippedBorderEnabled && !(item.Location == ItemLocation.Stash || item.Location == ItemLocation.Inventory))
                {
                    NotArmorySetBorderBrush.DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height);
                }
                return;
            }
            if (item.Location == ItemLocation.Stash)
            {
                if (StashBorderEnabled) StashBorderBrush.DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height);
            }
            else if (item.Location == ItemLocation.Inventory)
            {
                if (InventoryBorderEnabled) InventoryBorderBrush.DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height);
            }
            else
            {
                if (EquippedBorderEnabled) EquippedBorderBrush.DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height);
            }
            if (ShowArmorySetNumberLabels)      // Toggleable with ToggleLabelsKey
            {
                builder.Length = 0;
                builder.Append('#');
                var endIndex = setNumbers.Count - 1;
                var current = 0;
                try
                {
                    while (current <= endIndex)
                    {
                        var first = current;
                        var last = current + 1;
                        var delta = 1;
                        if (last <= endIndex && (setNumbers[last] - setNumbers[first]) == delta)
                        {
                            while (last + 1 <= endIndex && (setNumbers[last + 1] - setNumbers[first]) == delta + 1)
                            {
                                last += 1;
                                delta += 1;
                            }
                            builder.Append(setNumbers[first])
                                .Append(delta == 1 ? ',' : '-')
                                .Append(setNumbers[last]).Append(',');
                            current = last + 1;
                        }
                        else
                        {
                            builder.Append(setNumbers[first]).Append(',');
                            current += 1;
                        }
                    }
                    builder.Length -= 1;
                }
                catch (Exception ex)
                {
                    Hud.Debug("Error " + ex.ToString());
                    builder.Append("E:").Append(setNumbers.Count);
                }
                var upper = item.Location == ItemLocation.Stash || item.Location == ItemLocation.Inventory;
                var font = ArmorySetLabelFont;
                var text = builder.ToString();
                var textLayout = font.GetTextLayout(text);
                var x = upper ? rect.Left + rv / 15.0f : rect.Right - textLayout.Metrics.Width - rv / 15.0f;
                var y = upper ? rect.Top + rv / 35.0f : rect.Bottom - 2 * (rv / 35.0f);
                font.DrawText(textLayout, x, y);
            }
        }

        private bool hasSameSkills(IPlayer player, IPlayerArmorySet set)
        {
            return isEqual(player.Powers.SkillSlots[(int)ActionKey.Skill1], set.Skill1SnoPower, set.Skill1Rune) &&
                isEqual(player.Powers.SkillSlots[(int)ActionKey.Skill2], set.Skill2SnoPower, set.Skill2Rune) &&
                isEqual(player.Powers.SkillSlots[(int)ActionKey.Skill3], set.Skill3SnoPower, set.Skill3Rune) &&
                isEqual(player.Powers.SkillSlots[(int)ActionKey.Skill4], set.Skill4SnoPower, set.Skill4Rune) &&
                isEqual(player.Powers.SkillSlots[(int)ActionKey.LeftSkill], set.LeftSkillSnoPower, set.LeftSkillRune) &&
                isEqual(player.Powers.SkillSlots[(int)ActionKey.RightSkill], set.RightSkillSnoPower, set.RightSkillRune);
        }

        private bool hasSameCubePowers(IPlayer player, IPlayerArmorySet set)
        {
            return isEqual(player.CubeSnoItem1, set.CubeSnoItem1) && 
                isEqual(player.CubeSnoItem2, set.CubeSnoItem2) && 
                isEqual(player.CubeSnoItem3, set.CubeSnoItem3);
        }

        private bool hasSamePassivePowers(IPlayer player, IPlayerArmorySet set)
        {
            return isEqual(player.Powers.PassiveSlots[0], set.PassiveSnoPower1) &&
                isEqual(player.Powers.PassiveSlots[1], set.PassiveSnoPower2) &&
                isEqual(player.Powers.PassiveSlots[2], set.PassiveSnoPower3) &&
                isEqual(player.Powers.PassiveSlots[3], set.PassiveSnoPower4);
        }

        private bool isEqual(IPlayerSkill skill, ISnoPower power, byte rune)
        {
            if (skill != null && power != null)
            {
                return skill.SnoPower.Sno == power.Sno && skill.Rune == rune;
            }
            return skill == null && power == null ? true : false;
        }

        private bool isEqual(ISnoPower left, ISnoPower right)
        {
            if (left != null && right != null)
            {
                return left.Sno == right.Sno;
            }
            return left == null && right == null ? true : false;
        }

        private bool isEqual(ISnoItem left, ISnoItem right)
        {
            if (left != null && right != null)
            {
                return left.Sno == right.Sno;
            }
            return left == null && right == null ? true : false;
        }

        private class SimpleLabel
        {
            public IFont TextFont;
            public float X;
            public float Y;

            public SimpleLabel(IController hud, string fontFamily, float size, SharpDX.Color textColor, bool bold = false)
                :
                this(hud.Render.CreateFont(fontFamily, size, textColor.A, textColor.R, textColor.G, textColor.B, bold, false, true))
            { }

            public SimpleLabel(IFont font)
            {
                TextFont = font;
            }

            public SimpleLabel WithPosition(float x, float y)
            {
                X = x;
                Y = y;
                return this;
            }

            public SharpDX.DirectWrite.TextMetrics GetTextMetrics(string text)
            {
                return TextFont.GetTextLayout(text).Metrics;
            }

            public float PaintLeft(string text)
            {
                return PaintLeft(X, Y, text);
            }
            public float PaintLeft(float x, float y, string text)
            {
                var layout = TextFont.GetTextLayout(text);
                TextFont.DrawText(layout, x, y);
                return layout.Metrics.Height;
            }

            public float PaintRight(string text)
            {
                return PaintRight(X, Y, text);
            }
            public float PaintRight(float x, float y, string text)
            {
                var layout = TextFont.GetTextLayout(text);
                TextFont.DrawText(layout, x - layout.Metrics.Width, y - layout.Metrics.Height);
                return layout.Metrics.Height;
            }
        }
    }
}