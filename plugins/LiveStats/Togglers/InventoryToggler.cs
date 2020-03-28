// Custom BagFreeSpacePlugin by HaKache

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using SharpDX.DirectInput;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.LiveStats.Togglers
{
    public class InventoryToggler : BasePlugin, ICustomizer, IInGameTopPainter, IKeyEventHandler
    {
	// Bag Free Space
        public TopLabelDecorator RedDecorator { get; set; }
        public TopLabelDecorator YellowDecorator { get; set; }
        public TopLabelDecorator GreenDecorator { get; set; }
	// Blood Shards Counter
        public TopLabelDecorator RedShardDecorator { get; set; }
        public TopLabelDecorator YellowShardDecorator { get; set; }
        public TopLabelDecorator GreenShardDecorator { get; set; }
        public bool ShowRemaining { get; set; }

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

        public InventoryToggler() : base()
        {
            Enabled = true;
	    Order = -10001; // Load Order - Just before LiveStats Plugin
        }

        public void Customize()
        {
	// Disable Default InventoryFreeSpace Plugin
	Hud.GetPlugin<InventoryFreeSpacePlugin>().Enabled = false;
	// Disable Default BloodShard Plugin
	Hud.GetPlugin<BloodShardPlugin>().Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
            HotKey = plugin.HotKey;

            RedDecorator = new TopLabelDecorator(Hud)
	    {
		TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 100, 100, true, false, 255, 0, 0, 0, true),
		BackgroundTexture1 = Hud.Texture.ButtonTextureGray,
		BackgroundTexture2 = Hud.Texture.BackgroundTextureOrange,
		BackgroundTextureOpacity1 = 0.8f,
		BackgroundTextureOpacity2 = 0.8f,
		TextFunc = () => (Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed).ToString("D", CultureInfo.InvariantCulture),
		HintFunc = () => "Free Space in Inventory",
	    };
            YellowDecorator = new TopLabelDecorator(Hud)
	    {
		TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 200, 205, 50, true, false, 255, 0, 0, 0, true),
		BackgroundTexture1 = Hud.Texture.ButtonTextureGray,
		BackgroundTexture2 = Hud.Texture.BackgroundTextureOrange,
		BackgroundTextureOpacity1 = 0.8f,
		BackgroundTextureOpacity2 = 0.8f,
		TextFunc = () => (Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed).ToString("D", CultureInfo.InvariantCulture),
		HintFunc = () => "Free Space in Inventory",
	    };
            GreenDecorator = new TopLabelDecorator(Hud)
	    {
		TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 100, 130, 100, true, false, 255, 0, 0, 0, true),
		BackgroundTexture1 = Hud.Texture.ButtonTextureGray,
		BackgroundTexture2 = Hud.Texture.BackgroundTextureOrange,
		BackgroundTextureOpacity1 = 0.8f,
		BackgroundTextureOpacity2 = 0.8f,
		TextFunc = () => (Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed).ToString("D", CultureInfo.InvariantCulture),
		HintFunc = () => "Free Space in Inventory",
	    };

	    RedShardDecorator = new TopLabelDecorator(Hud)
	    {
		TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 100, 100, true, false, 255, 0, 0, 0, true),
		BackgroundTexture1 = Hud.Texture.BackgroundTextureOrange,
		BackgroundTexture2 = Hud.Texture.ButtonTextureGray,
		BackgroundTextureOpacity1 = 0.8f,
		BackgroundTextureOpacity2 = 0.8f,
		TextFunc = () => Hud.Game.Me.Materials.BloodShard.ToString("D", CultureInfo.InvariantCulture),
		HintFunc = () => "Amount of Blood Shards"
	    };
	    YellowShardDecorator = new TopLabelDecorator(Hud)
	    {
		TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 200, 205, 50, true, false, 255, 0, 0, 0, true),
		BackgroundTexture1 = Hud.Texture.BackgroundTextureOrange,
		BackgroundTexture2 = Hud.Texture.ButtonTextureGray,
		BackgroundTextureOpacity1 = 0.8f,
		BackgroundTextureOpacity2 = 0.8f,
		TextFunc = () => Hud.Game.Me.Materials.BloodShard.ToString("D", CultureInfo.InvariantCulture),
		HintFunc = () => "Amount of Blood Shards"
	    };
	    GreenShardDecorator = new TopLabelDecorator(Hud)
	    {
		TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 100, 130, 100, true, false, 255, 0, 0, 0, true),
		BackgroundTexture1 = Hud.Texture.BackgroundTextureOrange,
		BackgroundTexture2 = Hud.Texture.ButtonTextureGray,
		BackgroundTextureOpacity1 = 0.8f,
		BackgroundTextureOpacity2 = 0.8f,
		TextFunc = () => Hud.Game.Me.Materials.BloodShard.ToString("D", CultureInfo.InvariantCulture),
		HintFunc = () => "Amount of Blood Shards"
	    };

        }

        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (keyEvent.IsPressed && ToggleKeyEvent.Matches(keyEvent))
            {
                Show = !Show;
            }
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (Hud.Render.UiHidden) return;
            if (clipState != ClipState.BeforeClip) return;
            if ((Hud.Game.MapMode == MapMode.WaypointMap) || (Hud.Game.MapMode == MapMode.ActMap) || (Hud.Game.MapMode == MapMode.Map)) return;
            if (!IsEnabled) return;

	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
	    if (!plugin.Show) Show = false;
		else Show = true;
	    if (Show) return;

            var uiRect = Hud.Render.InGameBottomHudUiElement.Rectangle;

            var freeSpace = Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed;
            var BagDecorator = freeSpace < 6 ? RedDecorator : freeSpace < 30 ? YellowDecorator : GreenDecorator;
            BagDecorator.Paint(uiRect.Left + (uiRect.Width * 0.645f), uiRect.Top + (uiRect.Height * 0.88f), uiRect.Width * 0.019f, uiRect.Height * 0.12f, HorizontalAlign.Center);

	    var maxshards = 500 + (Hud.Game.Me.HighestSoloRiftLevel * 10);
            var fillpercent = (((double)Hud.Game.Me.Materials.BloodShard / maxshards) * 100);
         // var remaining = maxshards - Hud.Game.Me.Materials.BloodShard;

            var ShardDecorator = fillpercent > 67 ? RedShardDecorator : (fillpercent > 50 ? YellowShardDecorator : GreenShardDecorator);
         // var ShardDecorator = remaining < 500 ? RedShardDecorator : (remaining < 850 ? YellowShardDecorator : GreenShardDecorator);
            ShardDecorator.Paint(uiRect.Left + uiRect.Width * 0.664f, uiRect.Top + uiRect.Height * 0.88f, uiRect.Width * 0.038f, uiRect.Height * 0.12f, HorizontalAlign.Center);
        }

    }
}