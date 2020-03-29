// Modified. Original (by glq): https://www.ownedcore.com/forums/diablo-3/turbohud/turbohud-discussions/618926-damage-count-down-circle-of-spirit-barrage-post3728819.html#post3728819
using System.Linq;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.glq
{
	public class SpiritBarragePhantasmPlugin : BasePlugin, IInGameWorldPainter
	{

		public WorldDecoratorCollection SpiritBarragePhantasmDecorator { get; set; }
		public bool ShowOthers {get; set;}

		public SpiritBarragePhantasmPlugin()
		{
			Enabled = true;
		}

		public override void Load(IController hud)
		{
			base.Load(hud);

			ShowOthers = true; // also show those of other players.

			SpiritBarragePhantasmDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud)
				{
					//B=255, R= 0, G=128 originally; now pink = 255/213/0
					Brush = Hud.Render.CreateBrush(255, 213, 0, 255, 8),
					Radius = 10,
				},
				new GroundLabelDecorator(Hud)
				{
					CountDownFrom = 10,
					TextFont = Hud.Render.CreateFont("tahoma", 9, 255, 100, 255, 150, true, false, 128, 0, 0, 0, true),
				},
				new GroundTimerDecorator(Hud)
				{
					CountDownFrom = 10,
					BackgroundBrushEmpty = Hud.Render.CreateBrush(100, 0, 0, 0, 0),
					BackgroundBrushFill = Hud.Render.CreateBrush(200, 0, 128, 255, 0),
					Radius = 25,
				}
			);
		}

		public void PaintWorld(WorldLayer layer)
		{
			var actors = Hud.Game.Actors.Where(a => a.SnoActor.Sno == ActorSnoEnum._wd_spiritbarragerune_aoe_ghostmodel);
			foreach (var actor in actors)
			{
				if ( (actor.SummonerAcdDynamicId == Hud.Game.Me.SummonerId) || ShowOthers )
					SpiritBarragePhantasmDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
			}
		}
    }
}
