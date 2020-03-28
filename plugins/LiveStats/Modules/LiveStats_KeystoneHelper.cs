// RunStats Keystone Helper by Razor

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
    public class LiveStats_KeystoneHelper : BasePlugin, IAfterCollectHandler, ICustomizer //, IKeyEventHandler //, IInGameTopPainter
	{
	public int KeysAtStart { get; set; } = -1;
	public int KeysLastSeen { get; set; }
	public int KeysSpent { get; set; }
	public int KeysGained { get; set; }
		
	public int MaxKeysGainable { get; set; } = 8;	// Max # of Keys gainable in a fraction of a second (Sometimes the game client takes a moment to load the current key count)
							// T16 => 66% chance of getting 4 keys + Cain Set (25% for each key to drop an additional one)
	
	public TopLabelDecorator Label { get; set; }
	public IFont KeyFont { get; set; }
	public IBrush BgBrush { get; set; }
	public IBrush BgBrushAlt { get; set; }

        public int Priority { get; set; } = 3;
        public int Hook { get; set; } = 0;
		
        public LiveStats_KeystoneHelper()
	{
	    Enabled = true;
	}
		
	public void Customize()
	{
	    // Add this display to the LiveStats readout with a(n optional) specified positional order priority of 2
	    Hud.RunOnPlugin<LiveStatsPlugin>(plugin => {
		plugin.Add(this.Label, this.Priority, this.Hook);
	    });
	}

        public override void Load(IController hud)
        {
            base.Load(hud);
			
	    KeyFont = Hud.Render.CreateFont("tahoma", 7, 255, 198, 86, 255, false, false, true); //205, 102, 255
			
	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
	    BgBrush = plugin.BgBrush; 
	    BgBrushAlt = plugin.BgBrushAlt;
			
	    int delta = this.KeysGained - this.KeysSpent;
	    Label = new TopLabelDecorator(Hud)
	    {
                TextFont = KeyFont,
		//BackgroundBrush = BgBrush,
		TextFunc = () => {
			double diff = (double)(this.KeysGained - this.KeysSpent) / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d);
			return (diff > 0 ? "+" + diff.ToString("F0") : diff.ToString("F0")) + " keys/h";
			},
                //HintFunc = () => "Greater Rift Keystones Change Rate",
		ExpandUpLabels = new List<TopLabelDecorator>() {
		    new TopLabelDecorator(Hud) {
			TextFont = KeyFont,
			BackgroundBrush = BgBrush,
			ExpandedHintFont = KeyFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => "+" + (this.KeysGained / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F0") + " keys/h", //ValueToString(Hud.Game.Me.Defense.EhpCur, ValueFormat.ShortNumber),
			HintFunc = () => "Gain Rate",
		    },
		    new TopLabelDecorator(Hud) {
			TextFont = KeyFont,
			BackgroundBrush = BgBrushAlt,
			ExpandedHintFont = KeyFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => "-" + (this.KeysSpent / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F0") + " keys/h",
			HintFunc = () => "Spend Rate",
		    },
		    new TopLabelDecorator(Hud) {
			TextFont = KeyFont,
			BackgroundBrush = BgBrush,
			ExpandedHintFont = KeyFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => this.KeysGained.ToString(),
			HintFunc = () => "Keys Gained",
		    },
		    new TopLabelDecorator(Hud) {
			TextFont = KeyFont,
			BackgroundBrush = BgBrushAlt,
			ExpandedHintFont = KeyFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => this.KeysSpent.ToString(),
			HintFunc = () => "Keys Spent",
		    },
		    new TopLabelDecorator(Hud) {
			TextFont = KeyFont,
			BackgroundBrush = BgBrush,
			ExpandedHintFont = KeyFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => Hud.Game.Me.Materials.GreaterRiftKeystone.ToString(),
			HintFunc = () => "Current Total",
		    },
		},
            };
			
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame) return;
			
			int seen = (int)Hud.Game.Me.Materials.GreaterRiftKeystone;
			if (KeysAtStart == -1) {
				KeysAtStart = seen;
				KeysLastSeen = seen;
			}
			
			if (KeysLastSeen != seen) {
				int delta = seen - KeysLastSeen;
				
				if (Math.Abs(delta) > MaxKeysGainable) // Gaining more than MaxKeysGainable in a fraction of a second? Change KeysAtStart instead
				{					
					KeysAtStart = seen;
					KeysSpent = 0;
					KeysGained = 0;
				}
				else if (delta < 0)
					KeysSpent += (int)Math.Abs(delta);
				else
					KeysGained += delta;
				
				KeysLastSeen = seen;
			}
		}
    }
}