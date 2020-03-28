// RunStats Blood Shards Helper by Resu

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
    public class LiveStats_BloodShardHelper : BasePlugin, IAfterCollectHandler, ICustomizer, IInGameTopPainter
    {
        public int BCsAtStart { get; set; } = -1;
        public int BCsLastSeen { get; set; }
        public int BCsSpent { get; set; }
        public int BCsGained { get; set; }
        public int BloodTotal { get; set; }
        public int BloodRemaining { get; set; }
        public double BloodPercent { get; set; }

        public int MaxBCsGainable { get; set; } = 500; // Max # of BCs gainable in a fraction of a second (Sometimes the game client takes a moment to load the current material count after the game has started)
        
        public TopLabelDecorator Label { get; set; }
        public IFont BCFont { get; set; }
        public IBrush BgBrush { get; set; }
        public IBrush BgBrushAlt { get; set; }
        public String WarningSign { get; set; }

        public bool UsePercent { get; set; }
        public int RemainingTreshold { get; set; }
        public double PercentTreshold { get; set; }

        public int Priority { get; set; } = 2;
        public int Hook { get; set; } = 0;

        public LiveStats_BloodShardHelper()
        {
            Enabled = true;
        }
        
        public void Customize()
        {
	    // Add this display to the RunStats readout
	    Hud.RunOnPlugin<LiveStatsPlugin>(plugin => {
		plugin.Add(this.Label, this.Priority, this.Hook);
	    });
	    // Turn off Default BloodShardPlugin
	    Hud.TogglePlugin<BloodShardPlugin>(false);
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

	    UsePercent = true; // Use % filled cap instead of remaining shards before cap

	    RemainingTreshold = 300;	// Remaining Blood Shards before cap Warning
	    PercentTreshold = 67;	// % Filled Blood Shards cap Warning

            BCFont = Hud.Render.CreateFont("tahoma", 7, 255, 234, 47, 0, false, false, true); //205, 102, 255 
            WarningSign = "\u26A0 ";

            var plugin = Hud.GetPlugin<LiveStatsPlugin>();
            BgBrush = plugin.BgBrush; 
            BgBrushAlt = plugin.BgBrushAlt;
            
            int delta = this.BCsGained - this.BCsSpent;
            Label = new TopLabelDecorator(Hud)
            {
                TextFont = BCFont,
                //BackgroundBrush = BgBrush,
                TextFunc = () =>{
                    double diff = (double)(this.BCsGained - this.BCsSpent) / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d);
			bool WarnActive = false; if (UsePercent && BloodPercent > PercentTreshold) WarnActive = true; 
			if (!UsePercent && BloodRemaining < RemainingTreshold) WarnActive = true;
                    return WarnActive ? WarningSign + Hud.Game.Me.Materials.BloodShard + "/" + BloodTotal : (diff > 0 ? "+" + diff.ToString("F0") : diff.ToString("F0")) + " bs/h";
                },
                //HintFunc = () => "Blood Shards Change Rate",
                ExpandUpLabels = new List<TopLabelDecorator>() {
                    new TopLabelDecorator(Hud) {
                        TextFont = BCFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont = BCFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "+" + (this.BCsGained / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F0") + " bs/h", //ValueToString(Hud.Game.Me.Defense.EhpCur, ValueFormat.ShortNumber),
                        HintFunc = () => "Gain Rate",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont = BCFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = BCFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "-" + (this.BCsSpent / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F0") + " bs/h",
                        HintFunc = () => "Spend Rate",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont = BCFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont = BCFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => this.BCsGained.ToString(),
                        HintFunc = () => "Shards Gained",

                    },
                    new TopLabelDecorator(Hud) {
                        TextFont = BCFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = BCFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => this.BCsSpent.ToString(),
                        HintFunc = () => "Shards Spent",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont = BCFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont = BCFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => Hud.Game.Me.Materials.BloodShard + "/" + BloodTotal,
                        HintFunc = () => "Current Total",
                    },
                },
            };
            
        }

        public void PaintTopInGame(ClipState clipState)
        {
            BloodTotal = 500 + (Hud.Game.Me.HighestSoloRiftLevel * 10);
            BloodRemaining = (int)(BloodTotal - Hud.Game.Me.Materials.BloodShard);
	    BloodPercent = (((double)Hud.Game.Me.Materials.BloodShard / BloodTotal) * 100);

            if (Hud.Time.Now.Second % 2 == 0)
                WarningSign = "\u26A0 "; // \u2800\u2800 -> To make it blink
            else
                WarningSign = "\u26A0 ";
        }

	public void AfterCollect()
	{
            if (!Hud.Game.IsInGame) return;
            
            int seen = (int)Hud.Game.Me.Materials.BloodShard;
            if (BCsAtStart == -1) {
                BCsAtStart = seen;
                BCsLastSeen = seen;
            }
            
            if (BCsLastSeen != seen) {
                int delta = seen - BCsLastSeen;
                
                if (Hud.Game.IsInTown && Math.Abs(delta) > MaxBCsGainable) // Gaining a lot of BC's while in town?
                {					
                    BCsAtStart = seen;
                    BCsSpent = 0;
                    BCsGained = 0;
                }
                else if (delta < 0)
                    BCsSpent += (int)Math.Abs(delta);
                else
                    BCsGained += delta;
                
                BCsLastSeen = seen;
            }
        }

    }
}