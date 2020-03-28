// RunStats XP Helper by Razor

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

    public class LiveStats_XPHelper : BasePlugin, ICustomizer //, IInGameTopPainter
    {
	public TopLabelDecorator Label { get; set; }
	public IFont TextFont { get; set; }
	public IBrush BgBrush { get; set; }
	public IBrush BgBrushAlt { get; set; }

        public int Priority { get; set; } = 7;
        public int Hook { get; set; } = 0;

        public LiveStats_XPHelper()
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
			
	    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 25, 225, 255, false, false, true);
			
	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
	    BgBrush = plugin.BgBrush;
	    BgBrushAlt = plugin.BgBrushAlt;
			
	    Label = new TopLabelDecorator(Hud)
            {
                TextFont = TextFont,
                TextFunc = () => ValueToString(Hud.Tracker.Session.GainedExperiencePerHourPlay, ValueFormat.ShortNumber) + " xp/h",
                HintFunc = () => "Exp per Hour",
				ExpandUpLabels = new List<TopLabelDecorator>()
                {
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.Session.GainedExperiencePerHourFull, ValueFormat.ShortNumber) + " xp/h",
                        HintFunc = () => "+ Town Time",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = TextFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.SessionAlwaysRunning.GainedExperiencePerHourFull, ValueFormat.ShortNumber) + " xp/h",
                        HintFunc = () => "+ Town + Menus",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.Session.GainedExperience, ValueFormat.ShortNumber) + " xp",
                        HintFunc = () => "Total (Session)",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = TextFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.CurrentAccountToday.GainedExperience, ValueFormat.ShortNumber) + " xp",
                        HintFunc = () => "Total (Today)",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont, //expandedHintFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => (Hud.Game.Me.CurrentLevelNormal >= Hud.Game.Me.CurrentLevelNormalCap && Hud.Game.Me.ParagonExpToNextLevel > 0 ?
					((float)Hud.Game.Me.BonusPoolRemaining / Hud.Game.Me.ParagonExpToNextLevel).ToString("0.00%", CultureInfo.InvariantCulture) :
					"N/A" //haven't figured out how to access the pre paragon xp tables yet
					),
                        HintFunc = () => "Bonus Pools",
                    },
		}
            };			
	}

    }
}