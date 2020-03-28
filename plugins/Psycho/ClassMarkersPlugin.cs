using Turbo.Plugins.Default;
using System.Collections.Generic;
using System;
namespace Turbo.Plugins.Psycho
{
    public class ClassMarkersPlugin : BasePlugin, ICustomizer, IInGameWorldPainter
    {
        public Dictionary<HeroClass, MapShapeDecorator> MyMapCircleDecorator { get; set; }
        public Dictionary<HeroClass, GroundCircleDecorator> MyGroundCirleDecorator { get; set; }
        public WorldDecoratorCollection MyGroundCircle {get; set;}
        public bool MyPlayerCircle {get; set;}
        public bool OtherPlayersCircles {get; set;}
        public bool MyPlayerCircleColorOverride {get; set;}
 
        public ClassMarkersPlugin()
        {
            Enabled = true;
            MyPlayerCircle = true;
            OtherPlayersCircles = false;
            MyPlayerCircleColorOverride = false;
        }

        public MapShapeDecorator CreateMapCircleDecorators(int a = 255, int r = 255, int g = 255, int b = 255, int t = 5, float radius = 1f)
        {
            return new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(a, r, g, b, t),
                    ShapePainter = new CircleShapePainter(Hud),
                    Radius = radius,
                };
        }
        public GroundCircleDecorator CreateGroundCircleDecorators(int a = 255, int r = 255, int g = 255, int b = 255, int t = 5, float radius = 2f)
        {
            return new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(a, r, g, b, t),
                    Radius = radius,
                };
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            MyMapCircleDecorator = new Dictionary<HeroClass, MapShapeDecorator>();
            MyGroundCirleDecorator = new Dictionary<HeroClass, GroundCircleDecorator>();

            MyMapCircleDecorator[HeroClass.Barbarian] = CreateMapCircleDecorators(200, 204, 0, 0);
            MyMapCircleDecorator[HeroClass.Crusader] = CreateMapCircleDecorators(240, 204, 204, 0);
            MyMapCircleDecorator[HeroClass.DemonHunter] = CreateMapCircleDecorators(255, 102, 0, 204);
            MyMapCircleDecorator[HeroClass.Monk] = CreateMapCircleDecorators(245, 255, 255, 255);
            MyMapCircleDecorator[HeroClass.WitchDoctor] = CreateMapCircleDecorators(155, 102, 204, 0);
            MyMapCircleDecorator[HeroClass.Wizard] = CreateMapCircleDecorators(255, 0, 102, 204, 5);
            MyMapCircleDecorator[HeroClass.Necromancer] = CreateMapCircleDecorators(255, 0, 204, 204);

            MyGroundCirleDecorator[HeroClass.Barbarian] = CreateGroundCircleDecorators(200, 204, 0, 0);
            MyGroundCirleDecorator[HeroClass.Crusader] = CreateGroundCircleDecorators(240, 204, 204, 0);
            MyGroundCirleDecorator[HeroClass.DemonHunter] = CreateGroundCircleDecorators(255, 102, 0, 204);
            MyGroundCirleDecorator[HeroClass.Monk] = CreateGroundCircleDecorators(245, 255, 255, 255);
            MyGroundCirleDecorator[HeroClass.WitchDoctor] = CreateGroundCircleDecorators(155, 102, 204, 0);
            MyGroundCirleDecorator[HeroClass.Wizard] = CreateGroundCircleDecorators(255, 0, 102, 204, 5);
            MyGroundCirleDecorator[HeroClass.Necromancer] = CreateGroundCircleDecorators(255, 0, 204, 204);
  
            MyGroundCircle = new WorldDecoratorCollection(CreateGroundCircleDecorators(255,255,255,255),CreateMapCircleDecorators(255,255,255,255));
        }

        public void Customize()
        {
            if (OtherPlayersCircles)
            {
                Hud.RunOnPlugin<OtherPlayersPlugin>(plugin =>
                {
                    plugin.NameOffsetZ = 0;  
                    foreach (HeroClass heroClass in Enum.GetValues(typeof(HeroClass)))
                    {
                        plugin.DecoratorByClass[heroClass].Decorators.Add(MyGroundCirleDecorator[heroClass]);
                        plugin.DecoratorByClass[heroClass].Decorators.Add(MyMapCircleDecorator[heroClass]);
                    }
                });
            }
        }
            
        public void PaintWorld(WorldLayer layer)
        {
            if (Hud.Game.IsInTown) return;
            if (MyPlayerCircle == false) return;
 
            if (MyPlayerCircleColorOverride)
            {
                MyGroundCircle.Paint(layer, Hud.Game.Me, Hud.Game.Me.FloorCoordinate, string.Empty);
                return;
            }
            if (OtherPlayersCircles)
            {            
                Hud.RunOnPlugin<OtherPlayersPlugin>(plugin =>
                {
                    plugin.DecoratorByClass[Hud.Game.Me.HeroClassDefinition.HeroClass].Paint(layer, Hud.Game.Me, Hud.Game.Me.FloorCoordinate, string.Empty);
                    return;
                });  
            }              
        }
    }
}