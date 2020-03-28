// RunStats Bag Space Helper by Resu

using Turbo.Plugins.Default;
using System.Drawing;
//using SharpDX;
using SharpDX.DirectInput;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Threading;

namespace Turbo.Plugins.LiveStats.Modules
{
    public class LiveStats_BagSpaceHelper : BasePlugin, ICustomizer, IInGameTopPainter, IItemPickedHandler //, IKeyEventHandler
    {
        public int FSsAtStart { get; set; } = -1;
        public int FSsLastSeen { get; set; }
        public int FSsSpent { get; set; }
        public int FSsGained { get; set; }
        public float SquareSide { get; set; }
        public int freeSpace { get; set; }
        public int freeSpaceTwo { get; set; }
        public Dictionary<string, string> InventorySlots;
        public bool InventoryOpen { get; set; }
        public int CachesCount { get; set; }
        public int CachesLoopCount { get; set; }
        public string HeroName { get; set; }
        public bool Go { get; set; }

        public int MaxFSsGainable { get; set; } = 60; //max number of FSs gainable in a fraction of a second (sometimes the game client takes a moment to load the current material count after the game has started)
        
        public TopLabelDecorator Label { get; set; }
        public IFont FSFont { get; set; }
        public IBrush BgBrush { get; set; }
        public IBrush BgBrushAlt { get; set; }
        public String WarningSign { get; set; }

        public int Priority { get; set; } = 1;
        public int Hook { get; set; } = 0;

        public LiveStats_BagSpaceHelper()
        {
            Enabled = true;
        }
        
        public void Customize()
        {
            // Add this display to the RunStats readout
            Hud.RunOnPlugin<LiveStatsPlugin>(plugin => {
                plugin.Add(this.Label, this.Priority, this.Hook);
            });
            // Turn off Default InventoryFreeSpacePlugin
            Hud.TogglePlugin<InventoryFreeSpacePlugin>(false);

        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            
            FSFont = Hud.Render.CreateFont("tahoma", 7, 255, 0, 255, 127, false, false, true);
           // FSFontYellow = Hud.Render.CreateFont("tahoma", 7, 255, 200, 205, 50, false, false, true);
           // FSFontRed = Hud.Render.CreateFont("tahoma", 8, 255, 255, 100, 100, false, false, true);

            HeroName = string.Empty;
            WarningSign = "\u26A0 ";

            InventorySlots = new Dictionary<string, string>
            {
             {"C0R0", string.Empty}, {"C0R1", string.Empty}, {"C0R2", string.Empty}, {"C0R3", string.Empty}, {"C0R4", string.Empty}, {"C0R5", string.Empty},
             {"C1R0", string.Empty}, {"C1R1", string.Empty}, {"C1R2", string.Empty}, {"C1R3", string.Empty}, {"C1R4", string.Empty}, {"C1R5", string.Empty},
             {"C2R0", string.Empty}, {"C2R1", string.Empty}, {"C2R2", string.Empty}, {"C2R3", string.Empty}, {"C2R4", string.Empty}, {"C2R5", string.Empty},
             {"C3R0", string.Empty}, {"C3R1", string.Empty}, {"C3R2", string.Empty}, {"C3R3", string.Empty}, {"C3R4", string.Empty}, {"C3R5", string.Empty},
             {"C4R0", string.Empty}, {"C4R1", string.Empty}, {"C4R2", string.Empty}, {"C4R3", string.Empty}, {"C4R4", string.Empty}, {"C4R5", string.Empty},
             {"C5R0", string.Empty}, {"C5R1", string.Empty}, {"C5R2", string.Empty}, {"C5R3", string.Empty}, {"C5R4", string.Empty}, {"C5R5", string.Empty},
             {"C6R0", string.Empty}, {"C6R1", string.Empty}, {"C6R2", string.Empty}, {"C6R3", string.Empty}, {"C6R4", string.Empty}, {"C6R5", string.Empty},
             {"C7R0", string.Empty}, {"C7R1", string.Empty}, {"C7R2", string.Empty}, {"C7R3", string.Empty}, {"C7R4", string.Empty}, {"C7R5", string.Empty},
             {"C8R0", string.Empty}, {"C8R1", string.Empty}, {"C8R2", string.Empty}, {"C8R3", string.Empty}, {"C8R4", string.Empty}, {"C8R5", string.Empty},
             {"C9R0", string.Empty}, {"C9R1", string.Empty}, {"C9R2", string.Empty}, {"C9R3", string.Empty}, {"C9R4", string.Empty}, {"C9R5", string.Empty}
            };

            var plugin = Hud.GetPlugin<LiveStatsPlugin>();
            BgBrush = plugin.BgBrush; 
            BgBrushAlt = plugin.BgBrushAlt;
            
            int delta = this.FSsGained - this.FSsSpent;
            Label = new TopLabelDecorator(Hud)
            {
                TextFont = FSFont,
            //BackgroundBrush = BgBrush,
            TextFunc = () => freeSpaceTwo == int.MaxValue ? "\u25A0" + freeSpace + "  \u25AE" + "?" : WarningSign +"\u25A0" + freeSpace + "  \u25AE" + freeSpaceTwo.ToString("D", CultureInfo.InvariantCulture),
                
                

                ExpandUpLabels = new List<TopLabelDecorator>() {
                    new TopLabelDecorator(Hud) {
                        TextFont = FSFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont = FSFont,
                        TextFunc = () => freeSpaceTwo == int.MaxValue ? "?" : freeSpaceTwo.ToString("D", CultureInfo.InvariantCulture),
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.8),
                        HintFunc = () => "Space left for 2 slot items",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont = FSFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = FSFont,
                        TextFunc = () => freeSpace.ToString(),
                        ExpandedHintWidthMultiplier =(float)((Hud.Window.Size.Width / 1000)* 1.8),
                        HintFunc = () => "Space left for 1 slot items",
                    },
                },
            };
            
        }

        public void PaintTopInGame(ClipState clipState)
        {
            var uiRect = Hud.Render.InGameBottomHudUiElement.Rectangle;
            freeSpace = Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed;

            if (Hud.Time.Now.Second % 2 == 0)
                WarningSign = "\u26A0 ";
            else
                WarningSign = "\u2800\u2800 ";

            if (freeSpaceTwo > 2) WarningSign = "\u2800\u2800 ";

            if (HeroName != Hud.Game.Me.HeroName)
            {
                freeSpaceTwo = int.MaxValue;
                foreach (var key in InventorySlots.Keys.ToList()) // empty dictionary values
                {
                    InventorySlots[key] = string.Empty;
                }
                HeroName = Hud.Game.Me.HeroName;
                Go = false;
            }
            if (clipState == ClipState.Inventory)
                Go = true;
            if (!Go)
                goto SwitchCharacter;

            var ContainerRect = Hud.Inventory.InventoryItemsUiElement.Rectangle;

            var ItemCheck = Hud.Game.Items.FirstOrDefault(i => i.Location == ItemLocation.Inventory);
            if (ItemCheck == null)
            {
                freeSpaceTwo = int.MaxValue;
            }
            else
            {
                var Rect = Hud.Inventory.GetItemRect(ItemCheck);
                SquareSide = Rect.Width;
            }

            if (SquareSide == 0f || ContainerRect == null)
            {
                freeSpaceTwo = int.MaxValue;
            }
            else if (clipState != ClipState.Inventory)
            {
                InventoryOpen = false;
            }
            else if (clipState == ClipState.Inventory)
            {
                InventoryOpen = true;
                float FirstSquareTop = ContainerRect.Top;
                float FirstSquareLeft = ContainerRect.Left;

                var Items = Hud.Game.Items.Where(i => i.Location == ItemLocation.Inventory);

                foreach (var key in InventorySlots.Keys.ToList()) // empty dictionary values before filling
                {
                    InventorySlots[key] = string.Empty;
                }

                foreach (var Item in Items)
                {
                    var ItemRect = Hud.Inventory.GetItemRect(Item);

                    for (int c = 0; c < 10; c++) // 10 columns
                    {
                        for (int r = 0; r < 6; r++) // 6 rows
                        {
                            float DatSquareTop = FirstSquareTop + (SquareSide * r);
                            float DatSquareLeft = FirstSquareLeft + (SquareSide * c);


                            if (Math.Abs(ItemRect.Height - SquareSide) < 1)
                            {
                                if (Math.Abs(ItemRect.Top - DatSquareTop) < 4 && Math.Abs(ItemRect.Left - DatSquareLeft) < 4) //populate inventory slot
                                {
                                    string DatKey = "C" + c + "R" + r;
                                    InventorySlots[DatKey] = Item.SnoItem.Sno.ToString() + Item.CreatedAtInGameTick.ToString(); //Item.ItemUniqueId;
                                }

                            }
                            else
                            {
                                if (Math.Abs(ItemRect.Top - DatSquareTop) < 4 && Math.Abs(ItemRect.Left - DatSquareLeft) < 4) //populate 2 inventory slots
                                {
                                    string DatKey = "C" + c + "R" + r;
                                    InventorySlots[DatKey] = Item.SnoItem.Sno.ToString() + Item.CreatedAtInGameTick.ToString(); //Item.ItemUniqueId;
                                    string DatKeyTwo = "C" + c + "R" + (r + 1);
                                    InventorySlots[DatKeyTwo] = Item.SnoItem.Sno.ToString() + Item.CreatedAtInGameTick.ToString(); //Item.ItemUniqueId;
                                }
                            }
                        }
                    }
                }
            }

            if (SquareSide != 0f && ContainerRect != null) // calculate the freespacetwo value
            {
                var TwoSlotsCount = 0;
                for (int c = 0; c < 10; c++) // 10 columns
                {
                    for (int r = 0; r < 6; r++) // 6 rows
                    {
                        string DatKey = "C" + c + "R" + r;
                        string DatValue = InventorySlots[DatKey];
                        if (DatValue == string.Empty && r < 5)
                        {
                            string DatKeyTwo = "C" + c + "R" + (r + 1);
                            string DatValueTwo = InventorySlots[DatKeyTwo];
                            if (DatValueTwo == string.Empty)
                            {
                                TwoSlotsCount++;
                                r++;
                            }
                        }
                    }
                }

                freeSpaceTwo = TwoSlotsCount;

                //  Horadric Cache Workaround
                if (clipState != ClipState.Inventory)
                {
                    CachesLoopCount = 0;
                    var HoradricCaches = Hud.Inventory.ItemsInInventory.Where(x => x.SnoItem.MainGroupCode == "horadriccache");
                    foreach (var item in HoradricCaches)
                    {
                        CachesLoopCount++;
                    }

                    CachesLoopCount = Math.Abs(CachesLoopCount);

                    if (CachesLoopCount > CachesCount)
                    {
                        int HoradricCachesToAdd = CachesLoopCount - CachesCount;
                        for (int h = 0; h < HoradricCachesToAdd; h++)
                        {
                            for (int r = 0; r < 5; r++) // 5 rows (we don't need the last one)
                            {
                                for (int c = 0; c < 10; c++) // 10 columns
                                {
                                    string DatKey = "C" + c + "R" + r;
                                    string DatValue = InventorySlots[DatKey];
                                    string DatKeyTwo = "C" + c + "R" + (r + 1);
                                    string DatValueTwo = InventorySlots[DatKeyTwo];
                                    if (DatValue == string.Empty && DatValueTwo == string.Empty)
                                    {
                                        InventorySlots[DatKey] = "HoradricWorkaroundFromHell";
                                        InventorySlots[DatKeyTwo] = "HoradricWorkaroundFromHell";
                                        goto OuterFromHell;
                                    }
                                }
                            }
                            OuterFromHell:
                            ;
                        }
                    }

                }

            }
            CachesCount = CachesLoopCount;

            SwitchCharacter:
            ;


        }


        public void OnItemPicked(IItem item)
        {
            if (!InventoryOpen)
            {
                string ItemID = item.SnoItem.Sno.ToString() + item.CreatedAtInGameTick.ToString();
                if (item.SnoItem.MainGroupCode == "helm" || item.SnoItem.MainGroupCode == "chestarmor" ||
                    item.SnoItem.MainGroupCode == "gloves" || item.SnoItem.MainGroupCode == "boots" || item.SnoItem.MainGroupCode == "shoulders" ||
                    item.SnoItem.MainGroupCode == "pants" || item.SnoItem.MainGroupCode == "bracers" || item.SnoItem.MainGroupCode == "crusadershield" ||
                    item.SnoItem.MainGroupCode == "quiver" || item.SnoItem.MainGroupCode == "source" || item.SnoItem.MainGroupCode == "mojo" ||
                    item.SnoItem.MainGroupCode == "shield" || item.SnoItem.MainGroupCode == "necromanceroffhand" || item.SnoItem.MainGroupCode == "1h" ||
                    item.SnoItem.MainGroupCode == "2h") // 2 slot items
                {
                    for (int r = 0; r < 5; r++) // 5 rows (we don't need the last one)
                    {
                        for (int c = 0; c < 10; c++) // 10 columns
                        {
                            string DatKey = "C" + c + "R" + r;
                            string DatValue = InventorySlots[DatKey];
                            string DatKeyTwo = "C" + c + "R" + (r + 1);
                            string DatValueTwo = InventorySlots[DatKeyTwo];
                            if (DatValue == string.Empty && DatValueTwo == string.Empty)
                            {
                                InventorySlots[DatKey] = ItemID;
                                InventorySlots[DatKeyTwo] = ItemID;
                                goto Outer;
                            }
                        }
                    }
                }
                else if (item.SnoItem.MainGroupCode != "horadriccache")// 1 slot item
                {
                    for (int c = 0; c < 10; c++) // 10 columns
                    {
                        for (int r = 0; r < 6; r++) // 6 rows
                        {
                            string DatKey = "C" + c + "R" + r;
                            string DatValue = InventorySlots[DatKey];
                            if (DatValue == string.Empty)
                            {
                                InventorySlots[DatKey] = ItemID;
                                goto Outer;
                            }
                        }
                    }
                }
                Outer:
                ;
            }
        }




    }
}