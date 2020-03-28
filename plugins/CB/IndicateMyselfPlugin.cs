//################################################################################
//# ..:: created with TCT Version 5.2 for THUD v7.6 (17.11.20.0) ::.. by RealGsus #
//################################################################################

using Turbo.Plugins.Default;

namespace Turbo.Plugins.CB
{
    public class IndicateMyselfPlugin : BasePlugin, IInGameWorldPainter
    {
        public WorldDecoratorCollection IndicateMyselfDecorator { get; set; }

        public IndicateMyselfPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

         IndicateMyselfDecorator = new WorldDecoratorCollection(
             new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 255, 255, 0, 6.0f),
                    Radius = 0.1f
                }
		,
             new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(204, 255, 255, 255, 1f),
                    Radius = 10f
                }
         );
        }

        public void PaintWorld(WorldLayer layer)
        {
            if (Hud.Game.IsInTown) return;
            var me = Hud.Game.Me;
            if (Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.DemonHunter) {
			
			IndicateMyselfDecorator.Paint(layer, me, me.FloorCoordinate, null);
			}
        }
    }
}
