using System.Collections.Generic;
using System.Linq;
using System;
using Turbo.Plugins.Default;
namespace Turbo.Plugins.BM
{
    public class HealthBarOnElitePlugin : BasePlugin, IInGameWorldPainter
    {
        public IFont TextFont { get; set; }
        public IBrush BorderBrush { get; set; }

        public IBrush BorderBrushInvulS { get; set; }
        public IBrush BorderBrushInvulH { get; set; }
		public IBrush BackgroundBrushInvul { get; set; }
		//test
		public IBrush BackgroundBrushStrong { get; set; }
		public IBrush BorderBrushStrong { get; set; }
		//
        public IBrush BackgroundBrush { get; set; }
        public IBrush RareBrush { get; set; }
		public IBrush RareJBrush { get; set; }
        public IBrush ChampionBrush { get; set; }
        public IFont TextFontHaunt { get; set; }
        public IFont TextFontStrongarmed { get; set; }
		

        public HealthBarOnElitePlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            Order = 25000;

            TextFont = Hud.Render.CreateFont("tahoma", 9, 255, 255, 255, 255, false, false, true);
            BorderBrush = Hud.Render.CreateBrush(255, 0, 100, 0, -1);
            BorderBrushInvulS = Hud.Render.CreateBrush(255, 180, 0, 0, -1);
            BorderBrushInvulH = Hud.Render.CreateBrush(255, 255, 255, 0, -1);
            BackgroundBrushInvul = Hud.Render.CreateBrush(200, 255, 0, 0, 0);
            BackgroundBrush = Hud.Render.CreateBrush(255, 100, 100, 100, 0);
            RareBrush = Hud.Render.CreateBrush(255, 255, 148, 20, 0);
		    RareJBrush = Hud.Render.CreateBrush(255, 255, 50, 0, 0);
            ChampionBrush = Hud.Render.CreateBrush(255, 0, 128, 255, 0);
			//test
			BackgroundBrushStrong = Hud.Render.CreateBrush(255, 25, 225, 25, 0);
			BorderBrushStrong = Hud.Render.CreateBrush(255, 32, 230, 32, -1);

            TextFontStrongarmed = Hud.Render.CreateFont("tahoma", 9, 255, 32, 230, 32, false, false, true);
            TextFontHaunt = Hud.Render.CreateFont("tahoma", 9, 255, 255, 0, 0, false, false, true);
            TextFontStrongarmed.SetShadowBrush(255, 255, 0, 255, false);
            TextFontHaunt.SetShadowBrush(255, 255, 0, 0, false);
        }

        private bool HasAffix(IMonster m, MonsterAffix afx){
            return m.AffixSnoList.Any(a => a.Affix == afx);
        }

        public void PaintWorld(WorldLayer layer)
        {
            var h = 17;
            var w1 = 35;
            var textStrongarmed = "";
            var layoutStrongarmed = TextFontStrongarmed.GetTextLayout(textStrongarmed);
            var textHaunt = "H";
            var layoutHaunt = TextFontHaunt.GetTextLayout(textHaunt);
            var layoutInvul = TextFontHaunt.GetTextLayout("");
            var py = Hud.Window.Size.Height / 600;
            var monsters = Hud.Game.AliveMonsters.Where(x => x.IsAlive);
            List<IMonster> monstersElite = new List<IMonster>();
            foreach (var monster in monsters)
            {
			if (monster.SummonerAcdDynamicId == 0)
			{
                if (monster.Rarity == ActorRarity.Champion || monster.Rarity == ActorRarity.Rare)
                {
                    monstersElite.Add(monster);
                }
            }
			}
            foreach (var monster in monstersElite)
            {
                    var wint = monster.CurHealth / monster.MaxHealth ; var hptext = "";
					if ((wint < 0) || (wint > 1)) { wint = 1; hptext = "bug"; }
                    else { hptext = ValueToString(wint * 100 , ValueFormat.NormalNumberNoDecimal); }
                    var w = wint * w1 ;
                    var layout = TextFont.GetTextLayout(hptext);
					var layoutS = TextFontStrongarmed.GetTextLayout(hptext);
                    
					var monsterX = monster.FloorCoordinate.ToScreenCoordinate().X - w1 / 2;
                    var monsterY = monster.FloorCoordinate.ToScreenCoordinate().Y - py * 29;   //기존값 8 
                    if (monsterY < 0) {  monsterY = monster.FloorCoordinate.ToScreenCoordinate().Y ; }
                    var StrongarmedX = monsterX - w1 + 35;  //기본값 /2
                    //var hauntX = monsterX + w1 + 5;
					var buffY = monsterY - 1;  // 기본값 -1
                    var hpX = monsterX + 10; // 기본값 +7
					//test
					var StrongarmedY = monsterY - 20;

                    BorderBrush.DrawRectangle(monsterX, monsterY, w1, h);
                    BackgroundBrush.DrawRectangle(monsterX, monsterY, w1, h);
                    if (monster.Rarity == ActorRarity.Champion) 
					{
						ChampionBrush.DrawRectangle(monsterX, monsterY, (float)w, h);
					}
                    if (monster.Rarity == ActorRarity.Rare)
					{
                        RareBrush.DrawRectangle(monsterX, monsterY, (float)w, h);
						// bool flagJ = false;
						// foreach (var snoMonsterAffix in monster.AffixSnoList)
                        // {
                            // if (snoMonsterAffix.Affix == MonsterAffix.Juggernaut)
                            // {
                                // flagJ = true;
                                // break;
                            // }
                        // }
                        // if (flagJ) RareJBrush.DrawRectangle(monsterX, monsterY, (float)w, h);
                        // else RareBrush.DrawRectangle(monsterX, monsterY, (float)w, h);
                    }
                    if (monster.Invulnerable) {
                         BackgroundBrushInvul.DrawRectangle(monsterX, monsterY, w1, h);
                         if (monster.Invisible) { BorderBrushInvulH.DrawRectangle(monsterX - 1, monsterY - 1, w1 + 2, h + 2); }
                         else if ( HasAffix(monster, MonsterAffix.Shielding) ) BorderBrushInvulS.DrawRectangle(monsterX - 1, monsterY - 1, w1 + 2, h + 2);
						}
                    // if (monster.Strongarmed && monster.Rarity == ActorRarity.Rare) {
						// BackgroundBrush.DrawRectangle(monsterX, monsterY, w1, h);
						// RareBrush.DrawRectangle(monsterX, monsterY, (float)w, h);
						// BorderBrushStrong.DrawRectangle(monsterX - 1, monsterY - 1, w1 + 2, h + 2);
						// TextFontStrongarmed.DrawText(layoutS, hpX, buffY);
					// }
					// if (monster.Strongarmed && monster.Rarity == ActorRarity.Champion) {
						// BackgroundBrush.DrawRectangle(monsterX, monsterY, w1, h);
						// //ChampionBrush.DrawRectangle(monsterX, monsterY, (float)w, h);
						// //BorderBrushStrong.DrawRectangle(monsterX - 1, monsterY - 1, w1 + 2, h + 2);
						// //TextFontStrongarmed.DrawText(layoutS, hpX, buffY);
					// }
					
					//TextFontStrongarmed.DrawText(layoutStrongarmed, StrongarmedX, StrongarmedY);  //기본값 buffY
                    
					
					
					//if (monster.Haunted) TextFontHaunt.DrawText(layoutHaunt, hauntX, buffY);
                    TextFont.DrawText(layout, hpX, buffY);                
            }
            monstersElite.Clear();
        }
    }
}