using System;
using System.Linq;
using System.Collections.Generic;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.RNN
{
	public class SpiritBarrageIcon : BasePlugin, IInGameTopPainter, ICustomizer, INewAreaHandler
	{
		private Dictionary<uint,int> Phantasms { get;set; } = new Dictionary<uint,int>();
		private int MyIndex { get; set; } = -1;
		private IFont FontCounter { get; set; }
		private IFont FontExpl { get; set; }
		private IFont FontLimit { get; set; }
		private IFont FontDefault { get; set; } = null;

		private float SizeIconWidth  { get; set; }
		private float SizeIconHeight  { get; set; }
		public int duration { get; set; } = 10;
		public float warning { get; set; } = 2.0f;

		public float Xpor  { get; set; }
		public float Ypor  { get; set; }
		public float SizeMultiplier  { get; set; }
		public float Opacity { get; set; } = 1f;
		public bool OnlyGR { get; set; }
		public bool OnlyMe { get; set; }

		public SpiritBarrageIcon()
		{
			Enabled = true;
		}

		public override void Load(IController hud)
		{
			base.Load(hud);
			Order = 30002;

			Xpor = 0.46f;   		// Valid values: from 0 to 1 . To set the x coordinate of the icon
			Ypor = 0.43f;			// Valid values: from 0 to 1 . To set the y coordinate of the icon
			SizeMultiplier = 1f; 	// Size multiplier for icons
			Opacity	= 0.75f;		// 0..1 Textures
			OnlyGR = false;			// Show in GR only
			OnlyMe = false;			// Ignore phantasm created by others players
			warning	= 2.0f;			// 9.0f...0f Text will take the color yellow when it reaches this value
		}

		public void Customize()
		{
			FontCounter = Hud.Render.CreateFont("tahoma", 7f * SizeMultiplier, 255, 0, 255, 0, true, false, 160, 0, 0, 0, true);
			FontLimit = Hud.Render.CreateFont("tahoma", 7f * SizeMultiplier, 255, 255, 255, 0, true, false, 160, 0, 0, 0, true);
			FontExpl = Hud.Render.CreateFont("tahoma", 8f * SizeMultiplier, 255, 50, 150, 255, true, false, 160, 0, 0, 0, true);

			SizeIconWidth = Hud.Texture.BuffFrameTexture.Width  * 0.60f * SizeMultiplier;
			SizeIconHeight = Hud.Texture.BuffFrameTexture.Height * 0.68f * SizeMultiplier;

			if ((warning < 0f) || (warning > 9f)) { warning = 2; }
		}


		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame || (MyIndex != Hud.Game.Me.Index) )   // Fix partialment the newGame limitation
			{
				MyIndex = Hud.Game.Me.Index;
				Phantasms.Clear();
			}
 		}


		public void PaintTopInGame(ClipState clipState)
		{
			if (clipState != ClipState.BeforeClip) return;
			if (!Hud.Game.IsInGame) return;
			if (OnlyGR && !Hud.Game.Me.InGreaterRift) return;

			var players = Hud.Game.Players.Where( p => p.HasValidActor && (p.IsMe || !OnlyMe) && p.Powers.UsedSkills.Any(s => s.SnoPower.Sno == 108506 && (s.Rune == 2 || p.Powers.BuffIsActive(484270))) );
			if ( players.Any() )
			{
				var actors = Hud.Game.Actors.Where(a => (a.SnoActor.Sno == ActorSnoEnum._wd_spiritbarragerune_aoe_ghostmodel) && ((a.SummonerAcdDynamicId == Hud.Game.Me.SummonerId) || !OnlyMe));
				var x = Hud.Window.Size.Width * Xpor; var y = Hud.Window.Size.Height * Ypor;
				Hud.Texture.GetTexture(1117784160).Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
				if (actors.Any())
				{
					foreach(var a in actors)
					{
					  if (!Phantasms.ContainsKey(a.AnnId)) { Phantasms[a.AnnId] = a.CreatedAtInGameTick; }
					}
					actors = actors.OrderByDescending(a => Phantasms[a.AnnId]);
					Hud.Texture.BuffFrameTexture.Draw(x, y, SizeIconWidth , SizeIconHeight, Opacity);
					var c = 0;
					foreach (var actor in actors)
					{
						if (c++ == 3) break;
						duration  = (players.FirstOrDefault(p => (actor.SummonerAcdDynamicId == p.SummonerId) && p.Powers.BuffIsActive(484270)) != null) ? 10 : 5;
						var t = duration - (Hud.Game.CurrentGameTick - Phantasms[actor.AnnId]) /  60f;
						if (t <= 0)
						{
							var layout = FontExpl.GetTextLayout("🞴");
							FontExpl.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/2.0f), y +(layout.Metrics.Height * 0.53f * (c - 1)) );
						}
						else
						{
							FontDefault = (t > warning)? FontCounter:FontLimit;
							var layout = FontDefault.GetTextLayout( (t < 1)? String.Format("{0:N1}",t) : String.Format("{0:0}",(int) (t + 0.90)) ); // Redondeará a X si es menor  a X.10
							FontDefault.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/2.0f), y + (layout.Metrics.Height * 0.73f * (c - 1)) );
						}
					}
				}
				else
				{
					Hud.Texture.DebuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
				}
			}
		}
    }
}
