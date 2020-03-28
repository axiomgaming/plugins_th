// Custom NetworkLatencyPlugin by HaKache

using System.Globalization;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.LiveStats.Togglers
{
    public class LatencyPlugin : BasePlugin, ICustomizer, IInGameTopPainter
    {
	public TopLabelDecorator AverageDecoratorNormal { get; set; }
	public TopLabelDecorator CurrentDecoratorNormal { get; set; }
	public TopLabelDecorator AverageDecoratorMedium { get; set; }
	public TopLabelDecorator CurrentDecoratorMedium { get; set; }
	public TopLabelDecorator AverageDecoratorHigh { get; set; }
	public TopLabelDecorator CurrentDecoratorHigh { get; set; }
	public int MediumLimit { get; set; } = 80;
	public int HighLimit { get; set; } = 150;

	// Special Toggle to keep Customize Method
        public bool IsEnabled { get; set; } = true;

	public LatencyPlugin()
	{
            Enabled = false;
	    Order = -10001; // Load Order - Just before LiveStats Plugin
	}
		
	public void Customize()
	{
	    // Disable Default LatencyPlugin
	    Hud.TogglePlugin<NetworkLatencyPlugin>(false);
	}
		
        public override void Load(IController hud)
        {
            base.Load(hud);

            AverageDecoratorNormal = new TopLabelDecorator(Hud)
            {
                TextFont = Hud.Render.CreateFont("tahoma", 7, 200, 150, 200, 150, true, false, 120, 0, 0, 0, true),
                TextFunc = () => Hud.Game.AverageLatency.ToString("F0", CultureInfo.InvariantCulture),
                HintFunc = () => "Average Latency"
            };
            CurrentDecoratorNormal = new TopLabelDecorator(Hud)
            {
                TextFont = Hud.Render.CreateFont("tahoma", 7, 200, 150, 200, 150, true, false, 120, 0, 0, 0, true),
                TextFunc = () => Hud.Game.CurrentLatency.ToString("F0", CultureInfo.InvariantCulture),
                HintFunc = () => "Current Latency"
            };

            AverageDecoratorMedium = new TopLabelDecorator(Hud)
            {
                TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 200, 175, 150, true, false, 120, 0, 0, 0, true),
                TextFunc = () => Hud.Game.AverageLatency.ToString("F0", CultureInfo.InvariantCulture),
                HintFunc = () => "Average Latency"
            };
            CurrentDecoratorMedium = new TopLabelDecorator(Hud)
            {
                TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 200, 175, 150, true, false, 120, 0, 0, 0, true),
                TextFunc = () => Hud.Game.CurrentLatency.ToString("F0", CultureInfo.InvariantCulture),
                HintFunc = () => "Current Latency"
            };

            AverageDecoratorHigh = new TopLabelDecorator(Hud)
            {
                TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 90, 90, true, false, 120, 0, 0, 0, true),
                TextFunc = () => Hud.Game.AverageLatency.ToString("F0", CultureInfo.InvariantCulture),
                HintFunc = () => "Average Latency"
            };
            CurrentDecoratorHigh = new TopLabelDecorator(Hud)
            {
                TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 90, 90, true, false, 120, 0, 0, 0, true),
                TextFunc = () => Hud.Game.CurrentLatency.ToString("F0", CultureInfo.InvariantCulture),
                HintFunc = () => "Current Latency"
            };
		}

        public void PaintTopInGame(ClipState clipState)
        {
            if (Hud.Render.UiHidden) return;
            if (clipState != ClipState.BeforeClip) return;
            if (!IsEnabled) return;

            var uiRect = Hud.Render.GetUiElement("Root.NormalLayer.game_dialog_backgroundScreenPC.latency_meter").Rectangle;

            var avg = Hud.Game.AverageLatency;
            var cur = Hud.Game.CurrentLatency;

            (avg >= HighLimit ? AverageDecoratorHigh : (avg >= MediumLimit ? AverageDecoratorMedium : AverageDecoratorNormal)).Paint(uiRect.Left + uiRect.Width * 1.6f, uiRect.Top + uiRect.Height * 0.54f, uiRect.Width, uiRect.Height * 0.15f, HorizontalAlign.Left);

            (cur >= HighLimit ? CurrentDecoratorHigh : (cur >= MediumLimit ? CurrentDecoratorMedium : CurrentDecoratorNormal)).Paint(uiRect.Left + uiRect.Width * 1.6f, uiRect.Top + uiRect.Height * 0.74f, uiRect.Width, uiRect.Height * 0.15f, HorizontalAlign.Left);
        }

    }

}