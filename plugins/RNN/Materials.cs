using Turbo.Plugins.Default;
using System.Globalization;

namespace Turbo.Plugins.RNN
{
    public class Materials : BasePlugin, IInGameTopPainter, ICustomizer
    {
		private float x { get; set; }
		private float y { get; set; }
		private float Space { get; set; }
		private float sizeIcon { get; set; }

		private long[] ListMats { get; set; }
		private IFont[] ListFont { get; set; }
		private ITexture[] ListTexture { get; set; }
		public IBrush BorderBrushRed { get; set; }

		public float Xpor { get; set; }
		public float Ypor { get; set; }
		public float SizeMultiplier { get; set; }
		public float Separation { get; set; }
		public bool ColorText  { get; set; }
		public bool WarningBS { get; set; }
		public long RemainingBS { get; set; }
		public bool InventorySpace  { get; set; }
		public bool OnlyInTown  { get; set; }

		public Materials()
		{
			Enabled = true;
		}

		public override void Load(IController hud)
		{
			base.Load(hud);
			Order = 30001;

			Xpor = 0.100f; //0.567f;
			Ypor = 0.001f; //0.984f;
			SizeMultiplier = 1.05f; //1.0f;
			Separation	= 0.5f;
			ColorText = true;
			WarningBS = true;
			RemainingBS = 250;
			InventorySpace = true;
			OnlyInTown = false;

			BorderBrushRed = Hud.Render.CreateBrush(255, 255, 0, 0, 1);
		}

        public void Customize()
		{
			ListMats = new long[] {	0,0,0,0,0,0,0,0,0 };

			ListFont = new IFont[]
			{
				Hud.Render.CreateFont("tahoma", 7 * SizeMultiplier, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true),
				Hud.Render.CreateFont("tahoma", 7 * SizeMultiplier, 255, 175, 180, 175, false, false, 255, 0, 0, 0, true),
				Hud.Render.CreateFont("tahoma", 7 * SizeMultiplier, 255, 102, 102, 255, false, false, 255, 0, 0, 0, true),
				Hud.Render.CreateFont("tahoma", 7 * SizeMultiplier, 255, 255, 255,   0, false, false, 255, 0, 0, 0, true),
				Hud.Render.CreateFont("tahoma", 7 * SizeMultiplier, 255, 108, 216, 187, false, false, 255, 0, 0, 0, true),
				Hud.Render.CreateFont("tahoma", 7 * SizeMultiplier, 255, 255, 128,   0, false, false, 255, 0, 0, 0, true),
				Hud.Render.CreateFont("tahoma", 7 * SizeMultiplier, 255, 220, 135, 220, false, false, 255, 0, 0, 0, true),
				Hud.Render.CreateFont("tahoma", 7 * SizeMultiplier, 255, 245,  75,  75, false, false, 255, 0, 0, 0, true),
				Hud.Render.CreateFont("tahoma", 7 * SizeMultiplier, 255, 200, 120,  70, false, false, 255, 0, 0, 0, true)
			};

			ListTexture = new ITexture[]
			{
				Hud.Texture.GetTexture("MarkerExclamation"),
				Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Crafting_AssortedParts_01),
				Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Crafting_Magic_01),
				Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Crafting_Rare_01),
				Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Crafting_Looted_Reagent_01),
				Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Crafting_Legendary_01),
				Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.GreaterLootRunKey),
				Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.HoradricRelic),
				Hud.Texture.Button2TextureBrown
			};

			Space = ListFont[1].GetTextLayout("0").Metrics.Height * Separation;

			sizeIcon = ListTexture[1].Width * 0.26f * SizeMultiplier;

			if ((Xpor > 0.5f) && (Xpor < 0.65f) && (Ypor > 0.97f))
			{
				Hud.TogglePlugin<BloodShardPlugin>(false);
				Hud.TogglePlugin<InventoryFreeSpacePlugin>(false);
			}
		}

        public string MatsToString(long value)
        {
            if 		(value < 1000)		{	return value.ToString("#,0.#", CultureInfo.InvariantCulture);					}
			else if (value < 1000000)	{	return (value / 1000.0f).ToString("#,0.#K", CultureInfo.InvariantCulture);		}
			else 						{	return (value / 1000000.0f).ToString("#,0.#M", CultureInfo.InvariantCulture);	}
        }

		public void PaintTopInGame(ClipState clipState)
		{
			if (clipState != ClipState.BeforeClip) return;
			if (!Hud.Game.IsInGame)  return;
			if (OnlyInTown && !Hud.Game.IsInTown) return;

			x = Hud.Window.Size.Width * Xpor ;	y = Hud.Window.Size.Height * Ypor ;

			ListMats[1] = Hud.Game.Me.Materials.ReusableParts;	ListMats[2] = Hud.Game.Me.Materials.ArcaneDust;		ListMats[3] = Hud.Game.Me.Materials.VeiledCrystal;
			ListMats[4] = Hud.Game.Me.Materials.DeathsBreath;	ListMats[5] = Hud.Game.Me.Materials.ForgottenSoul;	ListMats[6] = Hud.Game.Me.Materials.GreaterRiftKeystone;
			ListMats[7] = Hud.Game.Me.Materials.BloodShard;		ListMats[8] = Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed;

			for(var i = 1; i < 9; i++)
			{
				if ((i == 8) && !InventorySpace) continue;
				ListTexture[i].Draw(x, y, sizeIcon, sizeIcon);
				if (i != 8) x += sizeIcon;
				var layout = ListFont[i].GetTextLayout( MatsToString(ListMats[i]) );
				ListFont[ColorText?i:0].DrawText(layout, x, y);
				if (i == 7)
				{
					if (WarningBS && ((500 + (Hud.Game.Me.HighestSoloRiftLevel * 10) - Hud.Game.Me.Materials.BloodShard) < RemainingBS))
					{
						BorderBrushRed.DrawRectangle(x - 1, y +1, layout.Metrics.Width + 2, layout.Metrics.Height - 2);
					}
				}
				x += layout.Metrics.Width + Space;
			}
		}
	}
}
