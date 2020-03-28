// Custom TopExperienceStatistics by HaKache

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using SharpDX.DirectInput;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.LiveStats.Togglers
{
    public class NextParagonToggler : BasePlugin, IInGameTopPainter, ICustomizer, IKeyEventHandler
    {
        public HorizontalTopLabelList LabelList { get; private set; }
	public IBrush BgBrush { get; set; }
	public IBrush BgBrushAlt { get; set; }
	public IBrush BorderBrush { get; set; }
	private IUiElement Anchor;

	// Key Toggle
        public bool Show { get; set; }
        public IKeyEvent ToggleKeyEvent { get; set; }
        public Key HotKey
        {
            get { return ToggleKeyEvent.Key; }
            set { ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, value, false, false, false); }
        }

	// Special Toggle to keep Customize Method
        public bool IsEnabled { get; set; } = true;

        public NextParagonToggler() : base()
        {
            Enabled = true;
	    Order = -10001; // Load Order - Just before LiveStats Plugin
        }

	public void Customize()
	{
	    Hud.GetPlugin<TopExperienceStatistics>().Enabled = false;
	}

        public override void Load(IController hud)
        {
            base.Load(hud);

	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
            HotKey = plugin.HotKey;

	    Anchor = Hud.Render.GetPlayerSkillUiElement(ActionKey.Heal);

            var expandedHintFont = Hud.Render.CreateFont("tahoma", 6, 255, 200, 200, 200, false, false, true);
	    BgBrush = Hud.Render.CreateBrush(100, 0, 0, 0, 0);
	    BgBrushAlt = Hud.Render.CreateBrush(160, 0, 0, 0, 0);
	    BorderBrush = Hud.Render.CreateBrush(104, 64, 180, 255, 1); // 35, 190, 190, 190

            LabelList = new HorizontalTopLabelList(hud)
            {
                LeftFunc = () => (Hud.Window.Size.Width / 2) - (Hud.Window.Size.Height * 0.08f),
                TopFunc = () => Hud.Window.Size.Height * 0.00f, // 0.001f Default
                WidthFunc = () => Hud.Window.Size.Height * 0.08f,
                HeightFunc = () => (float)Math.Ceiling(Hud.Window.Size.Height - Anchor.Rectangle.Bottom) - 1f,
            };

            var currentLevelDecorator = new TopLabelDecorator(Hud)
            {
                TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 57, 137, 205, true, false, true),
		BackgroundBrush = BgBrush,
                TextFunc = () => (Hud.Game.Me.CurrentLevelNormal < Hud.Game.Me.CurrentLevelNormalCap) ? Hud.Game.Me.CurrentLevelNormal.ToString("0") : "p" + Hud.Game.Me.CurrentLevelParagonDouble.ToString("0.##", CultureInfo.InvariantCulture),
                ExpandDownLabels = new List<TopLabelDecorator>(),
            };

            foreach (var levelIncrement in new uint[] { 1, 2, 5, 10, 20, 50, 100, 250, 500, 1000 })
            {
                currentLevelDecorator.ExpandDownLabels.Add(
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = Hud.Render.CreateFont("tahoma", 6, 180, 255, 255, 255, true, false, true),
                        ExpandedHintFont = expandedHintFont,
                        ExpandedHintWidthMultiplier = 2f,
			BackgroundBrush = BgBrushAlt,
                        HideBackgroundWhenTextIsEmpty = true,
                        TextFunc = () => Hud.Game.Me.CurrentLevelNormal >= Hud.Game.Me.CurrentLevelNormalCap ? ("p" + (Hud.Game.Me.CurrentLevelParagon + levelIncrement).ToString("D", CultureInfo.InvariantCulture)) : null,
                        HintFunc = () => ExpToParagonLevel(Hud.Game.Me.CurrentLevelParagon + levelIncrement) + " = " + TimeToParagonLevel(Hud.Game.Me.CurrentLevelParagon + levelIncrement, false),
                    });
            }

            LabelList.LabelDecorators.Add(currentLevelDecorator);

            LabelList.LabelDecorators.Add(new TopLabelDecorator(Hud)
            {
                TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 57, 137, 205, true, false, true),
		BackgroundBrush = BgBrush,
                TextFunc = () => ValueToString(Hud.Game.CurrentHeroToday.GainedExperiencePerHourFull, ValueFormat.ShortNumber) + "/h",
            });
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;
            if (!IsEnabled) return;

	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
	    if (!plugin.Show) Show = false;
		else Show = true;
	    if (Show) return;

	    BorderBrush.DrawLine((Hud.Window.Size.Width / 2) - (Hud.Window.Size.Height * 0.08f), (float)Math.Ceiling(Hud.Window.Size.Height - Anchor.Rectangle.Bottom) - 1f, 
			(Hud.Window.Size.Width / 2) + (Hud.Window.Size.Height * 0.079f), (float)Math.Ceiling(Hud.Window.Size.Height - Anchor.Rectangle.Bottom) - 1f);
            LabelList.Paint();
        }

        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (keyEvent.IsPressed && ToggleKeyEvent.Matches(keyEvent))
            {
                Show = !Show;
            }
        }

        public string ExpToParagonLevel(uint paragonLevel)
        {
            if (paragonLevel > Hud.Game.Me.CurrentLevelParagon)
            {
                var xpRequired = Hud.Sno.TotalParagonExperienceRequired(paragonLevel);
                var xpRemaining = xpRequired - Hud.Game.Me.ParagonTotalExp;
                return ValueToString(xpRemaining, ValueFormat.LongNumber);
            }

            return null;
        }

        public string TimeToParagonLevel(uint paragonLevel, bool includetext)
        {
            var tracker = Hud.Game.CurrentHeroToday;
            if (tracker != null)
            {
                if (paragonLevel > Hud.Game.Me.CurrentLevelParagon)
                {
                    var text = includetext ? "p" + paragonLevel.ToString("D", CultureInfo.InvariantCulture) + ": " : "";
                    var xph = tracker.GainedExperiencePerHourFull;
                    if (xph > 0)
                    {
                        var xpRequired = Hud.Sno.TotalParagonExperienceRequired(paragonLevel);
                        var xpRemaining = xpRequired - Hud.Game.Me.ParagonTotalExp;
                        var hours = xpRemaining / xph;
                        var ticks = Convert.ToInt64(Math.Ceiling(hours * 60.0d * 60.0d * 1000.0d * TimeSpan.TicksPerMillisecond));
                        text += ValueToString(ticks, ValueFormat.LongTimeNoSeconds);
                    }
                    else
                    {
                        text += "-";
                    }

                    return text;
                }
            }

            return null;
        }
    }
}