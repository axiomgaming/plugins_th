// RunStats Death's Breaths Helper by Razor

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
    public class LiveStats_DeathBreathHelper : BasePlugin, IAfterCollectHandler, ICustomizer //, IKeyEventHandler //, IInGameTopPainter
	{
	public int DBsAtStart { get; set; } = -1;
	public int DBsLastSeen { get; set; }
	public int DBsSpent { get; set; }
	public int DBsGained { get; set; }
		
	public int MaxDBsGainable { get; set; } = 100; //max number of DBs gainable in a fraction of a second (sometimes the game client takes a moment to load the current material count after the game has started)
		
	public TopLabelDecorator Label { get; set; }
	public IFont DBFont { get; set; }
	public IBrush BgBrush { get; set; }
	public IBrush BgBrushAlt { get; set; }

        public int Priority { get; set; } = 4;
        public int Hook { get; set; } = 0;
		
        public LiveStats_DeathBreathHelper()
	{
	    Enabled = true;
	}
		
	public void Customize()
	{
	    // Add this display to the LiveStats readout
	    Hud.RunOnPlugin<LiveStatsPlugin>(plugin => {
		plugin.Add(this.Label, this.Priority, this.Hook);
	    });
	}

        public override void Load(IController hud)
        {
            base.Load(hud);
			
	    DBFont = Hud.Render.CreateFont("tahoma", 7, 255, 32, 180, 140, false, false, true); //205, 102, 255
			
	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
	    BgBrush = plugin.BgBrush; 
	    BgBrushAlt = plugin.BgBrushAlt;
			
	    int delta = this.DBsGained - this.DBsSpent;
	    Label = new TopLabelDecorator(Hud)
	    {
                TextFont = DBFont,
		//BackgroundBrush = BgBrush,
		TextFunc = () => {
			double diff = (double)(this.DBsGained - this.DBsSpent) / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d);
			return (diff > 0 ? "+" + diff.ToString("F0") : diff.ToString("F0")) + " db/h";
			},
                //HintFunc = () => "Deaths Breath Change Rate",
		ExpandUpLabels = new List<TopLabelDecorator>() {
		    new TopLabelDecorator(Hud) {
			TextFont = DBFont,
			BackgroundBrush = BgBrush,
			ExpandedHintFont = DBFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => "+" + (this.DBsGained / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F0") + " DBs/h", //ValueToString(Hud.Game.Me.Defense.EhpCur, ValueFormat.ShortNumber),
			HintFunc = () => "Gain Rate",
		    },
		    new TopLabelDecorator(Hud) {
			TextFont = DBFont,
			BackgroundBrush = BgBrushAlt,
			ExpandedHintFont = DBFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => "-" + (this.DBsSpent / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F0") + " DBs/h",
			HintFunc = () => "Spend Rate",
		    },
		    new TopLabelDecorator(Hud) {
			TextFont = DBFont,
			BackgroundBrush = BgBrush,
			ExpandedHintFont = DBFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => this.DBsGained.ToString(),
			HintFunc = () => "Breaths Gained",
		    },
		    new TopLabelDecorator(Hud) {
			TextFont = DBFont,
			BackgroundBrush = BgBrushAlt,
			ExpandedHintFont = DBFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => this.DBsSpent.ToString(),
			HintFunc = () => "Breaths Spent",
		    },
		    new TopLabelDecorator(Hud) {
			TextFont = DBFont,
			BackgroundBrush = BgBrush,
			ExpandedHintFont = DBFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => Hud.Game.Me.Materials.DeathsBreath.ToString(),
			HintFunc = () => "Current Total",
		    },
		},
            };
			
		}
		
	public void AfterCollect()
	{
		if (!Hud.Game.IsInGame) return;
			
		int seen = (int)Hud.Game.Me.Materials.DeathsBreath;
		if (DBsAtStart == -1) {
			DBsAtStart = seen;
			DBsLastSeen = seen;
		}
			
		if (DBsLastSeen != seen) {
			int delta = seen - DBsLastSeen;
			
			if (Hud.Game.IsInTown && Math.Abs(delta) > MaxDBsGainable) //gaining a lot of db's while in town?
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
	}

    }
}