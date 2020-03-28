using System.Linq;
using System.Collections.Generic;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.DavMonster
{
	public class DAV_BossWarmingPlugin : BasePlugin, IInGameWorldPainter {
		public Dictionary<AnimSnoEnum, string> WarmingMessage;
		
		public WorldDecoratorCollection MessageDecorator { get; set; }
		public float BossOffsetX { get; set; }
		public float BossOffsetY { get; set; }
		public float BossOffsetZ { get; set; }
		public float MeOffsetX { get; set; }
		public float MeOffsetY { get; set; }
		public float MeOffsetZ { get; set; }
		public bool ShowOrlashClone { get; set; }
		public bool onBoss { get; set; }
		public bool onMe { get; set; }
		public bool GRonly { get; set; }
		
		public DAV_BossWarmingPlugin() {
			Enabled = true;
			BossOffsetX = -20.0f;
			BossOffsetY = 0.0f;
			BossOffsetZ = 10.0f;
			MeOffsetX = -10.0f;
			MeOffsetY = 0.0f;
			MeOffsetZ = 5.0f;
			ShowOrlashClone = false;
			GRonly = true;
			onBoss = true;
			onMe = true;
		}

		public override void Load(IController hud) {
			base.Load(hud);

			WarmingMessage = new Dictionary<AnimSnoEnum, string>();
			
			MessageDecorator = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud) {
					TextFont = Hud.Render.CreateFont("tahoma", 15, 255, 255, 255, 51, true, true, 255, 255, 51, 51, true),
				}
			);
		}
		
		public void PaintWorld(WorldLayer layer) {
			if (GRonly && Hud.Game.SpecialArea != SpecialArea.GreaterRift) return ;
			
			var mylocate = Hud.Game.Me.FloorCoordinate;
			var bosses = Hud.Game.AliveMonsters.Where(m => m.Rarity == ActorRarity.Boss);
			
			foreach(IMonster m in bosses) {
				if (!ShowOrlashClone && m.SummonerAcdDynamicId != 0) continue;
				
				string outmessage;
				if (!WarmingMessage.TryGetValue(m.Animation, out outmessage)) continue;
				
				if (onBoss) MessageDecorator.Paint(layer, m, m.FloorCoordinate.Offset(BossOffsetX, BossOffsetY, BossOffsetZ), outmessage);
				if (onMe) MessageDecorator.Paint(layer, null, mylocate.Offset(MeOffsetX, MeOffsetY, MeOffsetZ), outmessage);
			}
		}
	}
}