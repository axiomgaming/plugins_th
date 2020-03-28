using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.DAV
{
	public class DAV_SaderValorRGK : BasePlugin, IInGameWorldPainter {
		public bool showOnChar { get; set; }
		public float xPos { get; set; }
		public float yPos { get; set; }
		public IFont Font_Norm { get; set; }
		public IFont Font_Good { get; set; }
		public IFont Font_Warm { get; set; }

		public float barH { get; set; }
		public float barW { get; set; }
		public float Energy_Bad { get; set; }
		public string Energy_Message { get; set; }
		public IBrush Brush_BG { get; set; }
		public IBrush Brush_EnergyBad { get; set; }
		public IBrush Brush_EnergyGood { get; set; }

		public int FistofHeaven_Min { get; set; }
		public float iconSize { get; set; }
		public WorldDecoratorCollection FistofHeaven_Decorator { get; set; }

		private float xref { get; set; }
		private float yref { get; set; }
		private ITexture Icon_FistHeaven { get; set; }
		private ITexture Icon_HeavenFury { get; set; }
		private ITexture Icon_COE { get; set; }
		private IPlayer ValorSader { get; set; }

		public DAV_SaderValorRGK() {
			Enabled = true;
		}

		public override void Load(IController hud) {
			base.Load(hud);

			Energy_Bad = 0.4f;
			Energy_Message = "Fury : ";
			FistofHeaven_Min = 4;

			showOnChar = true;
			xPos = 60; // Hud.Window.Size.Width * 810 / 1920;
			yPos = -20;  // Hud.Window.Size.Height * 160 / 1080;
			barH = Hud.Window.Size.Height * 14 / 1080;
			barW = Hud.Window.Size.Width * 120 / 1920;
			iconSize = Hud.Window.Size.Height * 40 / 1080;

			Font_Norm = Hud.Render.CreateFont("arial", 7, 255, 255, 255, 255, false, false, true);
			Font_Good = Hud.Render.CreateFont("arial", 9, 255, 51, 255, 51, true, false, 255, 0, 0, 0, true);
			Font_Warm = Hud.Render.CreateFont("arial", 9, 255, 255, 51, 51, true, false, 255, 0, 0, 0, true);

			Brush_EnergyGood = Hud.Render.CreateBrush(240, 51, 255, 51, 0);
			Brush_EnergyBad = Hud.Render.CreateBrush(240, 255, 51, 51, 0);
			Brush_BG = Hud.Render.CreateBrush(240, 204, 204, 204, 0);

			FistofHeaven_Decorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(102, 204, 204, 51, 0),
					Radius = 5,
				}
			);

			Icon_FistHeaven = Hud.Texture.GetTexture(3476188190);
			Icon_HeavenFury = Hud.Texture.GetTexture(2206308930);
			Icon_COE = Hud.Texture.GetTexture(2639019912);
		}

		public void PaintWorld(WorldLayer layer) {
			if (Hud.Game.SpecialArea != SpecialArea.GreaterRift) return;

			if (Hud.Game.Me.Powers.BuffIsActive(483639, 0))
				ValorSader = Hud.Game.Me;
			else {
				if (Hud.Game.RiftPercentage < 100) return;
				var players = Hud.Game.Players.Where(x => x.HeroClassDefinition.HeroClass == HeroClass.Crusader && x.Powers.BuffIsActive(483639, 0));
				if (players.Count() == 1)
					ValorSader = players.FirstOrDefault();
				else return;
			}

			var FistofHeaven = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._x1_crusader_fistofheavens_teslacoil);
			foreach (var actor in FistofHeaven)
				FistofHeaven_Decorator.Paint(layer, null, actor.FloorCoordinate, null);

			if (showOnChar) {
				var screenCoord = ValorSader.FloorCoordinate.ToScreenCoordinate(true);
				xref = screenCoord.X + xPos;
				yref = screenCoord.Y + yPos;
			}
			else {
				xref = xPos;
				yref = yPos;
			}

			// Number of Fist of Heaven Actor
			Icon_FistHeaven?.Draw(xref, yref, iconSize, iconSize);
			var stack = FistofHeaven.Count();
			var usedFont = stack < FistofHeaven_Min ? Font_Warm : Font_Good;
			var layout = usedFont.GetTextLayout(stack.ToString());
			usedFont.DrawText(layout, xref + (iconSize - layout.Metrics.Width) / 2f, yref + (iconSize - layout.Metrics.Height) / 2f);
			xref += iconSize;

			// Number of Valor Set 2 Stack
			var furyBuff = ValorSader.Powers.GetBuff(483643);
			Icon_HeavenFury?.Draw(xref, yref, iconSize, iconSize);
			stack = furyBuff.IconCounts[1];
			usedFont = stack < 3 ? Font_Warm : Font_Good;
			usedFont.DrawText(stack.ToString(), xref + 2, yref + 2);

			var timeLeft = furyBuff.TimeLeftSeconds[1];
			if (timeLeft > 0) {
				layout = Font_Warm.GetTextLayout(timeLeft.ToString(timeLeft < 1 ? "F1" : "F0"));
				Font_Warm.DrawText(layout, xref + iconSize - layout.Metrics.Width - 2, yref + iconSize - layout.Metrics.Height - 2);
			}
			xref += iconSize;

			// COE
			if (ValorSader.Powers.BuffIsActive(430674, 0)) {
				Icon_COE.Draw(xref, yref, iconSize, iconSize);
				var coeLeft = ValorSader.Powers.GetBuff(430674).TimeLeftSeconds[8];
				if (coeLeft >= 8) {
					usedFont = coeLeft > 12 ? Font_Warm : Font_Good;
					layout = usedFont.GetTextLayout((coeLeft - 8).ToString("F1"));
					usedFont.DrawText(layout, xref + (iconSize - layout.Metrics.Width) / 2f, yref + (iconSize - layout.Metrics.Height) / 2f);
				}
			}
			yref += iconSize;
			xref -= iconSize * 2;

			// Resource Status
			var EnergyPct = ValorSader.Stats.ResourcePctPri / 100f;
			var usedBrush = EnergyPct < Energy_Bad ? Brush_EnergyBad : Brush_EnergyGood;
			Brush_BG.DrawRectangle(xref, yref, barW, barH);
			usedBrush.DrawRectangle(xref, yref, barW * EnergyPct, barH);

			layout = Font_Norm.GetTextLayout(Energy_Message + EnergyPct.ToString("0%"));
			Font_Norm.DrawText(layout, xref + 2, yref + (barH - layout.Metrics.Height) / 2f);
		}
	}
}