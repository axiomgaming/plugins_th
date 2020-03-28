// RunStats Damage Meter by Razor

using Turbo.Plugins.Default;
using System.Drawing;
using SharpDX.Direct2D1;
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
    public class LiveStats_DamageMeter : BasePlugin, ICustomizer, IKeyEventHandler, INewAreaHandler, IInGameTopPainter //, IAfterCollectHandler
    {
        public string TextSave { get; set; } = "SAVE";
        public string TextLive { get; set; } = "LIVE";
        public string TextNow { get; set; } = "now";
        
	public bool HideGraphInCombat { get; set; } = false; // Hide the full Graph when you're in combat
        public bool HideTooltipInCombat { get; set; } = false; // Hide only the Graph Tooltips when you're in combat

        public IFont TextFont { get; set; } // Label font
        public IBrush BgBrush { get; set; } // Label bg
        public IBrush BgBrushAlt { get; set; } // Label bg
        public TopLabelDecorator Label { get; set; }
        public List<TopLabelDecorator> ExpandUpLabels { get; set; }

        public IBrush GraphBgBrush { get; set; }
        public IBrush MarkerBrush { get; set; }
        public IFont MarkerFont { get; set; }
        public IFont DpsFont { get; set; }
        public IFont AvgFont { get; set; }
        public IFont DmgFont { get; set; }
        public IFont CritFont { get; set; }
        public IFont MHPFont { get; set; }
        
        public int GraphDuration { get; set; } = 30; // In seconds		
        public float GraphWidth { get; set; } // Screen size in pixels
        public float GraphHeight { get; set; } // Screen size in pixels
        public bool InvertGraph { get; set; } = true;
        public int YAxisMarkersCount { get; set; } = 5;
        public int XAxisMarkersCount { get; set; } = 6;
        
        public List<GraphLine> LiveData { get; private set; }
        public List<GraphLine> SaveData { get; private set; }
        public DateTime LiveTimestamp { get; private set; }
        public DateTime SaveTimestamp { get; private set; }
        public bool ShowSave { get; private set; } = false;
        public GraphLine DmgLine { get; private set; }
        public GraphLine DpsLine { get; private set; }
        public GraphLine AvgLine { get; private set; }
        public double HighestHit { get; private set; }
        public double LowestHit { get; private set; }
        public int TickInterval { get; private set; }
        
        private int LastRecordedTick;
        private double LastSeenDamageDealtHit;
        private double LastSeenDamageDealtAll;
        private double LastSeenDamageDealtCrit;
        private IWatch ConduitPlayTime;
        
        public class GraphLine
        {
            public List<decimal> Data { get; set; } = null;
            public string Name { get; set; }
            public Func<decimal> DataFunc { get; set; }
            public IBrush Brush { get; set; }
            public IFont Font { get; set; }
            public TopLabelDecorator Tooltip { get; set; } = null;

            public GraphLine(string name)
            {
                Name = name;
            }
            
            public GraphLine(GraphLine toCopy)
            {
                Data = new List<decimal>(toCopy.Data);
                DataFunc = toCopy.DataFunc;
                Name = toCopy.Name;
                Brush = toCopy.Brush;
                Font = toCopy.Font;
            }
        }	

        public int Priority { get; set; } = 20;
        public int Hook { get; set; } = 0;

        public LiveStats_DamageMeter()
        {
            Enabled = true;
        }
        
        public void Customize()
        {
            // Add this display to the LiveStats readout with a specified positional order priority of 20
            Hud.RunOnPlugin<LiveStatsPlugin>(plugin => 
            {
                plugin.Add(this.Label, this.Priority, this.Hook);
            });
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            
            ConduitPlayTime = Hud.Time.CreateWatch();
            
            TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 135, 135, 135, false, false, true);
            DmgFont = Hud.Render.CreateFont("tahoma", 7, 255, 211, 228, 255, false, false, true);
            DpsFont = Hud.Render.CreateFont("tahoma", 7, 255, 91, 237, 59, false, false, true);
            AvgFont = Hud.Render.CreateFont("tahoma", 7, 255, 107, 96, 255, false, false, true); //73, 255, 239 //84, 106, 255
            CritFont = Hud.Render.CreateFont("tahoma", 7, 255, 211, 237, 59, false, false, true);
            MHPFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 73, 73, false, false, true);
            
            GraphBgBrush = Hud.Render.CreateBrush(125, 0, 0, 0, 0);
            MarkerFont = Hud.Render.CreateFont("tahoma", 6, 200, 211, 228, 255, false, false, true);
            MarkerBrush = Hud.Render.CreateBrush(45, 190, 190, 190, 1);
            
            var plugin = Hud.GetPlugin<LiveStatsPlugin>();
            BgBrush = plugin.BgBrush;
            BgBrushAlt = plugin.BgBrushAlt;
            
            Label = new TopLabelDecorator(Hud)
            {
                TextFont = AvgFont,
                TextFunc = () => 
                {
                    // Initialize graph size
                    if (GraphWidth < 1) 
                    {
                        GraphWidth = Hud.Window.Size.Width - Hud.Render.MinimapUiElement.Rectangle.X - 5; //scale to the width of the minimap
                        GraphHeight = GraphWidth * 0.7f;
                    }
                    
                    decimal time = (decimal)(Hud.Tracker.Session.PlayElapsedMilliseconds - ConduitPlayTime.ElapsedMilliseconds);
                    return (time <= 0 ? "0" : ValueToString((long)((decimal)Hud.Tracker.Session.DamageDealtAll / time)*1000, ValueFormat.ShortNumber)) + " dps"; //ValueToString(Hud.Game.Me.Damage.CurrentDps, ValueFormat.ShortNumber) + " dps"; // 📉
                },
                HintFunc = () => "Average DPS",
                ExpandUpLabels = new List<TopLabelDecorator>() 
                {
                    new TopLabelDecorator(Hud) 
                    {
                        TextFont = MHPFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont = MHPFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Stat.MonsterHitpointDecreasePerfCounter.LastValue, ValueFormat.ShortNumber) + " mhl", // MHL (Monster HP Loss) instead of HPS which was misleading (Healing / Second ?)
                        HintFunc = () => "Monster HP Loss",
                    },
                    new TopLabelDecorator(Hud) 
                    {
                        TextFont = DpsFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = DpsFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Game.Me.Damage.CurrentDps, ValueFormat.ShortNumber) + " dps",
                        HintFunc = () => "Current DPS",
                    },
                    new TopLabelDecorator(Hud) 
                    {
                        TextFont = CritFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont = CritFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.Session.DamageDealtCrit, ValueFormat.ShortNumber),
                        HintFunc = () => "Crit Damage",
                    },
                    new TopLabelDecorator(Hud) 
                    {
                        TextFont = DmgFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = DmgFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.Session.DamageDealtAll - Hud.Tracker.Session.DamageDealtCrit, ValueFormat.ShortNumber),
                        HintFunc = () => "Non-Crit Damage",
                    },
                    new TopLabelDecorator(Hud) 
                    {
                        TextFont = TextFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(this.HighestHit, ValueFormat.ShortNumber),
                        HintFunc = () => "Highest Hit",
                    },
                    new TopLabelDecorator(Hud) 
                    {
                        Enabled = false,
                        TextFont = TextFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = TextFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.Session.DamageDealtAll, ValueFormat.ShortNumber),
                        HintFunc = () => "Total Damage",
                    },
                    new TopLabelDecorator(Hud) 
                    {
                        Enabled = false,
                        TextFont = TextFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(this.LowestHit, ValueFormat.ShortNumber),
                        HintFunc = () => "Lowest Hit",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
                        TextFunc = () => {
			float x = Hook == 0 ? (plugin.PinnedLabel == Label ? plugin.ExpandLabelHook : plugin.SelectedRectangle.X) : (plugin.PinnedTopLabel == Label ? plugin.ExpandLabelTopHook : plugin.SelectedTopRectangle.X);
                        float y = Hook == 0 ? (plugin.SelectedRectangle.Y - ExpandUpLabels.Count(l => l.Enabled) * plugin.HeightFunc()) : (plugin.SelectedTopRectangle.Y + ExpandUpLabels.Count(l => l.Enabled) * plugin.HeightFunc());
			float width = GraphWidth + 5;
                            if (x + width > Hud.Window.Size.Width) // Boundary checking
                                x = Hud.Window.Size.Width - width;
			    if (y - GraphHeight < 0) // Boundary checking
				y = y + GraphHeight + 40;
                                
			DrawGraph((ShowSave && SaveData != null ? SaveData : LiveData), x, y - GraphHeight - 10);
                            
			return " ";
                        }
                    }
                }
            };
            
            ExpandUpLabels = Label.ExpandUpLabels;
            
            // Partially initialize, but don't preallocate Data memory until the graph size is determined
            LiveData = new List<GraphLine>() { 
                new GraphLine("Monster HP Loss")
                { 
                    DataFunc = () => {
                        var dps = Hud.Stat.MonsterHitpointDecreasePerfCounter.LastValue; // Monster HP Loss Rate
                        
                        return (dps < 0 ? 1 : dps > (double.MaxValue/2) ? 1 :(decimal)dps // Possibly a number overflow error
                        );
                    },
                    Brush = Hud.Render.CreateBrush(255, 255, 73, 73, 1), 
                    Font = MHPFont,
                }, 
                new GraphLine("Current DPS")
                { 
                    //Name = "Current DPS", 
                    DataFunc = () => {
                        double dps = Hud.Game.Me.Damage.CurrentDps;
                        
                        return (ConduitPlayTime.IsRunning && dps == 0 ? // HUD doesn't update damage data while conduit is active
                            -1 : 
                            (decimal)dps
                        );
                    },
                    Brush = Hud.Render.CreateBrush(255, 91, 237, 59, 1), 
                    Font = DpsFont
                }, 
                new GraphLine("Average DPS")
                { 
                    //Name = "Average DPS", 
                    DataFunc = () => ((decimal)Hud.Tracker.Session.DamageDealtAll / (decimal)(Hud.Tracker.Session.PlayElapsedMilliseconds - ConduitPlayTime.ElapsedMilliseconds))*1000,
                    Brush = Hud.Render.CreateBrush(255, 107, 96, 255, 1), 
                    Font = AvgFont 
                },
                new GraphLine("Non-Crit Damage") 
                { 
                    //Name = "Non-Crit Damage", 
                    DataFunc = () => {
                        double all = Hud.Tracker.Session.DamageDealtAll - LastSeenDamageDealtAll;
                        double crit = Hud.Tracker.Session.DamageDealtCrit - LastSeenDamageDealtCrit; // LastSeenDamageDealtCrit is updated in another GraphLine, don't have to modify it here

                        LastSeenDamageDealtAll = Hud.Tracker.Session.DamageDealtAll;
                        
                        return (all < crit ? 
                            0 :
                            (ConduitPlayTime.IsRunning && all == 0 ? // HUD doesn't update damage data while conduit is active
                                -1 :
                                (decimal)(all - crit)
                            )
                        );
                    },
                    Brush = Hud.Render.CreateBrush(255, 211, 228, 255, 1.5f), 
                    Font = DmgFont
                }, 
                new GraphLine("Crit Damage") 
                { 
                    DataFunc = () => {
                        double delta = Hud.Tracker.Session.DamageDealtCrit - LastSeenDamageDealtCrit;
                        LastSeenDamageDealtCrit = Hud.Tracker.Session.DamageDealtCrit;
                        
                        return (ConduitPlayTime.IsRunning && delta == 0 ? // Damage data doesn't update while conduit is active
                            -1 :
                            (decimal)delta
                        );
                    },
                    Brush = Hud.Render.CreateBrush(255, 211, 237, 59, 1.5f), 
                    Font = CritFont 
                }
            };
            
        }
        
        public void OnNewArea(bool newGame, ISnoArea area)
        {
            if (newGame)
                LastRecordedTick = 0;
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (!Hud.Game.IsInGame || Hud.Game.IsInTown)
            {
                ConduitPlayTime.Stop();
                return;
            }
            
            if (Hud.Game.Me.Powers.BuffIsActive(403404) || Hud.Game.Me.Powers.BuffIsActive(263029))
            {
                if (!ConduitPlayTime.IsRunning)
                    ConduitPlayTime.Start();
            }
            
            // Graph dimensions not yet initialized
            if (GraphWidth < 1) return; 
            
            // Initialize damage history
            if (TickInterval == 0) 
            {
                TickInterval = (int)((float)(GraphDuration * 60)/GraphWidth);
                LastRecordedTick = Hud.Game.CurrentGameTick;
                LastSeenDamageDealtAll = Hud.Game.CurrentHeroTotal.DamageDealtAll;
                LastSeenDamageDealtHit = LastSeenDamageDealtAll;

                int capacity = (int)GraphWidth + 1;
                foreach (GraphLine line in LiveData)
                {
                    if (line.Data == null)
                        line.Data = new List<decimal>(capacity);
                    
                }

                return;
            }
            
            // Check for changes in damage done total to record biggest and smallest hits
            double damage = Hud.Game.CurrentHeroTotal.DamageDealtAll;
            double hit = damage - LastSeenDamageDealtHit;
            if (hit != 0) 
            {
                if (hit > HighestHit) 
                {
                    if (HighestHit == LowestHit) LowestHit = hit;
                    HighestHit = hit;
                } else if (hit < LowestHit) 
                    LowestHit = hit;

                LastSeenDamageDealtHit = damage; // Remember the last damage total value for hit calculation
            }
            
            // Record damage done during the intervals of time that are to be drawn in a graph
            if (Hud.Game.CurrentGameTick >= LastRecordedTick + TickInterval) 
            {
                foreach (GraphLine line in LiveData)
                {
                    line.Data.Add(line.DataFunc());
                    
                    if (line.Data.Count > GraphWidth)
                        line.Data.RemoveAt(0);
                }
                
                LiveTimestamp = Hud.Time.Now;
                LastRecordedTick = Hud.Game.CurrentGameTick;
            }
        }
        
        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (!Hud.Window.IsForeground) return; // Only process the key event if HUD is actively displayed

            if (keyEvent.IsPressed)
            {
                if (keyEvent.Key == Key.Subtract)
                    InvertGraph = false;
                else if (keyEvent.Key == Key.Add)
                    InvertGraph = true;
                else if (keyEvent.Key == Key.Down) // && LastRecordedTick != 0) // To avoid a crash when the graph is empty (Actual fix done)
                    SaveGraph();
                else if (keyEvent.Key == Key.Up)
                    ToggleBetweenGraphs();
            }
        }
        
        public void DrawGraph(List<GraphLine> Recordings, float x, float y)
        {
	    if (HideGraphInCombat && Hud.Game.Me.InCombat) return;

            GraphBgBrush.DrawRectangle(x-5, y-5, GraphWidth+10, GraphHeight+10);
            
            if (Recordings == null || Recordings[0].Data == null || Recordings[0].Data.Count < 2) return; // Not enough data to graph
            
            // Draw title
            TextLayout layout = MarkerFont.GetTextLayout(Recordings == LiveData ?
                string.Format("{0} ({1})", TextLive, LiveTimestamp.ToString("hh:mm:ss tt")) :
                string.Format("{0} ({1})", TextSave, SaveTimestamp.ToString("hh:mm:ss tt"))
            );
            MarkerFont.DrawText(layout, x + GraphWidth*0.5f - layout.Metrics.Width*0.5f, y - 10 - layout.Metrics.Height);

            decimal max = Recordings.Select(r => r.Data.Max()).Max();
            decimal damagePerPixel = max / (decimal)GraphHeight;
            
            if (damagePerPixel <= 0) return; // No damage per pixel, no max damage > 0 recorded
            
            // Draw axis markers
            if (YAxisMarkersCount > 0)
            {
                float markerX = x - 5;
                decimal interval = max / ((decimal)YAxisMarkersCount - 1);
                for (int i = 0; i <= YAxisMarkersCount; ++i)
                {
                    decimal dmg = (i == YAxisMarkersCount ? 0 : max - (interval * i));
                    float markerY = y + GraphHeight - (float)(dmg / damagePerPixel);
                    layout = MarkerFont.GetTextLayout(ValueToString((long)dmg, ValueFormat.ShortNumber));
                    MarkerFont.DrawText(layout, markerX - layout.Metrics.Width - 10, markerY - layout.Metrics.Height*0.5f);
                    MarkerBrush.DrawLine(markerX - 5, markerY, markerX + GraphWidth + 5, markerY);
                }
            }
            
            // Draw axis markers
            if (XAxisMarkersCount > 0)
            {
                float markerY = y + GraphHeight + 5;
                layout = MarkerFont.GetTextLayout(TextNow);
                MarkerFont.DrawText(layout, x - layout.Metrics.Width*0.5f, markerY + 5);
                MarkerBrush.DrawLine(x, markerY + 5, x, markerY - GraphHeight - 5);
                
                float interval = (float)GraphDuration / ((float)XAxisMarkersCount - 1f);
                float width = GraphWidth / ((float)XAxisMarkersCount - 1f);
                for (int i = 1; i < XAxisMarkersCount; ++i) // Was <=
                {
                    float time = interval * i;
                    float markerX = x + (float)(width * i);
                    layout = MarkerFont.GetTextLayout(time.ToString("F0") + (i == 1 ? "s ago" : "s"));
                    MarkerFont.DrawText(layout, markerX - layout.Metrics.Width*0.5f, markerY + 5);
                    MarkerBrush.DrawLine(markerX, markerY + 5, markerX, markerY - GraphHeight - 5);
                }				
            }
            
            foreach (GraphLine line in Recordings)
            {
                using (var pg = Hud.Render.CreateGeometry()) // Pathgeometry
                {
                    using (var gs = pg.Open()) // Geometrysink
                    {
                        
                        gs.BeginFigure(new SharpDX.Vector2(x, y + GraphHeight - (float)(line.Data[line.Data.Count - 1]/damagePerPixel)), FigureBegin.Filled);
                        
                        for (int i = 1; i < line.Data.Count; ++i)
                        {
                            decimal data = line.Data[line.Data.Count - 1 - i];
                            if (data < 0)
                                gs.AddLine(new SharpDX.Vector2(x + i, y + GraphHeight));
                            else
                                gs.AddLine(new SharpDX.Vector2(x + i, y + GraphHeight - (float)(data/damagePerPixel)));
                        }
                            
                        gs.EndFigure(FigureEnd.Open); //FigureEnd.Closed //FigureEnd.Open
                        gs.Close();
                    }
                    
                    line.Brush.DrawGeometry(pg);
                }
            }
            
            // Draw tooltip
            if (HideTooltipInCombat && Hud.Game.Me.InCombat) return;

                float cursorX = (float)Hud.Window.CursorX;
                float cursorY = (float)Hud.Window.CursorY;
                bool yValidHover = (cursorY <= y + GraphHeight + 5 && cursorY >= y - 5);
                bool xValidHover = (cursorX >= x - 5 && cursorX <= x + Recordings[0].Data.Count + 5);
                
                if (yValidHover) 
                {
                    if (xValidHover) 
                    {
                        List<Tuple<TextLayout, TextLayout, IFont>> tooltips = new List<Tuple<TextLayout, TextLayout, IFont>>(Recordings.Count);
                        float labelWidth = 0;
                        float valueWidth = 0;
                        float height = 0;
                        
                        foreach (GraphLine line in Recordings)
                        {
                            decimal data;
                            if (cursorX >= x - 5 && cursorX <= x) // Accounting for a little edge error
                                data = line.Data[line.Data.Count - 1];
                            else if (cursorX >= x + line.Data.Count && cursorX <= x + line.Data.Count + 5) // Accounting for a little edge error
                                data = line.Data[0];
                            else
                                data = line.Data[line.Data.Count - 1 - (int)(cursorX - x)];
                            
                            var tooltip = new Tuple<TextLayout, TextLayout, IFont>(
                                line.Font.GetTextLayout(line.Name + " : "), 
                                line.Font.GetTextLayout(data == -1 ? "N/A" : ValueToString((long)data, ValueFormat.LongNumber)),
                                line.Font
                            );
                            
                            // Update max widths
                            labelWidth = (float)Math.Max(tooltip.Item1.Metrics.Width, labelWidth);						
                            valueWidth = (float)Math.Max(tooltip.Item2.Metrics.Width, valueWidth);
                            height = (float)Math.Max(tooltip.Item2.Metrics.Height, height);
                            
                            // Add line
                            tooltips.Add(tooltip);
                            
                            // Draw circle around the data point on the graph
                            if (data != -1)
                                line.Brush.DrawEllipse(cursorX, y + GraphHeight - (float)(data/damagePerPixel), 5f, 5f);
                        }

                        RectangleF rect = new RectangleF(cursorX - labelWidth - valueWidth - 10 - 15, 
                            cursorY - (height+10)*(tooltips.Count - 0.5f) - 10 - 10, 
                            labelWidth + valueWidth + 15,
                            (height+10)*(tooltips.Count - 0.5f) + 10
                        );
                        BgBrush.DrawRectangle(rect);
                        MarkerBrush.DrawRectangle(rect);
                        
                        float tooltipX = cursorX - valueWidth - 5 - 15;
                        float tooltipY = cursorY - height - 5 - 10;
                        
                        foreach (var tooltip in tooltips)
                        {
                            tooltip.Item3.DrawText(tooltip.Item1, tooltipX - tooltip.Item1.Metrics.Width, tooltipY);
                            tooltip.Item3.DrawText(tooltip.Item2, tooltipX + 5, tooltipY);
                            tooltipY -= height + 10;
                        }
                    }
                }
        }
        
        private void SaveGraph()
        {
	if (LiveData == null || LiveData[0].Data == null || LiveData[0].Data.Count < 2) return; // Safecheck : No save if there is not enough data to draw the graph

            if (SaveData != null)
                SaveData.Clear();
            else
                SaveData = new List<GraphLine>(LiveData.Count);
            
            foreach (GraphLine line in LiveData)
                SaveData.Add(new GraphLine(line));
            
            SaveTimestamp = LiveTimestamp;
        }
        
        private void ToggleBetweenGraphs()
        {
            ShowSave = !ShowSave;
        }
    }
}