// PlayersArcPlugin by HaKache
// Draws Arcs under players to show life, shield and resource values.
// Thanks to S4000 and RNN for code and ideas.

using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.Extended.Players
{
	public class PlayersArcPlugin : BasePlugin, IInGameWorldPainter
	{

	public int Arc_Start { get; set; }
	public int Arc_End { get; set; }
	public float ArcSize { get; set; }

	public IBrush Brush_BG { get; set; }
	public IBrush Brush_ShieldBG { get; set; }
	public IBrush Brush_ResBG { get; set; }
	public IBrush Brush_Life { get; set; }
	public IBrush Brush_Shield { get; set; }

	public IBrush Brush_Mana { get; set; }
	public IBrush Brush_Fury { get; set; }
	public IBrush Brush_Arcane { get; set; }
	public IBrush Brush_Spirit { get; set; }
	public IBrush Brush_Wrath { get; set; }
	public IBrush Brush_Essence { get; set; }
	public IBrush Brush_Hatred { get; set; }
	public IBrush Brush_Disc { get; set; }

        public bool MeResource { get; set; }
        public bool MeLife { get; set; }
        public bool MeShield { get; set; }

        public bool PlayersResource { get; set; }
        public bool PlayersLife { get; set; }
        public bool PlayersShield { get; set; }

        public IFont HealthFont { get; set; }

	public PlayersArcPlugin()
	{
	    Enabled = true;

	    MeLife = true;
	    MeShield = true;
	    MeResource = true;

	    PlayersLife = true;
	    PlayersShield = true;
	    PlayersResource = false;
	}

	public void PaintWorld(WorldLayer layer)
	{
	    var players = Hud.Game.Players.Where(x => x.CoordinateKnown && Hud.Game.Me.SnoArea.Sno == x.SnoArea.Sno && x.HeadStone == null);
		foreach (var player in players)
		{
			Paint(layer, player, Arc_Start, Arc_End, ArcSize);
		}
	}

	public override void Load(IController hud)
	{
	    base.Load(hud);

	    Arc_Start = 0; // -45
	    Arc_End = 135; // 180
	    ArcSize = 3.3f;

	    Brush_BG = Hud.Render.CreateBrush(255, 32, 32, 32, 6, DashStyle.Solid, CapStyle.Round, CapStyle.Round);
	    Brush_ShieldBG = Hud.Render.CreateBrush(255, 20, 130, 60, 5, DashStyle.Solid, CapStyle.Round, CapStyle.Round);
	    Brush_Life = Hud.Render.CreateBrush(255, 255, 50, 50, 4, DashStyle.Solid, CapStyle.Round, CapStyle.Round);
	    Brush_Shield = Hud.Render.CreateBrush(255, 100, 200, 150, 3, DashStyle.Solid, CapStyle.Round, CapStyle.Round);

	    Brush_ResBG = Hud.Render.CreateBrush(128, 255, 255, 255, 6, DashStyle.Solid, CapStyle.Round, CapStyle.Round);
	    Brush_Mana = Hud.Render.CreateBrush(240, 0, 0, 240, 4, DashStyle.Solid, CapStyle.Round, CapStyle.Round);
	    Brush_Fury = Hud.Render.CreateBrush(200, 255, 170, 0, 4, DashStyle.Solid, CapStyle.Round, CapStyle.Round);
	    Brush_Arcane = Hud.Render.CreateBrush(200, 100, 0, 205, 4, DashStyle.Solid, CapStyle.Round, CapStyle.Round);
	    Brush_Spirit = Hud.Render.CreateBrush(200, 255, 255, 200, 4, DashStyle.Solid, CapStyle.Round, CapStyle.Round);
	    Brush_Wrath = Hud.Render.CreateBrush(200, 255, 255, 225, 4, DashStyle.Solid, CapStyle.Round, CapStyle.Round);
	    Brush_Essence = Hud.Render.CreateBrush(200, 100, 175, 175, 4, DashStyle.Solid, CapStyle.Round, CapStyle.Round);
	    Brush_Hatred = Hud.Render.CreateBrush(200, 180, 0, 0, 4, DashStyle.Solid, CapStyle.Round, CapStyle.Round);
	    Brush_Disc = Hud.Render.CreateBrush(200, 0, 0, 200, 4, DashStyle.Solid, CapStyle.Round, CapStyle.Round);

	    HealthFont = Hud.Render.CreateFont("tahoma", 7f, 225, 255, 120, 120, true, false, 160, 0, 0, 0, true);

	}

	public void Paint(WorldLayer layer, IPlayer player, int start, int end, float size)
	{
	    if (player.IsMe) {

		if (MeResource) {
		    if (player.HeroClassDefinition.HeroClass == HeroClass.DemonHunter)
		    {
			PaintBackgroundArc(player, -45, 40, 2.6f); PaintResArc(player, -45, 40, 2.6f); 		// Hatred
			PaintBackgroundArc(player, 50, end, 2.6f); PaintSecResArc(player, 50, end, 2.6f);	// Disc
		    }
		    else { PaintBackgroundArc(player, -45, end, 2.6f); PaintResArc(player, -45, end, 2.6f); }
		}

		if (MeLife) { PaintBackgroundArc(player, -45, end, 3.3f); PaintLifeArc(player, -45, end, 3.3f); }
		if (MeShield) PaintShieldArc(player, -47, end + 2, 3.6f);
	    }

	    else
	    {

		if (PlayersResource) {
		    if (player.HeroClassDefinition.HeroClass == HeroClass.DemonHunter)
		    {
			PaintBackgroundArc(player, start, 60, 2.6f); PaintResArc(player, start, 60, 2.6f); 	// Hatred
			PaintBackgroundArc(player, 70, end, 2.6f); PaintSecResArc(player, 70, end, 2.6f);	// Disc
		    }
		    else { PaintBackgroundArc(player, start, end, 2.6f); PaintResArc(player, start, end, 2.6f); }
		}

		if (PlayersLife) { 
		    if (Hud.Game.IsInTown && player.IsOnScreen)
		    {					
			var health = ValueToString(player.Defense.HealthMax, ValueFormat.LongNumber);
			HealthFont.DrawText(HealthFont.GetTextLayout(health), player.FloorCoordinate.ToScreenCoordinate().X - HealthFont.GetTextLayout(health).Metrics.Width * 2, player.FloorCoordinate.ToScreenCoordinate().Y + 5);
		    }
		    PaintBackgroundArc(player, start, end, 3.3f); PaintLifeArc(player, start, end, 3.3f); }
		if (PlayersShield) PaintShieldArc(player, start - 2, end + 2, 3.6f);


	    }

	}

	public void PaintBackgroundArc(IPlayer player, int Angle_Start, int Angle_End, float ArcSize)
	{
		if (!player.IsOnScreen) return;

		var worldCoord = player.FloorCoordinate;
		var lifeArc = (int)(player.Defense.HealthPct * (Angle_End - Angle_Start) / 100f) + Angle_Start;

		using (var pg1 = Hud.Render.CreateGeometry()) {
			using (var gs1 = pg1.Open()) {
				var mx = ArcSize * (float)Math.Cos(Angle_Start * Math.PI / 180f);
				var my = ArcSize * (float)Math.Sin(Angle_Start * Math.PI / 180f);

				var screenCoord = worldCoord.Offset(mx, my, 0).ToScreenCoordinate(true);
				var vector = new Vector2(screenCoord.X, screenCoord.Y);

				gs1.BeginFigure(vector, FigureBegin.Filled);

				for (int angle = Angle_Start + 1; angle <= Angle_End; angle++)
				{
					mx = ArcSize * (float)Math.Cos(angle * Math.PI / 180f);
					my = ArcSize * (float)Math.Sin(angle * Math.PI / 180f);
					screenCoord = worldCoord.Offset(mx, my, 0).ToScreenCoordinate(true);
					vector = new Vector2(screenCoord.X, screenCoord.Y);

					gs1.AddLine(vector);
				}

				gs1.EndFigure(FigureEnd.Open);
				gs1.Close();
			}

			Brush_BG.DrawGeometry(pg1);
		}
	}

	public void PaintLifeArc(IPlayer player, int Angle_Start, int Angle_End, float ArcSize)
	{
		if (!player.IsOnScreen) return;

		var worldCoord = player.FloorCoordinate;
		var lifeArc = (int)(player.Defense.HealthPct * (Angle_End - Angle_Start) / 100f) + Angle_Start;

		using (var pg2 = Hud.Render.CreateGeometry()) {
			using (var gs2 = pg2.Open()) {
				var mx = ArcSize * (float)Math.Cos(Angle_Start * Math.PI / 180f);
				var my = ArcSize * (float)Math.Sin(Angle_Start * Math.PI / 180f);

				var screenCoord = worldCoord.Offset(mx, my, 0).ToScreenCoordinate(true);
				var vector = new Vector2(screenCoord.X, screenCoord.Y);

				gs2.BeginFigure(vector, FigureBegin.Filled);

				for (int angle = Angle_Start + 1; angle <= Angle_End; angle++)
				{
					mx = ArcSize * (float)Math.Cos(angle * Math.PI / 180f);
					my = ArcSize * (float)Math.Sin(angle * Math.PI / 180f);

					screenCoord = worldCoord.Offset(mx, my, 0).ToScreenCoordinate(true);
					vector = new Vector2(screenCoord.X, screenCoord.Y);

					if (angle <= lifeArc) gs2.AddLine(vector);
				}

				gs2.EndFigure(FigureEnd.Open);
				gs2.Close();
			}

			Brush_Life.DrawGeometry(pg2);
		}
	}

	public void PaintShieldArc(IPlayer player, int Angle_Start, int Angle_End, float ArcSize)
	{
		if (!player.IsOnScreen) return;

		var worldCoord = player.FloorCoordinate;
		var lifeArc = (int)((player.Defense.CurShield / player.Defense.HealthMax * 100f) * (Angle_End - Angle_Start) / 100f) + Angle_Start;

		using (var pg3 = Hud.Render.CreateGeometry()) {
			using (var gs3 = pg3.Open()) {
				var mx = ArcSize * (float)Math.Cos(Angle_Start * Math.PI / 180f);
				var my = ArcSize * (float)Math.Sin(Angle_Start * Math.PI / 180f);

				var screenCoord = worldCoord.Offset(mx, my, 0).ToScreenCoordinate(true);
				var vector = new Vector2(screenCoord.X, screenCoord.Y);

				gs3.BeginFigure(vector, FigureBegin.Filled);

				for (int angle = Angle_Start + 1; angle <= Angle_End; angle++)
				{
					mx = ArcSize * (float)Math.Cos(angle * Math.PI / 180f);
					my = ArcSize * (float)Math.Sin(angle * Math.PI / 180f);

					screenCoord = worldCoord.Offset(mx, my, 0).ToScreenCoordinate(true);
					vector = new Vector2(screenCoord.X, screenCoord.Y);

					if (angle <= lifeArc) gs3.AddLine(vector);
				}

				gs3.EndFigure(FigureEnd.Open);
				gs3.Close();
			}

			Brush_ShieldBG.DrawGeometry(pg3); Brush_Shield.DrawGeometry(pg3);
		}
	}

	public void PaintResArc(IPlayer player, int Angle_Start, int Angle_End, float ArcSize)
	{
		if (!player.IsOnScreen) return;

		var worldCoord = player.FloorCoordinate;
		var resArc = (int)(player.Stats.ResourcePctPri * (Angle_End - Angle_Start) / 100f) + Angle_Start;

		using (var pg3 = Hud.Render.CreateGeometry()) {
			using (var gs3 = pg3.Open()) {
				var mx = ArcSize * (float)Math.Cos(Angle_Start * Math.PI / 180f);
				var my = ArcSize * (float)Math.Sin(Angle_Start * Math.PI / 180f);

				var screenCoord = worldCoord.Offset(mx, my, 0).ToScreenCoordinate(true);
				var vector = new Vector2(screenCoord.X, screenCoord.Y);

				gs3.BeginFigure(vector, FigureBegin.Filled);

				for (int angle = Angle_Start + 1; angle <= Angle_End; angle++)
				{
					mx = ArcSize * (float)Math.Cos(angle * Math.PI / 180f);
					my = ArcSize * (float)Math.Sin(angle * Math.PI / 180f);

					screenCoord = worldCoord.Offset(mx, my, 0).ToScreenCoordinate(true);
					vector = new Vector2(screenCoord.X, screenCoord.Y);

					if (angle <= resArc) gs3.AddLine(vector);
				}

				gs3.EndFigure(FigureEnd.Open);
				gs3.Close();
			}

			Brush_BG.DrawGeometry(pg3);

                    switch (player.HeroClassDefinition.HeroClass)
                    {
			case HeroClass.Wizard:
			    Brush_ResBG.DrawGeometry(pg3); Brush_Arcane.DrawGeometry(pg3);
                            break;
			case HeroClass.WitchDoctor:
			    Brush_ResBG.DrawGeometry(pg3); Brush_Mana.DrawGeometry(pg3);
                            break;
			case HeroClass.Barbarian:
			    Brush_Fury.DrawGeometry(pg3);
                            break;
			case HeroClass.Crusader:
			    Brush_Wrath.DrawGeometry(pg3);
                            break;
			case HeroClass.Monk:
			    Brush_Spirit.DrawGeometry(pg3);
                            break;
			case HeroClass.Necromancer:
			    Brush_Essence.DrawGeometry(pg3);
                            break;
			case HeroClass.DemonHunter:
			    Brush_ResBG.DrawGeometry(pg3); Brush_Hatred.DrawGeometry(pg3);
                            break;
		    }

		}
	}

	public void PaintSecResArc(IPlayer player, int Angle_Start, int Angle_End, float ArcSize)
	{
		if (!player.IsOnScreen) return;

		var worldCoord = player.FloorCoordinate;
		var resArc = (int)(player.Stats.ResourcePctSec * (Angle_End - Angle_Start) / 100f) + Angle_Start;

		using (var pg3 = Hud.Render.CreateGeometry()) {
			using (var gs3 = pg3.Open()) {
				var mx = ArcSize * (float)Math.Cos(Angle_Start * Math.PI / 180f);
				var my = ArcSize * (float)Math.Sin(Angle_Start * Math.PI / 180f);

				var screenCoord = worldCoord.Offset(mx, my, 0).ToScreenCoordinate(true);
				var vector = new Vector2(screenCoord.X, screenCoord.Y);

				gs3.BeginFigure(vector, FigureBegin.Filled);

				for (int angle = Angle_Start + 1; angle <= Angle_End; angle++)
				{
					mx = ArcSize * (float)Math.Cos(angle * Math.PI / 180f);
					my = ArcSize * (float)Math.Sin(angle * Math.PI / 180f);

					screenCoord = worldCoord.Offset(mx, my, 0).ToScreenCoordinate(true);
					vector = new Vector2(screenCoord.X, screenCoord.Y);

					if (angle <= resArc) gs3.AddLine(vector);
				}

				gs3.EndFigure(FigureEnd.Open);
				gs3.Close();
			}

			Brush_BG.DrawGeometry(pg3); Brush_ResBG.DrawGeometry(pg3); Brush_Disc.DrawGeometry(pg3);

		}
	}

    }
}