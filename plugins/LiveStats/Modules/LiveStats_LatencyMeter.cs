// RunStats LatencyMeter by HaKache

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
    public class LiveStats_LatencyMeter : BasePlugin, ICustomizer, IKeyEventHandler, INewAreaHandler, IInGameTopPainter //, IAfterCollectHandler
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
        public IFont GreenFont { get; set; }
        public IFont YellowFont { get; set; }
        public IFont OrangeFont { get; set; }
        public IFont RedFont { get; set; }
        
        public int GraphDuration { get; set; } = 120; // In seconds		
        public float GraphWidth { get; set; } // Screen size in pixels
        public float GraphHeight { get; set; } // Screen size in pixels
        public bool InvertGraph { get; set; } = true;
        public int YAxisMarkersCount { get; set; } = 6;
        public int XAxisMarkersCount { get; set; } = 6;
        
        public List<GraphLine> LiveData { get; private set; }
        public List<GraphLine> SaveData { get; private set; }
        public DateTime LiveTimestamp { get; private set; }
        public DateTime SaveTimestamp { get; private set; }
        public bool ShowSave { get; private set; } = false;

        public double HighestPing { get; private set; }
        public double LowestPing { get; private set; }
        public int TickInterval { get; private set; }
        private int LastRecordedTick;
        private double LastSeenPing;

        public int MediumLimit { get; set; } = 80;
        public int HighLimit { get; set; } = 150;
        
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
	
        public int Priority { get; set; } = 25;
        public int Hook { get; set; } = 0;

        public LiveStats_LatencyMeter()
        {
            Enabled = true;
        }
        
        public void Customize()
        {
            // Add this display to the LiveStats readout with a(n optional) specified positional order priority of 9
            Hud.RunOnPlugin<LiveStatsPlugin>(plugin => 
            {
                plugin.Add(this.Label, this.Priority, this.Hook);
            });
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            
	    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 135, 135, 135, false, false, true);
	    GreenFont = Hud.Render.CreateFont("tahoma", 7, 255, 91, 237, 59, false, false, true);
	    YellowFont = Hud.Render.CreateFont("tahoma", 7, 255, 211, 237, 59, false, false, true);
	    OrangeFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 128, 43, false, false, true);
	    RedFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 73, 73, false, false, true);
	
	    GraphBgBrush = Hud.Render.CreateBrush(125, 0, 0, 0, 0);
	    MarkerFont = Hud.Render.CreateFont("tahoma", 6, 200, 211, 228, 255, false, false, true);
	    MarkerBrush = Hud.Render.CreateBrush(45, 190, 190, 190, 1);

	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
	    BgBrush = plugin.BgBrush;
	    BgBrushAlt = plugin.BgBrushAlt;
            
            Label = new TopLabelDecorator(Hud)
            {
                TextFont = YellowFont,
                TextFunc = () => 
                {
                    // Initialize graph size
                    if (GraphWidth < 1) 
                    {
                        GraphWidth = Hud.Window.Size.Width - Hud.Render.MinimapUiElement.Rectangle.X - 5; // Scale to the width of the minimap
                        GraphHeight = GraphWidth * 0.7f;
                    }

                    return Hud.Game.CurrentLatency.ToString() + " ms"; // 📉
                },
                HintFunc = () => "Current Latency",
                ExpandUpLabels = new List<TopLabelDecorator>() 
                {
                    new TopLabelDecorator(Hud) 
                    {
                        TextFont = YellowFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont = YellowFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => Hud.Game.CurrentLatency.ToString() + " ms",
                        HintFunc = () => "Current Latency",
                    },
                    new TopLabelDecorator(Hud) 
                    {
                        TextFont = OrangeFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = OrangeFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => Hud.Game.AverageLatency.ToString() + " ms",
                        HintFunc = () => "Average Latency",
                    },
                    new TopLabelDecorator(Hud) 
                    {
                        TextFont = GreenFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont = GreenFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => this.LowestPing.ToString() + " ms",
                        HintFunc = () => "Lowest Ping",
                    },
                    new TopLabelDecorator(Hud) 
                    {
                        TextFont = RedFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = RedFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => this.HighestPing.ToString() + " ms",
                        HintFunc = () => "Highest Ping",
                    },
                    new TopLabelDecorator(Hud) 
                    {
			//Enabled = false,
                        TextFont = TextFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => (GC.GetTotalMemory(false) / 1024.0 / 1024.0).ToString("F0") + " MB", // + " (" + Hud.Stat.RenderPerfCounter.LastCount.ToString("F0") + " FPS)", FPS Render Time
                        HintFunc = () => "HUD Memory Use",
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
                new GraphLine("Current Latency")
                { 
                    //Name = "Current Latency", 
                    DataFunc = () => (decimal)Hud.Game.CurrentLatency,
                    Brush = Hud.Render.CreateBrush(255, 91, 237, 59, 1), 
                    Font = GreenFont 
                },
                new GraphLine("Average Latency")
                { 
                    //Name = "Average Latency", 
                    DataFunc = () => (decimal)Hud.Game.AverageLatency,
                    Brush = Hud.Render.CreateBrush(255, 255, 128, 43, 1), 
                    Font = OrangeFont,
                },
            };
            
        }

        public void OnNewArea(bool newGame, ISnoArea area)
        {
            if (newGame)
                LastRecordedTick = 0;
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (!Hud.Game.IsInGame) return;

	    // Initialize the Dynamic Font for the Menu TopLabel
	    if (HighLimit <= MediumLimit) { HighLimit = 150; MediumLimit = 80; } // Safecheck to reset values if HighLimit is lower or equal to MediumLimit
	    Label.TextFont = Hud.Game.CurrentLatency >= MediumLimit ? (Hud.Game.CurrentLatency >= HighLimit ? RedFont : OrangeFont) : YellowFont;

            // Graph Dimensions not yet initialized
            if (GraphWidth < 1) return; 
            
            // Initialize latency history
            if (TickInterval == 0) 
            {
                TickInterval = (int)((float)(GraphDuration * 60)/GraphWidth);
                LastRecordedTick = Hud.Game.CurrentGameTick;
                LastSeenPing = Hud.Game.CurrentLatency;

                int capacity = (int)GraphWidth + 1;
                foreach (GraphLine line in LiveData)
                {
                    if (line.Data == null)
                        line.Data = new List<decimal>(capacity);
                    
                }

                return;
            }

	    // Check for changes in latency to record highest and lowest pings
            double latency = Hud.Game.CurrentLatency;
            double latencytick = LastSeenPing;
            if (latencytick != 0) 
            {
                if (latencytick > HighestPing) 
                {
                    if (HighestPing == LowestPing) LowestPing = latencytick;
                    HighestPing = latencytick;
                } else if (latencytick < LowestPing) 
                    LowestPing = latencytick;

                LastSeenPing = latency; // Remember the last ping value
            }

            // Record latency ticks during the intervals of time that are to be drawn in a graph
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
                else if (keyEvent.Key == Key.Left)
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
            decimal lagPerPixel = max / (decimal)GraphHeight;
            
            if (lagPerPixel <= 0) return; // No lag per pixel, no latency > 0 recorded
            
            // Draw axis markers
            if (YAxisMarkersCount > 0)
            {
                float markerX = x - 5;
                decimal interval = max / ((decimal)YAxisMarkersCount - 1);
                for (int i = 0; i <= YAxisMarkersCount; ++i)
                {
                    decimal dmg = (i == YAxisMarkersCount ? 0 : max - (interval * i));
                    float markerY = y + GraphHeight - (float)(dmg / lagPerPixel);
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
                        
                        gs.BeginFigure(new SharpDX.Vector2(x, y + GraphHeight - (float)(line.Data[line.Data.Count - 1]/lagPerPixel)), FigureBegin.Filled);
                        
                        for (int i = 1; i < line.Data.Count; ++i)
                        {
                            decimal data = line.Data[line.Data.Count - 1 - i];
                            if (data < 0)
                                gs.AddLine(new SharpDX.Vector2(x + i, y + GraphHeight));
                            else
                                gs.AddLine(new SharpDX.Vector2(x + i, y + GraphHeight - (float)(data/lagPerPixel)));
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
                                line.Brush.DrawEllipse(cursorX, y + GraphHeight - (float)(data/lagPerPixel), 5f, 5f);
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