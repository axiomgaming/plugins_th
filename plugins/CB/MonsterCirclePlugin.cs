//################################################################################
//# ..:: created with TCT Version 5.2 for THUD v7.6 (17.11.20.0) ::.. by RealGsus #
//################################################################################

using System.Collections.Generic;
using Turbo.Plugins.Default;
using System.Linq;
using System;

namespace Turbo.Plugins.CB
{

    public class MonsterCirclePlugin : BasePlugin, IInGameWorldPainter
    {

        public Dictionary<MonsterAffix, WorldDecoratorCollection> AffixDecorators { get; set; }
        public Dictionary<MonsterAffix, string> CustomAffixNames { get; set; }
        public WorldDecoratorCollection ChampionDecorator { get; set; }
        public WorldDecoratorCollection RareDecorator { get; set; }
        public WorldDecoratorCollection RareMinionDecorator { get; set; }
        public WorldDecoratorCollection UniqueDecorator { get; set; }
        public WorldDecoratorCollection BossDecorator { get; set; }

        public MonsterCirclePlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            var shadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1);
         
            ChampionDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 0, 128, 255, 8f),
                    Radius = -1f
                },
                new MapShapeDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 0, 128, 255, 0f),
                    Radius = 6f,
                    ShapePainter = new CircleShapePainter(Hud)
                }
            );
         
            RareDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 128, 0, 8f),
                    Radius = -1f
                },
                new MapShapeDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 128, 0, 0f),
                    Radius = 6f,
                    ShapePainter = new CircleShapePainter(Hud)
                }
            );
         
            RareMinionDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(0, 255, 128, 0, 6f, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = -1f
                },
                new MapShapeDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 182, 92, 20, 2.0f),
                    Radius = 4f,
                    ShapePainter = new CircleShapePainter(Hud)
                }
            );
         
            UniqueDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 140, 255, 6f),
                    Radius = -1f
                },
                new MapShapeDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 140, 255, 0f),
                    Radius = 6f,
                    ShapePainter = new CircleShapePainter(Hud)
                }
            );
         
            BossDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 96, 0, 8f),
                    Radius = -1f
                },
                new MapShapeDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 96, 0, 0f),
                    Radius = 6f,
                    ShapePainter = new CircleShapePainter(Hud)
                }
            );
         
            CustomAffixNames = new Dictionary<MonsterAffix, string>();
            AffixDecorators = new Dictionary<MonsterAffix, WorldDecoratorCollection>();

        }

        public void PaintWorld(WorldLayer layer)
        {
            var monsters = Hud.Game.AliveMonsters;

            List<IMonster> monstersElite = new List<IMonster>();
            foreach (var monster in monsters)
            {
                if (monster.Rarity == ActorRarity.Champion || monster.Rarity == ActorRarity.Rare)
                {
                   monstersElite.Add(monster);
                }

             if (monster.Rarity == ActorRarity.RareMinion) {
                    RareMinionDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
                }

                if (monster.Rarity == ActorRarity.Unique) {
                    UniqueDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
                }

                if (monster.Rarity == ActorRarity.Boss) {
                    BossDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
                }
            }
            foreach (var monster in monstersElite)
            {
                if (monster.SummonerAcdDynamicId == 0)
                {
                    bool flag = true;
                    foreach (var snoMonsterAffix in monster.AffixSnoList)
                    {
                        string affixName = null;
                        if (CustomAffixNames.ContainsKey(snoMonsterAffix.Affix))
                        {
                            affixName = CustomAffixNames[snoMonsterAffix.Affix];
                        }
                        else affixName = snoMonsterAffix.NameLocalized;

                        WorldDecoratorCollection decorator;
                        if (!AffixDecorators.TryGetValue(snoMonsterAffix.Affix, out decorator)) continue;
                        decorator.Paint(layer, monster, monster.FloorCoordinate, affixName);
                    }
                    if (monster.Rarity == ActorRarity.Rare)
                    {
                        if (flag) RareDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);
                    }
                    if (monster.Rarity == ActorRarity.Champion) ChampionDecorator.Paint(layer, monster, monster.FloorCoordinate, monster.SnoMonster.NameLocalized);

             }

         }

            monstersElite.Clear();
        }
    }
}
