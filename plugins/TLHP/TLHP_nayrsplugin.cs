using System;
using SharpDX;
using System.Globalization;
using Turbo.Plugins.Default;
using System.Linq;
using SharpDX.DirectInput;
using System.Text;
using System.Collections.Generic;

namespace Turbo.Plugins.TLHP
{
	public class TLHP_NayrsPlugin : BasePlugin, IInGameTopPainter {
		public IFont TimeFont { get; set; }
		public IBrush GreenBrush { get; set; }
		public IBrush RedBrush { get; set; }
		public IBrush WhiteBrush { get; set; }
		public double Attacktime { get; set; } = 5d;
		public float BrushHigh { get; set; } = 15f;
		
		public TLHP_NayrsPlugin() {
			Enabled = true;
		}

		public override void Load(IController hud) {
			base.Load(hud);

			TimeFont = Hud.Render.CreateFont("arial", 7, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true);
			GreenBrush = Hud.Render.CreateBrush(240, 128, 255, 0, 0);
			RedBrush = Hud.Render.CreateBrush(240, 255, 51, 51, 0);
			WhiteBrush = Hud.Render.CreateBrush(240, 255, 255, 255, 0);
		}

		public void PaintTopInGame(ClipState clipState) {
			if (clipState != ClipState.BeforeClip) return;
			if (Hud.Game.Me.HeroClassDefinition.HeroClass != HeroClass.Necromancer) return;

			var Nayrs = Hud.Game.Me.Powers.GetBuff(476587);
			if (Nayrs == null || !Nayrs.Active) return;
			
			var uiSkill1 = Hud.Render.GetPlayerSkillUiElement(ActionKey.Skill1);
			foreach (var skill in Hud.Game.Me.Powers.CurrentSkills) {
				if (skill.ElementalType != 4) continue;
				
				var ui = Hud.Render.GetPlayerSkillUiElement(skill.Key);
				var x = (float)Math.Round(ui.Rectangle.X) + 0.5f;
				var y = ((float)Math.Round(uiSkill1.Rectangle.Y) + (float)Math.Round(ui.Rectangle.Height) + 0.5f);
				var w = (float)Math.Round(ui.Rectangle.Width);
				//WhiteBrush.DrawRectangle(x, y, w, BrushHigh);
				
				var t = Nayrs.TimeLeftSeconds[(int)skill.Key + 1];
				if (t <= 0) continue;
				
				var w1 = w * (float)t / 15f;
				var timeBrush = t > Attacktime ? GreenBrush : RedBrush;
				
				timeBrush.DrawRectangle(x, y, w1, BrushHigh);
				var layout = TimeFont.GetTextLayout(t.ToString(t > Attacktime ? "F0" : "F1", CultureInfo.InvariantCulture));
				TimeFont.DrawText(layout, x + 18, y + (BrushHigh - layout.Metrics.Height)/2);
			}
		}
	}
}