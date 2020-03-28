// TopStats Materials Tracker by HaKache

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
    public class LiveStats_MaterialTracker : BasePlugin, ICustomizer, IAfterCollectHandler //, IInGameWorldPainter //, IInGameTopPainter
	{
	public int DBsAtStart { get; set; } = -1;
	public int DBsLastSeen { get; set; }
	public int DBsSpent { get; set; }
	public int DBsGained { get; set; }
	public int MaxDBsGainable { get; set; } = 200; // Max # of DBs gainable in a fraction of a second (Sometimes the game client takes a moment to load the current material count after the game has started)

	public int SoulsAtStart { get; set; } = -1;
	public int SoulsLastSeen { get; set; }
	public int SoulsSpent { get; set; }
	public int SoulsGained { get; set; }
	public int MaxSoulsGainable { get; set; } = 200; // Max # of Souls gainable in a fraction of a second

        public int BCsAtStart { get; set; } = -1;
        public int BCsLastSeen { get; set; }
        public int BCsSpent { get; set; }
        public int BCsGained { get; set; }
        public int MaxBCsGainable { get; set; } = 500; // Max # of BCs gainable in a fraction of a second
 	
	public TopLabelDecorator Label { get; set; }
	public TopLabelDecorator Label0 { get; set; }
	public List<TopLabelDecorator> ExpandUpLabels { get; private set; }

	public IFont TitleFont { get; set; }
        public IFont WhiteFont { get; set; }
	public IFont SoulFont { get; set; }
	public IFont BSFont { get; set; }
	public IFont GoldFont { get; set; }
	public IFont DBFont { get; set; }
	public IFont DefaultFont { get; set; }

	public IFont TimeFont { get; set; }
	public IBrush BgBrush { get; set; }
	public IBrush BgBrushAlt { get; set; }
	public IBrush BgBrushTitle { get; set; }
	public IBrush HighlightBrush { get; set; }

        public int Priority { get; set; } = 3;
        public int Hook { get; set; } = 1;
		
        public LiveStats_MaterialTracker()
	{
	    Enabled = true;
	}
		
	public void Customize()
	{
		// Add this display to the LiveStats readout with a specified positional order priority of 3
		Hud.RunOnPlugin<LiveStatsPlugin>(plugin => {
			if (this.Hook == 0) plugin.Add(this.Label0, this.Priority, this.Hook);
				else plugin.Add(this.Label, this.Priority, this.Hook);			
		});
	}

        public override void Load(IController hud)
        {
            base.Load(hud);

	    TitleFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 188, 20, true, false, true);	
            WhiteFont = Hud.Render.CreateFont("tahoma", 7, 255, 245, 245, 245, false, false, true);
	    SoulFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 120, 0, false, false, true);
	    BSFont = Hud.Render.CreateFont("tahoma", 7, 255, 234, 47, 0, false, false, true);
	    DBFont = Hud.Render.CreateFont("tahoma", 7, 255, 32, 180, 140, false, false, true); //205, 102, 255
	    GoldFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 188, 20, false, false, true);

	    TimeFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 255, 255, false, false, true);
	    DefaultFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 140, 0, false, false, true);
			
	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
	    BgBrush = plugin.BgBrush;
	    BgBrushAlt = plugin.BgBrushAlt;
	    BgBrushTitle = Hud.Render.CreateBrush(175, 102, 100, 0, 0);
	    HighlightBrush = Hud.Render.CreateBrush(200, 72, 132, 84, 0);
			
            Label = new TopLabelDecorator(Hud)
            {
                TextFont = GoldFont,
                TextFunc = () => "materials",
                HintFunc = () => "Materials",
                ExpandUpLabels = new List<TopLabelDecorator>() {
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = TitleFont,
                        BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont =  TitleFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => "Account",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = GoldFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  GoldFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => string.Format("{0} ({1}/h)", ValueToString(Hud.Tracker.CurrentAccountTotal.GainedGold, ValueFormat.ShortNumber), ValueToString(Hud.Tracker.CurrentAccountTotal.GainedGoldPerHour, ValueFormat.ShortNumber)),
                        HintFunc = () => "Gold",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = BSFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  BSFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => string.Format("{0} ({1}/h)", ValueToString(Hud.Tracker.CurrentAccountTotal.DropBloodShard, ValueFormat.ShortNumber), ValueToString(Hud.Tracker.CurrentAccountTotal.DropBloodShardPerHour, ValueFormat.ShortNumber)),
                        HintFunc = () => "Blood Shards",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = GoldFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  GoldFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => ValueToString(Hud.Game.Me.Materials.Gold, ValueFormat.ShortNumber),
                        HintFunc = () => "Current Gold",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  DBFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  DBFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => Hud.Game.Me.Materials.DeathsBreath.ToString(),
                        HintFunc = () => "Total DBs",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  SoulFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  SoulFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => Hud.Game.Me.Materials.ForgottenSoul.ToString(),
                        HintFunc = () => "Total Souls",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = TitleFont,
                        BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont =  TitleFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => "Session",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = GoldFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  GoldFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => string.Format("{0} ({1}/h)", ValueToString(Hud.Tracker.Session.GainedGold, ValueFormat.ShortNumber), ValueToString(Hud.Tracker.Session.GainedGoldPerHour, ValueFormat.ShortNumber)),
                        HintFunc = () => "Gold",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = BSFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  BSFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => {
					double gainrate = (double)(this.BCsGained) / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d);
					return this.BCsGained.ToString() + " (" + gainrate.ToString("F0") + "/h)";
					},
                        HintFunc = () => "Blood Shards",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  DBFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  DBFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => {
					double diff = (double)(this.DBsGained - this.DBsSpent) / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d);
					return (this.DBsGained - this.DBsSpent).ToString() + " (" + (diff > 0 ? "+" + diff.ToString("F0") : diff.ToString("F0")) + "/h)";
					},
                        HintFunc = () => "Death's Breaths",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  SoulFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  SoulFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => {
					double diff = (double)(this.SoulsGained - this.SoulsSpent) / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d);
					return (this.SoulsGained - this.SoulsSpent).ToString() + " (" + (diff > 0 ? "+" + diff.ToString("F0") : diff.ToString("F0")) + "/h)";
					},
                        HintFunc = () => "Forgotten Souls",
                    },

		},
	    };

            Label0 = new TopLabelDecorator(Hud) // Menu for RunStats
            {
                TextFont = GoldFont,
                TextFunc = () => "materials",
                HintFunc = () => "Materials",
                ExpandUpLabels = new List<TopLabelDecorator>() {
                    new TopLabelDecorator(Hud) {
                        TextFont =  SoulFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  SoulFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => {
					double diff = (double)(this.SoulsGained - this.SoulsSpent) / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d);
					return (this.SoulsGained - this.SoulsSpent).ToString() + " (" + (diff > 0 ? "+" + diff.ToString("F0") : diff.ToString("F0")) + "/h)";
					},
                        HintFunc = () => "Forgotten Souls",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  DBFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  DBFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => {
					double diff = (double)(this.DBsGained - this.DBsSpent) / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d);
					return (this.DBsGained - this.DBsSpent).ToString() + " (" + (diff > 0 ? "+" + diff.ToString("F0") : diff.ToString("F0")) + "/h)";
					},
                        HintFunc = () => "Death's Breaths",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = BSFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  BSFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => {
					double gainrate = (double)(this.BCsGained) / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d);
					return this.BCsGained.ToString() + " (" + gainrate.ToString("F0") + "/h)";
					},
                        HintFunc = () => "Blood Shards",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = GoldFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  GoldFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => string.Format("{0} ({1}/h)", ValueToString(Hud.Tracker.Session.GainedGold, ValueFormat.ShortNumber), ValueToString(Hud.Tracker.Session.GainedGoldPerHour, ValueFormat.ShortNumber)),
                        HintFunc = () => "Gold",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = TitleFont,
                        BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont =  TitleFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => "Session",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  SoulFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  SoulFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => Hud.Game.Me.Materials.ForgottenSoul.ToString(),
                        HintFunc = () => "Total Souls",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  DBFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  DBFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => Hud.Game.Me.Materials.DeathsBreath.ToString(),
                        HintFunc = () => "Total DBs",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = GoldFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  GoldFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => ValueToString(Hud.Game.Me.Materials.Gold, ValueFormat.ShortNumber),
                        HintFunc = () => "Current Gold",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = BSFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  BSFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => string.Format("{0} ({1}/h)", ValueToString(Hud.Tracker.CurrentAccountTotal.DropBloodShard, ValueFormat.ShortNumber), ValueToString(Hud.Tracker.CurrentAccountTotal.DropBloodShardPerHour, ValueFormat.ShortNumber)),
                        HintFunc = () => "Blood Shards",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = GoldFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  GoldFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => string.Format("{0} ({1}/h)", ValueToString(Hud.Tracker.CurrentAccountTotal.GainedGold, ValueFormat.ShortNumber), ValueToString(Hud.Tracker.CurrentAccountTotal.GainedGoldPerHour, ValueFormat.ShortNumber)),
                        HintFunc = () => "Gold",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = TitleFont,
                        BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont =  TitleFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => "Account",
                        HintFunc = () => " ",
                    },
		},
	    };
			
		if (Hook == 0) ExpandUpLabels = Label0.ExpandUpLabels;
			else ExpandUpLabels = Label.ExpandUpLabels;
	}
		

	public void AfterCollect()
	{
		if (!Hud.Game.IsInGame) return;
			
		int seen = (int)Hud.Game.Me.Materials.DeathsBreath;
		int souls = (int)Hud.Game.Me.Materials.ForgottenSoul;
		int bshards = (int)Hud.Game.Me.Materials.BloodShard;

		if (DBsAtStart == -1) {
			DBsAtStart = seen;
			DBsLastSeen = seen;
		}
		if (SoulsAtStart == -1) {
			SoulsAtStart = souls;
			SoulsLastSeen = souls;
		}
            	if (BCsAtStart == -1) {
                	BCsAtStart = bshards;
                	BCsLastSeen = bshards;
            	}

		if (SoulsLastSeen != souls) {
		    int delta2 = souls - SoulsLastSeen;
		    if (Hud.Game.IsInTown && Math.Abs(delta2) > MaxSoulsGainable) // Gaining a lot of souls while in town
			{					
				SoulsAtStart = souls;
				SoulsSpent = 0;
				SoulsGained = 0;
			}
		    else if (delta2 < 0)
				SoulsSpent += (int)Math.Abs(delta2);
		    else
				SoulsGained += delta2;
				
				SoulsLastSeen = souls;
			}
		
		if (DBsLastSeen != seen) {
		    int delta = seen - DBsLastSeen;	
		    if (Hud.Game.IsInTown && Math.Abs(delta) > MaxDBsGainable) // Gaining a lot of db's while in town?
			{					
				DBsAtStart = seen;
				DBsSpent = 0;
				DBsGained = 0;
			}
		    else if (delta < 0)
				DBsSpent += (int)Math.Abs(delta);
		    else
				DBsGained += delta;
				
				DBsLastSeen = seen;
			}

            	if (BCsLastSeen != bshards) {
                	int delta = bshards - BCsLastSeen;
                	if (Hud.Game.IsInTown && Math.Abs(delta) > MaxBCsGainable) // Gaining a lot of BC's while in town?
                	    {					
                    		BCsAtStart = bshards;
                    		BCsSpent = 0;
                    		BCsGained = 0;
                	}
                else if (delta < 0)
                    		BCsSpent += (int)Math.Abs(delta);
                else
                    		BCsGained += delta;
                
                		BCsLastSeen = bshards;
			}
	}

    }
}