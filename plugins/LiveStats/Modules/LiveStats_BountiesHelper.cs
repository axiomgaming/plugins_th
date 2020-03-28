// RunStats Bounties Helper by Resu

using Turbo.Plugins.Default;
using System.Drawing;
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
    public class LiveStats_BountiesHelper : BasePlugin, ICustomizer, IInGameTopPainter, INewAreaHandler, IChatLineChangedHandler
    {
        public int BountyHistoryTimeout { get; set; } = 0;	// Stop showing old events after this many seconds (0 = never time out)
        public int BountyHistoryHighlight { get; set; } = 20;	// Highlight if the event is new for this many seconds
        public int BountyHistoryShown { get; set; } = 6;	// Max # of events to show for bounty history

        public List<BountySnapshot> BountyHistory { get; private set; }
        
        public TopLabelDecorator Label { get; set; }
        public List<TopLabelDecorator> ExpandUpLabels { get; private set; }
        
        public IFont WhiteFont { get; set; }
        public IFont DefaultFont { get; set; }
        public IFont YellowFont { get; set; }
        public IFont PurpleFont { get; set; }
        public IFont RedFont { get; set; }
        public IFont GreenFont { get; set; }

        public IFont BlueFont { get; set; }
        public IFont TimeFont { get; set; }
        public IBrush BgBrush { get; set; }
        public IBrush BgBrushAlt { get; set; }
        public IBrush HighlightBrush { get; set; }
        public long BountiesCompletion { get; set; }
        public int BountiesLeft { get; set; }
        public int I { get; set; }
        public int II { get; set; }
        public int III { get; set; }
        public int IV { get; set; }
        public int V { get; set; }
        public bool A1Cache { get; set; }
        public bool A2Cache { get; set; }
        public bool A3Cache { get; set; }
        public bool A4Cache { get; set; }
        public bool A5Cache { get; set; }
        public int IM1 { get; set; }
        public int prevIM1 { get; set; }
        public int IM2 { get; set; }
        public int prevIM2 { get; set; }
        public int IM3 { get; set; }
        public int prevIM3 { get; set; }
        public int IM4 { get; set; }
        public int prevIM4 { get; set; }
        public bool  BoolIM1 { get; set; }
        public bool BoolIM2 { get; set; }
        public bool BoolIM3 { get; set; }
        public bool BoolIM4 { get; set; }
        public string keyIM1 { get; set; }
        public string keyIM2 { get; set; }
        public string keyIM3 { get; set; }
        public string keyIM4 { get; set; }

        public List<BountySnapshot> RemoveThese { get; private set; }

        public class BountySnapshot {
            public string Line { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public int Priority { get; set; } = 5;
        public int Hook { get; set; } = 0;
        
        public LiveStats_BountiesHelper()
        {
            Enabled = true;
        }
        
        public void Customize()
        {
            // Add this display to the LiveStats readout with a(n optional) specified positional order priority
            Hud.RunOnPlugin<LiveStatsPlugin>(plugin => {
                plugin.Add(this.Label, this.Priority, this.Hook);
            });
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            I = 5;
            II = 5;
            III = 5;
            IV = 5;
            V = 5;
            RemoveThese = new List<BountySnapshot>();
            BountyHistory = new List<BountySnapshot>();
            
            WhiteFont = Hud.Render.CreateFont("tahoma", 7, 255, 245, 245, 245, false, false, true);
            TimeFont = Hud.Render.CreateFont("tahoma", 7, 255, 170, 150, 120, false, false, true);
            YellowFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 255, 55, false, false, true);
            PurpleFont = Hud.Render.CreateFont("tahoma", 7, 255, 198, 86, 255, false, false, true);
            RedFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 0, 0, false, false, true);
            BlueFont = Hud.Render.CreateFont("tahoma", 7, 255, 0, 191, 255, false, false, true);
            GreenFont = Hud.Render.CreateFont("tahoma", 7, 255, 91, 237, 59, false, false, true);
            DefaultFont = Hud.Render.CreateFont("tahoma", 7, 255, 0,191,255, false, false, true);
            
            var plugin = Hud.GetPlugin<LiveStatsPlugin>();
            BgBrush = plugin.BgBrush;
            BgBrushAlt = plugin.BgBrushAlt;
            HighlightBrush = Hud.Render.CreateBrush(200, 72, 132, 84, 0);
            
            Label = new TopLabelDecorator(Hud)
            {
                TextFont = GreenFont,
                TextFunc = () => BountiesLeft + " left",
                HintFunc = () => "Bounties left",
                ExpandUpLabels = new List<TopLabelDecorator>() {
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = GreenFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  GreenFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => BountiesLeft + " \u2800\u2800"+ " \u2800\u2800",
                        HintFunc = () => "Bounties left",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  GreenFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  GreenFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => V < 1 ? A5Cache ? "\u2714 \u25AE" + " \u2800\u2800" :"\u2714 \u2800\u2800" + " \u2800\u2800" : V + " \u2800\u2800" + " \u2800\u2800",
                        HintFunc = () => "Act V",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  GreenFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  GreenFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => IV < 1 ? A4Cache ? "\u2714 \u25AE" + keyIM4 :"\u2714 \u2800\u2800"+ keyIM4 : IV + " \u2800\u2800"+ keyIM4,
                        HintFunc = () => "Act IV",
                    },
                   new TopLabelDecorator(Hud) {
                        TextFont =  GreenFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  GreenFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => III < 1 ? A3Cache ? "\u2714 \u25AE" + keyIM3 :"\u2714 \u2800\u2800"+ keyIM3 : III + " \u2800\u2800"+ keyIM3,
                        HintFunc = () => "Act III",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  GreenFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  GreenFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => II < 1 ? A2Cache ? "\u2714 \u25AE" + keyIM2 :"\u2714 \u2800\u2800"+ keyIM2 : II + " \u2800\u2800"+ keyIM2,
                        HintFunc = () => "Act II",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  GreenFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  GreenFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => I < 1 ? A1Cache ? "\u2714 \u25AE" + keyIM1 :"\u2714 \u2800\u2800" + keyIM1 : I + " \u2800\u2800" + keyIM1,
                        HintFunc = () => "Act I",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont = DefaultFont,
                        TextFunc = () => {
			float w = plugin.WidthFunc();
			float h = plugin.HeightFunc();
			float x = Hook == 0 ? (plugin.PinnedLabel == Label ? plugin.ExpandLabelHook : plugin.SelectedRectangle.X) : (plugin.PinnedTopLabel == Label ? plugin.ExpandLabelTopHook : plugin.SelectedTopRectangle.X);
			float y = Hook == 0 ? (plugin.SelectedRectangle.Y - h * (ExpandUpLabels.Count - 0.5f)) : (plugin.SelectedTopRectangle.Y + h * (ExpandUpLabels.Count - 0.5f));
                            
			DrawBountyHistory(new RectangleF(x, y, w, h));
                            
			return " ";
                        },
                    },
                },
            };
            
            ExpandUpLabels = Label.ExpandUpLabels;
            
        }

        public void PaintTopInGame(ClipState clipState)
        {
            int TotalBounties = 0;
            int Completed = 0;
            var Bounties = Hud.Game.Bounties;
            if (Bounties != null)
            {
                I = 5;
                II = 5;
                III = 5;
                IV = 5;
                V = 5;

                foreach (var Bounty in Bounties)
                {
                    TotalBounties++;
                    if (Bounty.State == QuestState.completed)
                    {
                     Completed++;
                     switch (Bounty.SnoQuest.BountyAct)
                      {
                        case BountyAct.A1:
                            I--;
                            break;
                        case BountyAct.A2:
                            II--;
                            break;
                        case BountyAct.A3:
                            III--;
                            break;
                        case BountyAct.A4:
                            IV--;
                            break;
                        case BountyAct.A5:
                            V--;
                            break;
                      }

                      string BountyName = Bounty.SnoQuest.NameLocalized.Replace("Bounty:", "");

                        BountySnapshot bounty = null;
                        var DatLine = BountyHistory.FirstOrDefault(x => x.Line.Contains(BountyName) && x.Line.Contains("started"));
                        var DatNewLine = BountyHistory.FirstOrDefault(x => x.Line.Contains(BountyName) && x.Line.Contains("completed"));
                        if (DatLine != null && DatNewLine == null)
                        {
                            string TheLine = DatLine.Line.Replace("started", "completed");
                            bounty = new BountySnapshot() { Line = TheLine, Timestamp = Hud.Time.Now };
                            BountyHistory.Add(bounty);

                           RemoveThese.Add(DatLine);
                        }



                    }
                }
            }
            BountiesCompletion = Completed;
            BountiesLeft = (int)(TotalBounties - BountiesCompletion);



         var InventoryHoradricCaches = Hud.Game.Items.Where(x => x.Location == ItemLocation.Inventory && x.SnoItem.MainGroupCode == "horadriccache");
         foreach (var Cache in InventoryHoradricCaches)
          {
                if (Cache.SnoItem.Code.Contains("A1")) A1Cache = true;
                if (Cache.SnoItem.Code.Contains("A2")) A2Cache = true;
                if (Cache.SnoItem.Code.Contains("A3")) A3Cache = true;
                if (Cache.SnoItem.Code.Contains("A4")) A4Cache = true;
                if (Cache.SnoItem.Code.Contains("A5")) A5Cache = true;
                if (Cache.SnoItem.Code.Contains("Act1")) A1Cache = true;
                if (Cache.SnoItem.Code.Contains("Act2")) A2Cache = true;
                if (Cache.SnoItem.Code.Contains("Act3")) A3Cache = true;
                if (Cache.SnoItem.Code.Contains("Act4")) A4Cache = true;
                if (Cache.SnoItem.Code.Contains("Act5")) A5Cache = true;
          }


            foreach (var item in Hud.Inventory.ItemsInInventory)
            {
                if (item.SnoItem.Sno == 1054965529) IM1 = (int)item.Quantity; // Infernal Machine of Regret - Act I - Oleg
                if (item.SnoItem.Sno == 2788723894) IM2 = (int)item.Quantity; //  Infernal Machine of Putridness - Act II - Sokahr
                if (item.SnoItem.Sno == 2622355732) IM3 = (int)item.Quantity; // Infernal Machine of Terror - Act III - Xah'Rith
                if (item.SnoItem.Sno == 1458185494) IM4 = (int)item.Quantity; // Infernal Machine of Fright - Act IV - Nekarat
            }

            if (IM1 > prevIM1) BoolIM1 = true;
            if (IM2 > prevIM2) BoolIM2 = true;
            if (IM3 > prevIM3) BoolIM3 = true;
            if (IM4 > prevIM4) BoolIM4 = true;

            prevIM1 = IM1;
            prevIM2 = IM2;
            prevIM3 = IM3;
            prevIM4 = IM4;

            if (BoolIM1) keyIM1 = " \u26BF"; else keyIM1 = " \u2800\u2800";
            if (BoolIM2) keyIM2 = " \u26BF"; else keyIM2 = " \u2800\u2800";
            if (BoolIM3) keyIM3 = " \u26BF"; else keyIM3 = " \u2800\u2800";
            if (BoolIM4) keyIM4 = " \u26BF"; else keyIM4 = " \u2800\u2800";

            foreach (var RemoveThis in RemoveThese)
             {
                BountyHistory.Remove(RemoveThis);
             }
            RemoveThese.Clear();
        }
        
        public void OnNewArea(bool newGame, ISnoArea area)
        {
            if (newGame)
             {
               BountyHistory.Clear();
               RemoveThese.Clear();
               A1Cache = false;
               A2Cache = false;
               A3Cache = false;
               A4Cache = false;
               A5Cache = false;
               prevIM1 = IM1;
               prevIM2 = IM2;
               prevIM3 = IM3;
               prevIM4 = IM4;
               BoolIM1 = false;
               BoolIM2 = false;
               BoolIM3 = false;
               BoolIM4 = false;
            }
        }

        public void OnChatLineChanged(string currentLine, string previousLine)
        {
            if (!string.IsNullOrEmpty(currentLine))
             {

                if (currentLine.Contains("completed") || currentLine.Contains("Keywarden") || (currentLine.Contains("started") && !currentLine.Contains("started a bounty")))
                {
                    string TruncatedLine = String.Empty;

                    if (currentLine.Contains("Horadric Reliquary") && !currentLine.Contains("Act")) currentLine = currentLine.Replace("the event:", "Act I:");
                    else if (currentLine.Contains("Gift of the Emperor") && !currentLine.Contains("Act")) currentLine = currentLine.Replace("the event:", "Act II:");
                    else if (currentLine.Contains("Keep Armament") && !currentLine.Contains("Act")) currentLine = currentLine.Replace("the event:", "Act III:");
                    else if (currentLine.Contains("Blessings of the High Heavens") && !currentLine.Contains("Act")) currentLine = currentLine.Replace("the event:", "Act IV:");
                    else if (currentLine.Contains("Westmarch Stores") && !currentLine.Contains("Act")) currentLine = currentLine.Replace("the event:", "Act V:");
                    else if (currentLine.Contains("completed") && currentLine.Contains("event: ")) TruncatedLine = currentLine.Substring(currentLine.IndexOf("event: ") + 7);
                    else if (currentLine.Contains("completed") && currentLine.Contains("Bounty: ")) TruncatedLine = currentLine.Substring(currentLine.IndexOf("Bounty: ") + 8);
                    else if (currentLine.Contains("started the event:")) TruncatedLine = currentLine.Substring(currentLine.IndexOf("started the event: ") + 19);
                    else if (currentLine.Contains("Keywarden")) TruncatedLine = currentLine.Substring(currentLine.IndexOf("Keywarden"));


                    if (TruncatedLine != String.Empty)
                     {
                      TruncatedLine = TruncatedLine.Trim(' ','!');

                        foreach (var OutdatedLine in BountyHistory.Where(x => x.Line.Contains(TruncatedLine)))
                         {
                            RemoveThese.Add(OutdatedLine);
                         }

                     }

                    string TheLine = currentLine;
                    BountySnapshot bounty = null;
                    if (!BountyHistory.Any(d => d.Line.Equals(TheLine)))
                    {
                        bounty = new BountySnapshot() { Line = TheLine, Timestamp = Hud.Time.Now };
                        BountyHistory.Add(bounty);
                    }

                }
                else if (currentLine.Contains("Bounty: "))
                {
                    string PartTwo = currentLine.Substring(currentLine.IndexOf("Bounty: "));
                    if (PartTwo.Contains(",")) PartTwo = PartTwo.Substring(0, PartTwo.LastIndexOf(","));

                    string PartOne = String.Empty;
                    if (string.IsNullOrEmpty(previousLine))
                    {
                        if (Hud.Game.NumberOfPlayersInGame == 1)
                            PartOne = "You have started ";
                        else
                            PartOne = "Somebody has started ";
                    }
                    else if (previousLine.Contains("started "))
                    {
                        PartOne = previousLine.Substring(0, previousLine.LastIndexOf("started "));
                        PartOne = PartOne + "started ";
                    }
                    else if (previousLine.Contains("teleported "))
                    {
                        PartOne = previousLine.Substring(0, previousLine.LastIndexOf("teleported "));
                        PartOne = PartOne + "has started ";
                    }

                    if (PartOne == String.Empty) PartOne = "? has started ";

                    string TheLine = PartOne + PartTwo;
                    BountySnapshot bounty = null;
                    if (!BountyHistory.Any(d => d.Line.Equals(TheLine)))
                    {
                        bounty = new BountySnapshot() { Line = TheLine, Timestamp = Hud.Time.Now };
                        BountyHistory.Add(bounty);
                    }
                }
            }
            
        }


        private void DrawBountyHistory(RectangleF rect) // x,y coordinates of the bottom left of the bounty history window (above the other labels + space gap)
        {
            if (BountyHistory == null || BountyHistory.Count == 0) return;
            
            DateTime now = Hud.Time.Now;
            IEnumerable<BountySnapshot> history = (BountyHistoryTimeout > 0 ?
                BountyHistory.Where(d => (now - d.Timestamp).TotalSeconds < BountyHistoryTimeout) :
                BountyHistory
            ).OrderByDescending(d => d.Timestamp).Take(BountyHistoryShown);
            List<Tuple<TextLayout, TextLayout, IFont, IBrush>> lines = new List<Tuple<TextLayout, TextLayout, IFont, IBrush>>(BountyHistoryShown + 1);
            
            foreach (BountySnapshot bounty in history)
            {
                // How long ago was the bounty
                TimeSpan elapsed = now - bounty.Timestamp;
                string time;
                if (elapsed.TotalSeconds < 60)
                    time = elapsed.TotalSeconds.ToString("F0") + "s ago";
                else if (elapsed.TotalMinutes < 5)
                    time = elapsed.TotalMinutes.ToString("F0") + "m" + elapsed.Seconds.ToString("F0") + "s ago";
                else if (elapsed.TotalMinutes < 10)
                    time = elapsed.TotalMinutes.ToString("F0") + "m ago";
                else
                    time = bounty.Timestamp.ToString("hh:mm tt");

                // Line
                IFont color = GreenFont;
                if (bounty.Line.Contains("completed")) color = YellowFont;
                if (bounty.Line.Contains("Keywarden")) color = PurpleFont;
                if (bounty.Line.Contains("the Butcher") || bounty.Line.Contains("Queen Araneae") || bounty.Line.Contains("Vidian") || bounty.Line.Contains("the Skeleton King")
                    || bounty.Line.Contains("Maghda") || bounty.Line.Contains("Belial") || bounty.Line.Contains("Zoltun Kulle") || bounty.Line.Contains("the Siegebreaker Assault Beast")
                    || bounty.Line.Contains("Azmodan") || bounty.Line.Contains("Cydaea") || bounty.Line.Contains("Ghom") || bounty.Line.Contains("Diablo")
                    || bounty.Line.Contains("Izual") || bounty.Line.Contains("Malthael") || bounty.Line.Contains("Adria") || bounty.Line.Contains("Urzael") || bounty.Line.Contains("Rakanoth")) color = RedFont;
                if (bounty.Line.Contains("Horadric Reliquary") || bounty.Line.Contains("Gift of the Emperor") || bounty.Line.Contains("Keep Armament")
                    || bounty.Line.Contains("Blessings of the High Heavens") || bounty.Line.Contains("Westmarch Stores")) color = BlueFont;
                // Store line data
                lines.Add(new Tuple<TextLayout, TextLayout, IFont, IBrush>(
                    TimeFont.GetTextLayout(time),
                    color.GetTextLayout(bounty.Line),
                    color,
                    (elapsed.TotalSeconds <= BountyHistoryHighlight ? HighlightBrush : BgBrush)
                ));
            }
            
            // Compute width of the bounty history display
            float widthTime = lines.Select(t => t.Item1.Metrics.Width).Max();
            float widthName = lines.Select(t => t.Item2.Metrics.Width).Max();
            float width = Math.Max(rect.Width, 5 + widthTime + 10 + widthName + 5);
            
            //x += width;
            float x = rect.X;
            if (x + width > Hud.Window.Size.Width)
                x = Hud.Window.Size.Width - width;
            float y = rect.Y;
            
            foreach (Tuple<TextLayout, TextLayout, IFont, IBrush> line in lines)
            {
                TextLayout layout = line.Item1;
                float height = Math.Max(layout.Metrics.Height, rect.Height);
		if (Hook == 0) y -= height;
			else y += height;
                
                // Draw background
                line.Item4.DrawRectangle(x, y, width, height); 
                
                // Draw timestamp
                TimeFont.DrawText(layout, x + 5 + widthTime - layout.Metrics.Width, y + height*0.5f - layout.Metrics.Height*0.5f); 
                
                // Draw sentence
                layout = line.Item2;
                line.Item3.DrawText(layout, x + 5 + widthTime + 5 , y + height*0.5f - layout.Metrics.Height*0.5f);
            }
        }
    }
}