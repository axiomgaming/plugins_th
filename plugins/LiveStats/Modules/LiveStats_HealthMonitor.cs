// RunStats Health Meter by HaKache

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
    public class LiveStats_HealthMonitor : BasePlugin, ICustomizer, IKeyEventHandler, INewAreaHandler, IInGameTopPainter //, IAfterCollectHandler
    {
        public string TextSave { get; set; } = "SAVE";
        public string TextLive { get; set; } = "LIVE";
        public string TextNow { get; set; } = "now";

	public bool HideGraphInCombat { get; set; } = false; // Hide the full Graph when you're in combat
        public bool HideTooltipInCombat { get; set; } = false; // Hide only the Graph Tooltips when you're in combat

        public IFont TextFont { get; set; } // Label font
        public IBrush BgBrush { get; set; } // Label bg
        public IBrush BgBrushAlt { get; set; } // Label bg
        public IBrush BgBrushProc { get; set; } // Label bg
        public IBrush BgBrushProcAlt { get; set; } // Label bg
        public TopLabelDecorator Label { get; set; }
        public List<TopLabelDecorator> ExpandUpLabels { get; set; }

        public IBrush GraphBgBrush { get; set; }
        public IBrush MarkerBrush { get; set; }
        public IFont MarkerFont { get; set; }
        public IFont PinkFont { get; set; }
        public IFont BlueFont { get; set; }
        public IFont GreenFont { get; set; }
        public IFont YellowFont { get; set; }
        public IFont OrangeFont { get; set; }
        public IFont RedFont { get; set; }

        public IFont FlashGreenFont { get; set; }
        public IFont FlashRedFont { get; set; }
        public IFont NeutralFont { get; set; }
        
        public int GraphDuration { get; set; } = 60; // In seconds		
        public float GraphWidth { get; set; } // Screen size in pixels
        public float GraphHeight { get; set; } // Screen size in pixels
        public bool InvertGraph { get; set; } = true;
        public int YAxisMarkersCount { get; set; } = 5;
        public int XAxisMarkersCount { get; set; } = 5;
        
        public List<GraphLine> LiveData { get; private set; }
        public List<GraphLine> SaveData { get; private set; }
        public DateTime LiveTimestamp { get; private set; }
        public DateTime SaveTimestamp { get; private set; }
        public bool ShowSave { get; private set; } = false;

        public int TickInterval { get; private set; }
        private int LastRecordedTick;
        private double LastSeenHealth;
        private double HealthChangeRate;
        
        public class GraphLine
        {
            public bool Enabled { get; set; } = true;
            public List<double> Data { get; set; } = null;
            public string Name { get; set; }
            public Func<double> DataFunc { get; set; }
            public IBrush Brush { get; set; }
            public IFont Font { get; set; }
            public TopLabelDecorator Tooltip { get; set; } = null;

            public GraphLine(string name)
            {
               Name = name;
            }
            
            public GraphLine(GraphLine toCopy)
            {
                Data = new List<double>(toCopy.Data);
                DataFunc = toCopy.DataFunc;
                Name = toCopy.Name;
                Brush = toCopy.Brush;
                Font = toCopy.Font;
            }
        }

        public class MultiplayerRule
        {
            // Rules
	    public float Index { get; set; }
	    public StringGeneratorFunc Menu { get; set; }
            public Func<bool> IsRelevant { get; set; }	
            public StringGeneratorFunc Description { get; set; }

            // Track and display
            public TopLabelDecorator Label { get; set; }
            public IBrush BgBrush { get; set; } // Optional
            public IFont Font { get; set; } // Optional
            
            public MultiplayerRule() {}
        }
        public List<MultiplayerRule> Tracking { get; set; }

	public BuffRuleCalculator ProcRuleCalculator { get; set; } // Track if a player is procced
	public bool EnableProcTracker { get; set; } = true; // Enable or disable the Proc Tracker function

        public int Priority { get; set; } = 21;
        public int Hook { get; set; } = 0;
        
        public LiveStats_HealthMonitor()
        {
            Enabled = true;
        }
        
        public void Customize()
        {
            // Add this display to the LiveStats readout with a specified positional order priority of 21
            Hud.RunOnPlugin<LiveStatsPlugin>(plugin => 
            {
                plugin.Add(this.Label, this.Priority, this.Hook);
            });
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            
            TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 135, 135, 135, false, false, true);
            NeutralFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 234, 137, false, false, true);
            FlashGreenFont = Hud.Render.CreateFont("tahoma", 7, 255, 32, 220, 32, false, false, true);
            FlashRedFont = Hud.Render.CreateFont("tahoma", 7, 255, 230, 0, 0, false, false, true);

            BlueFont = Hud.Render.CreateFont("tahoma", 7, 255, 107, 96, 255, false, false, true);
            PinkFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 53, 123, false, false, true);
            GreenFont = Hud.Render.CreateFont("tahoma", 7, 255, 91, 237, 59, false, false, true);
            YellowFont = Hud.Render.CreateFont("tahoma", 7, 255, 211, 237, 59, false, false, true);
            OrangeFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 128, 43, false, false, true);
            RedFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 50, 50, false, false, true);
            
            GraphBgBrush = Hud.Render.CreateBrush(125, 0, 0, 0, 0);
            MarkerFont = Hud.Render.CreateFont("tahoma", 6, 200, 211, 228, 255, false, false, true);
            MarkerBrush = Hud.Render.CreateBrush(45, 190, 190, 190, 1);
            
            var plugin = Hud.GetPlugin<LiveStatsPlugin>();
            BgBrush = plugin.BgBrush;
            BgBrushAlt = plugin.BgBrushAlt;
            BgBrushProc = Hud.Render.CreateBrush(175, 182, 50, 80, 0);
            BgBrushProcAlt = Hud.Render.CreateBrush(175, 192, 30, 30, 0);

            Tracking = new List<MultiplayerRule>();
            
            // Myself
            Tracking.Add(new MultiplayerRule()
            {
		Index = 0,
		Menu = () => Hud.Game.Me.IsInGame ? Hud.Game.Me.BattleTagAbovePortrait : Hud.Game.Me.BattleTagAbovePortrait,
                IsRelevant = () => (Hud.Game.Me.IsInGame), // IsInRift() && Hud.Game.Me.InCombat
                Description = () => (Hud.Game.Me.Defense.HealthPct >= 100 || Hud.Game.Me.Defense.HealthPct == 0) ? Hud.Game.Me.Defense.HealthPct.ToString("F0", CultureInfo.InvariantCulture) + "% hp" : Hud.Game.Me.Defense.HealthPct.ToString("F1", CultureInfo.InvariantCulture) + "% hp",
                Font = PinkFont,
            });
            // Player 1
            Tracking.Add(new MultiplayerRule()
            {
		Index = 1,
		Menu = () => Hud.Game.NumberOfPlayersInGame > 1 ? Hud.Game.Players.Where(p => p.PortraitIndex == 1).First().BattleTagAbovePortrait : "N/A",
                IsRelevant = () => (Hud.Game.Me.IsInGame && Hud.Game.NumberOfPlayersInGame > 1 && Hud.Game.Players.Where(p => p.PortraitIndex == 1 && p.IsInGame).First() != null),
                Description = () => Hud.Game.NumberOfPlayersInGame > 1 && Hud.Game.Players.Where(p => p.PortraitIndex == 1) != null ? (Hud.Game.Players.Where(p => p.PortraitIndex == 1).First().Defense.HealthPct >= 100 || Hud.Game.Players.Where(p => p.PortraitIndex == 1).First().Defense.HealthPct == 0) ? Hud.Game.Players.Where(p => p.PortraitIndex == 1).First().Defense.HealthPct.ToString("F0", CultureInfo.InvariantCulture) + "% hp" : Hud.Game.Players.Where(p => p.PortraitIndex == 1).First().Defense.HealthPct.ToString("F1", CultureInfo.InvariantCulture) + "% hp" : "0% hp",
                Font = GreenFont,
            });
            // Player 2
            Tracking.Add(new MultiplayerRule()
            {
		Index = 2,
		Menu = () => Hud.Game.NumberOfPlayersInGame > 2 ? Hud.Game.Players.Where(p => p.PortraitIndex == 2).First().BattleTagAbovePortrait : "N/A",
                IsRelevant = () => (Hud.Game.Me.IsInGame && Hud.Game.NumberOfPlayersInGame > 2 && Hud.Game.Players.Where(p => p.PortraitIndex == 2 && p.IsInGame).First() != null),
                Description = () => Hud.Game.NumberOfPlayersInGame > 2 ? (Hud.Game.Players.Where(p => p.PortraitIndex == 2).First().Defense.HealthPct >= 100 || Hud.Game.Players.Where(p => p.PortraitIndex == 2).First().Defense.HealthPct == 0) ? Hud.Game.Players.Where(p => p.PortraitIndex == 2).First().Defense.HealthPct.ToString("F0", CultureInfo.InvariantCulture) + "% hp" : Hud.Game.Players.Where(p => p.PortraitIndex == 2).First().Defense.HealthPct.ToString("F1", CultureInfo.InvariantCulture) + "% hp" : "0% hp",
                Font = BlueFont,
            });
            // Player 3
            Tracking.Add(new MultiplayerRule()
            {
		Index = 3,
		Menu = () => Hud.Game.NumberOfPlayersInGame > 3 ? Hud.Game.Players.Where(p => p.PortraitIndex == 3).First().BattleTagAbovePortrait : "N/A",
                IsRelevant = () => (Hud.Game.Me.IsInGame && Hud.Game.NumberOfPlayersInGame > 3 && Hud.Game.Players.Where(p => p.PortraitIndex == 3 && p.IsInGame).First() != null),
                Description = () => Hud.Game.NumberOfPlayersInGame > 3 ? (Hud.Game.Players.Where(p => p.PortraitIndex == 3).First().Defense.HealthPct >= 100 || Hud.Game.Players.Where(p => p.PortraitIndex == 3).First().Defense.HealthPct == 0) ? Hud.Game.Players.Where(p => p.PortraitIndex == 3).First().Defense.HealthPct.ToString("F0", CultureInfo.InvariantCulture) + "% hp" : Hud.Game.Players.Where(p => p.PortraitIndex == 3).First().Defense.HealthPct.ToString("F1", CultureInfo.InvariantCulture) + "% hp" : "0% hp",
                Font = YellowFont,
            });

            // Create all the players hover labels based on number of players in game
            ExpandUpLabels = new List<TopLabelDecorator>();
            foreach (MultiplayerRule rule in Tracking)
            {
                TopLabelDecorator label = new TopLabelDecorator(Hud) {
                    TextFont = (rule.Font == null ? TextFont : rule.Font),
                    BackgroundBrush = (rule.BgBrush == null ? BgBrush : rule.BgBrush),
                    ExpandedHintFont = (rule.Font == null ? TextFont : rule.Font),
                    ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                    TextFunc = rule.Description,
                    HintFunc = rule.Menu,
                };
                
                rule.Label = label;
                ExpandUpLabels.Add(label);
            }

                    TopLabelDecorator labelgraph = new TopLabelDecorator(Hud) {
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
		};

                ExpandUpLabels.Add(labelgraph);

            // Create the main menu label
            Label = new TopLabelDecorator(Hud)
            {
                TextFont = PinkFont,
                TextFunc = () => 
                {
                    // Initialize graph size
                    if (GraphWidth < 1) 
                    {
                        GraphWidth = Hud.Window.Size.Width - Hud.Render.MinimapUiElement.Rectangle.X - 5; // Scale to the width of the minimap
                        GraphHeight = GraphWidth * 0.7f;
                    }

                    return (Hud.Game.Me.Defense.HealthPct >= 100 || Hud.Game.Me.Defense.HealthPct == 0) ? Hud.Game.Me.Defense.HealthPct.ToString("F0", CultureInfo.InvariantCulture) + "% hp" : Hud.Game.Me.Defense.HealthPct.ToString("F1", CultureInfo.InvariantCulture) + "% hp";
                },
                HintFunc = () => "Current HP %",
                ExpandUpLabels = this.ExpandUpLabels,
            };
            
            // Partially initialize, but don't preallocate Data memory until the graph size is determined
            LiveData = new List<GraphLine>() { 
                new GraphLine("P3")
                { 
                    //Name = "P3", 
		    Enabled = false,
                    DataFunc = () => Hud.Game.Me.IsInGame && Hud.Game.NumberOfPlayersInGame > 3 && Hud.Game.Players.Where(p => p.PortraitIndex == 3 && p.IsInGame).First() != null ? (double)Hud.Game.Players.Where(p => p.PortraitIndex == 3).First().Defense.HealthPct : 0,
                    Brush = Hud.Render.CreateBrush(255, 211, 237, 59, 1), 
                    Font = YellowFont,
                },
                new GraphLine("P2")
                { 
                    //Name = "P2", 
		    Enabled = false,
                    DataFunc = () => Hud.Game.Me.IsInGame && Hud.Game.NumberOfPlayersInGame > 2 && Hud.Game.Players.Where(p => p.PortraitIndex == 2 && p.IsInGame).First() != null ? (double)Hud.Game.Players.Where(p => p.PortraitIndex == 2).First().Defense.HealthPct : 0,
                    Brush = Hud.Render.CreateBrush(255, 107, 96, 255, 1), 
                    Font = BlueFont,
                },
                new GraphLine("P1")
                { 
                    //Name = "P1", 
		    Enabled = false,
                    DataFunc = () => Hud.Game.Me.IsInGame && Hud.Game.NumberOfPlayersInGame > 1 && Hud.Game.Players.Where(p => p.PortraitIndex == 1 && p.IsInGame).First() != null ? (double)Hud.Game.Players.Where(p => p.PortraitIndex == 1).First().Defense.HealthPct : 0,
                    Brush = Hud.Render.CreateBrush(255, 91, 237, 59, 1), 
                    Font = GreenFont,
                },
                new GraphLine("Me") 
                { 
                    //Name = "Me", 
		    Enabled = true,
                    DataFunc = () => Hud.Game.Me.IsInGame ? (double)Hud.Game.Me.Defense.HealthPct : 0,
                    Brush = Hud.Render.CreateBrush(255, 255, 53, 123, 1), 
                    Font = PinkFont,
                },
            };

	    ProcRuleCalculator = new BuffRuleCalculator(Hud);
	    ProcRuleCalculator.SizeMultiplier = 0.65f;
	    ProcRuleCalculator.Rules.Add(new BuffRule(Hud.Sno.SnoPowers.Wizard_Passive_UnstableAnomaly.Sno) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false});	// Passive downtime
	    ProcRuleCalculator.Rules.Add(new BuffRule(Hud.Sno.SnoPowers.Monk_Passive_NearDeathExperience.Sno) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false});	// Passive downtime
	    ProcRuleCalculator.Rules.Add(new BuffRule(359580) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false});							// Firebird 2P downtime
	    ProcRuleCalculator.Rules.Add(new BuffRule(Hud.Sno.SnoPowers.Barbarian_Passive_NervesOfSteel.Sno) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false});	// Cooldown
	    ProcRuleCalculator.Rules.Add(new BuffRule(Hud.Sno.SnoPowers.Necromancer_Passive_FinalService.Sno) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false});	// Cooldown
	    ProcRuleCalculator.Rules.Add(new BuffRule(Hud.Sno.SnoPowers.Crusader_Passive_Indestructible.Sno) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false});	// Passive cooldown
	    ProcRuleCalculator.Rules.Add(new BuffRule(Hud.Sno.SnoPowers.DemonHunter_Passive_Awareness.Sno) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false});	// Passive cooldown
	    ProcRuleCalculator.Rules.Add(new BuffRule(Hud.Sno.SnoPowers.WitchDoctor_Passive_SpiritVessel.Sno) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false});	// Passive cooldown
            
        }
        
        public void OnNewArea(bool newGame, ISnoArea area)
        {
            if (newGame)
                LastRecordedTick = 0;
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (!Hud.Game.IsInGame) return;

            bool isAnyRelevant = false;
            int alternate = 0;
            foreach (MultiplayerRule rule in Tracking)
            {
                // Show or hide labels based on whether or not the number of players in the game is relevant
                if (!rule.IsRelevant())
                {
                    rule.Label.Enabled = false;
                    continue;
                }
                rule.Label.Enabled = true;
                rule.Label.BackgroundBrush = (alternate++ % 2 == 0 ? BgBrush : BgBrushAlt);

		if (EnableProcTracker) {
		// Check if player is procced to change bg label color
		    foreach (IPlayer player in Hud.Game.Players.Where(p => p.HasValidActor && p.SnoArea == Hud.Game.Me.SnoArea && p.CoordinateKnown))
		    {
			ProcRuleCalculator.CalculatePaintInfo(player);

			if (ProcRuleCalculator.PaintInfoList.Count > 0 && player.PortraitIndex == 0 && rule.Index == 0) rule.Label.BackgroundBrush = (alternate++ % 2 == 0 ? BgBrushProc : BgBrushProcAlt);
			else if (ProcRuleCalculator.PaintInfoList.Count > 0 && player.PortraitIndex == 1 && rule.Index == 1) rule.Label.BackgroundBrush = (alternate++ % 2 == 0 ? BgBrushProc : BgBrushProcAlt);
			else if (ProcRuleCalculator.PaintInfoList.Count > 0 && player.PortraitIndex == 2 && rule.Index == 2) rule.Label.BackgroundBrush = (alternate++ % 2 == 0 ? BgBrushProc : BgBrushProcAlt);
			else if (ProcRuleCalculator.PaintInfoList.Count > 0 && player.PortraitIndex == 3 && rule.Index == 3) rule.Label.BackgroundBrush = (alternate++ % 2 == 0 ? BgBrushProc : BgBrushProcAlt);
		    }
		}

                isAnyRelevant = true;
            }
            Label.Enabled = isAnyRelevant;

            // Graph dimensions not yet initialized
            if (GraphWidth < 1) return; 
            
            // Initialize health change history
            if (TickInterval == 0) 
            {
                TickInterval = (int)((float)(GraphDuration * 60)/(GraphWidth)); 
                LastRecordedTick = Hud.Game.CurrentGameTick;
                LastSeenHealth = Hud.Game.Me.Defense.HealthCur;

                int capacity = (int)GraphWidth + 1;
                foreach (GraphLine line in LiveData)
                {
                    if (line.Data == null)
                        line.Data = new List<double>(capacity);
                    
                }

                return;
            }

	    // Check for changes in health to record healing and damage taken values - Deprecated (Later ?)
            double health = Hud.Game.Me.Defense.HealthCur; 
            double healthtick = LastSeenHealth; 

            if (healthtick != health) HealthChangeRate = (healthtick - health) * -1; 

            LastSeenHealth = health; // Remember the last health value
            

            // Record health changes during the intervals of time that are to be drawn in a graph
            if (Hud.Game.CurrentGameTick >= LastRecordedTick + TickInterval) 
            {
                foreach (GraphLine line in LiveData)
                {

		// Enable graphs depending on number of players in game (Not disabled after to still access the datas at anytime)
		if (Hud.Game.NumberOfPlayersInGame > 1 && line.Name == "P1" && !line.Enabled) line.Enabled = true;
		if (Hud.Game.NumberOfPlayersInGame > 2 && line.Name == "P2" && !line.Enabled) line.Enabled = true;
		if (Hud.Game.NumberOfPlayersInGame > 3 && line.Name == "P3" && !line.Enabled) line.Enabled = true;

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
            
            // if (keyEvent.IsPressed)
            // {
                // if (keyEvent.Key == Key.Subtract)
                    // InvertGraph = false;
                // else if (keyEvent.Key == Key.Add)
                    // InvertGraph = true;
                // else if (keyEvent.Key == Key.Right).
                    // SaveGraph();
                // else if (keyEvent.Key == Key.Up)
                    // ToggleBetweenGraphs();
            // }
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

            double max = Recordings.Select(r => r.Data.Max()).Max();
            double hpPerPixel = max / (double)GraphHeight;
            
            if (hpPerPixel <= 0) return; // No hp per pixel, no health > 0 recorded
            
            // Draw axis markers
            if (YAxisMarkersCount > 0)
            {
                float markerX = x - 5;
                double interval = max / ((double)YAxisMarkersCount - 1);
                for (int i = 0; i <= YAxisMarkersCount; ++i)
                {
                    double dmg = (i == YAxisMarkersCount ? 0 : max - (interval * i));
                    float markerY = y + GraphHeight - (float)(dmg / hpPerPixel);
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
                        
                        gs.BeginFigure(new SharpDX.Vector2(x, y + GraphHeight - (float)(line.Data[line.Data.Count - 1]/hpPerPixel)), FigureBegin.Filled);
                        
                        for (int i = 1; i < line.Data.Count; ++i)
                        {
                            double data = line.Data[line.Data.Count - 1 - i];
                            if (data < 0)
                                gs.AddLine(new SharpDX.Vector2(x + i, y + GraphHeight));
                            else
                                gs.AddLine(new SharpDX.Vector2(x + i, y + GraphHeight - (float)(data/hpPerPixel)));
                        }
                            
                        gs.EndFigure(FigureEnd.Open); //FigureEnd.Closed //FigureEnd.Open
                        gs.Close();
                    }
                    
                     if (line.Enabled) line.Brush.DrawGeometry(pg); // Only draw the graphline if it is enabled
                }
            }
            
            // Draw tooltip
            if (HideTooltipInCombat && Hud.Game.Me.InCombat) return;

		var nameMe = Hud.Game.Me.BattleTagAbovePortrait.ToString() + " : ";
		var healthMe = Hud.Game.Me.Defense.HealthMax;
		var nameP1 = "P1 : "; var healthP1 = healthMe;
		var nameP2 = "P2 : "; var healthP2 = healthMe;
		var nameP3 = "P3 : "; var healthP3 = healthMe;

		if (Hud.Game.NumberOfPlayersInGame > 1 && Hud.Game.Players.Where(p => p.PortraitIndex == 1 && p.IsInGame).First() != null) nameP1 = Hud.Game.Players.Where(p => p.PortraitIndex == 1).First().BattleTagAbovePortrait.ToString() + " : ";
		if (Hud.Game.NumberOfPlayersInGame > 2 && Hud.Game.Players.Where(p => p.PortraitIndex == 2 && p.IsInGame).First() != null) nameP2 = Hud.Game.Players.Where(p => p.PortraitIndex == 2).First().BattleTagAbovePortrait.ToString() + " : ";
		if (Hud.Game.NumberOfPlayersInGame > 3 && Hud.Game.Players.Where(p => p.PortraitIndex == 3 && p.IsInGame).First() != null) nameP3 = Hud.Game.Players.Where(p => p.PortraitIndex == 3).First().BattleTagAbovePortrait.ToString() + " : ";

		if (Hud.Game.NumberOfPlayersInGame > 1 && Hud.Game.Players.Where(p => p.PortraitIndex == 1 && p.IsInGame).First() != null && Hud.Game.Players.Where(p => p.PortraitIndex == 1).First().SnoArea == Hud.Game.Me.SnoArea) healthP1 = Hud.Game.Players.Where(p => p.PortraitIndex == 1).First().Defense.HealthMax;
		if (Hud.Game.NumberOfPlayersInGame > 2 && Hud.Game.Players.Where(p => p.PortraitIndex == 2 && p.IsInGame).First() != null && Hud.Game.Players.Where(p => p.PortraitIndex == 2).First().SnoArea == Hud.Game.Me.SnoArea) healthP2 = Hud.Game.Players.Where(p => p.PortraitIndex == 2).First().Defense.HealthMax;
		if (Hud.Game.NumberOfPlayersInGame > 3 && Hud.Game.Players.Where(p => p.PortraitIndex == 3 && p.IsInGame).First() != null && Hud.Game.Players.Where(p => p.PortraitIndex == 3).First().SnoArea == Hud.Game.Me.SnoArea) healthP3 = Hud.Game.Players.Where(p => p.PortraitIndex == 3).First().Defense.HealthMax;

                float cursorX = (float)Hud.Window.CursorX;
                float cursorY = (float)Hud.Window.CursorY;
                bool yValidHover = (cursorY <= y + GraphHeight + 5 && cursorY >= y - 5);
                bool xValidHover = (cursorX >= x - 5 && cursorX <= x + Recordings[0].Data.Count + 5);
                
                if (yValidHover) 
                {
                    if (xValidHover) 
                    {
                        List<Tuple<TextLayout, TextLayout, IFont, IFont>> tooltips = new List<Tuple<TextLayout, TextLayout, IFont, IFont>>(Recordings.Count);
                        float labelWidth = 0;
                        float valueWidth = 0;
                        float height = 0;
                        
                        foreach (GraphLine line in Recordings)
                        {
			
                            double data; double lastdata;
                            if (cursorX >= x - 5 && cursorX <= x) // Accounting for a little edge error
                                { data = line.Data[line.Data.Count - 1]; lastdata = line.Data[line.Data.Count - 2]; }
                            else if (cursorX >= x + line.Data.Count && cursorX <= x + line.Data.Count + 5) // Accounting for a little edge error
                                { data = line.Data[0]; lastdata = line.Data[line.Data.Count - 1]; }
                            else
                                { 
				data = line.Data[line.Data.Count - 1 - (int)(cursorX - x)]; 
				// Dirty workaround that mostly works...
				if ((line.Data.Count - 2 - (int)(cursorX - x)) != -1) lastdata = line.Data[line.Data.Count - 2 - (int)(cursorX - x)];
					else lastdata = line.Data[line.Data.Count - 1 - (int)(cursorX - x)];
				}

				// Initialize the player Btag in the tooltip
				string playerName;
				if (line.Name == "Me") playerName = nameMe;
				    else if (line.Name == "P1") playerName = nameP1;
				    else if (line.Name == "P2") playerName = nameP2;
				    else if (line.Name == "P3") playerName = nameP3;
					else playerName = line.Name + " : ";

				// Initialize the health change in the tooltip
				string playerHealthChange;
				if (line.Name == "Me") playerHealthChange = (double)(data - lastdata) > 0 ? "+" + ValueToString((double)(data - lastdata) * healthMe / 100, ValueFormat.LongNumber) : ValueToString((double)(data - lastdata) * healthMe / 100, ValueFormat.LongNumber);
				    else if (line.Name == "P1" && healthP1 != healthMe) playerHealthChange = (double)(data - lastdata) > 0 ? "+" + ValueToString((double)(data - lastdata) * healthP1 / 100, ValueFormat.LongNumber) : ValueToString((double)(data - lastdata) * healthP1 / 100, ValueFormat.LongNumber);
				    else if (line.Name == "P2" && healthP2 != healthMe) playerHealthChange = (double)(data - lastdata) > 0 ? "+" + ValueToString((double)(data - lastdata) * healthP2 / 100, ValueFormat.LongNumber) : ValueToString((double)(data - lastdata) * healthP2 / 100, ValueFormat.LongNumber);
				    else if (line.Name == "P3" && healthP3 != healthMe) playerHealthChange = (double)(data - lastdata) > 0 ? "+" + ValueToString((double)(data - lastdata) * healthP3 / 100, ValueFormat.LongNumber) : ValueToString((double)(data - lastdata) * healthP3 / 100, ValueFormat.LongNumber);
					else playerHealthChange = (double)(data - lastdata) > 0 ? "+" + ValueToString((double)(data - lastdata), ValueFormat.LongNumber) + "%" : ValueToString((double)(data - lastdata), ValueFormat.LongNumber) + "%";

                            var tooltip = new Tuple<TextLayout, TextLayout, IFont, IFont>(
                                line.Font.GetTextLayout(playerName), 
                                //line.Font.GetTextLayout(data == -1 ? "N/A" : playerHealthChange + " (" + ValueToString((double)data, ValueFormat.NormalNumber) + "%)"),
                                line.Font.GetTextLayout(data == -1 ? "N/A" : (data >= 100 || data == 0) ? playerHealthChange + " (" + data.ToString("F0", CultureInfo.InvariantCulture) + "%)" : playerHealthChange + " (" + data.ToString("F1", CultureInfo.InvariantCulture) + "%)"),
                                line.Font,
				((double)(data - lastdata) >= 0 ? (((double)(data - lastdata) == 0) ? NeutralFont : FlashGreenFont) : FlashRedFont)
                            );
                            
                            // Update max widths
                            labelWidth = (float)Math.Max(tooltip.Item1.Metrics.Width, labelWidth);						
                            valueWidth = (float)Math.Max(tooltip.Item2.Metrics.Width, valueWidth);
                            height = (float)Math.Max(tooltip.Item2.Metrics.Height, height);
                            
                            // Add line if the corresponding graphline is enabled
                            if (line.Enabled) tooltips.Add(tooltip);
                            
                            // Draw circle around the data point on the graph if the corresponding graphline is enabled
                            if (line.Enabled && data != -1)
                                line.Brush.DrawEllipse(cursorX, y + GraphHeight - (float)(data/hpPerPixel), 5f, 5f);
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
                            tooltip.Item4.DrawText(tooltip.Item2, tooltipX + 5, tooltipY);
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