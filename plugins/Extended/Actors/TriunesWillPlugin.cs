// TriunesWillPlugin
// Ground Decorators & Timers for Triune's Will circles from Season 18 buff.

using Turbo.Plugins.Default;
using System.Linq;

namespace Turbo.Plugins.Extended.Actors
{
    public class TriunesWillPlugin : BasePlugin, IInGameWorldPainter
    {
	public WorldDecoratorCollection LoveDecorator { get; set; }
	public WorldDecoratorCollection DeterminationDecorator { get; set; }
	public WorldDecoratorCollection CreationDecorator { get; set; }

	public bool EnableLove { get; set; }
	public bool EnableDetermination { get; set; }
	public bool EnableCreation { get; set; }

	public TriunesWillPlugin()
        {
            Enabled = true;

	    EnableLove = true;		// Damage Circle
	    EnableCreation = true;	// CDR Circle
	    EnableDetermination = true;	// RCR Circle
        }

	public override void Load(IController hud)
        {
            base.Load(hud);

	    LoveDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 0, 128, -2),
                    Radius = 10.0f,
                },
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 7,
                    TextFont = Hud.Render.CreateFont("tahoma", 11, 255, 255, 96, 255, true, false, 128, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 7,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(164, 192, 0, 0, 0),
                    Radius = 30,
                }
                );

	    CreationDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 64, 200, 144, -2),
                    Radius = 10.0f,
                },
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 7,
                    TextFont = Hud.Render.CreateFont("tahoma", 11, 255, 96, 230, 196, true, false, 128, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 7,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(164, 0, 192, 192, 0),
                    Radius = 30,
                }
                );

	    DeterminationDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 164, 164, 32, -2), // Dark Blue 0, 64, 255
                    Radius = 10.0f,
                },
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 7,
                    TextFont = Hud.Render.CreateFont("tahoma", 11, 255, 200, 200, 96, true, false, 128, 0, 0, 0, true), // Dark Blue 96, 96, 255
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 7,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(164, 164, 164, 0, 0), // Dark Blue 0, 0, 192
                    Radius = 30,
                }
                );

        }

	public void PaintWorld(WorldLayer layer)
        {
            if (Hud.Game.IsInTown) return;

	if (EnableLove) {
            var love = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._generic_proxy && x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, 483606) == 1);
            foreach (var actor in love)
            {
                LoveDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
            }
	}

	if (EnableCreation) {
            var creation = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._generic_proxy && x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_7_Visual_Effect_None, 483606) == 1);
            foreach (var actor in creation)
            {
                CreationDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
            }
	}

	if (EnableDetermination) {
            var determination = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._generic_proxy && x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_6_Visual_Effect_None, 483606) == 1);
            foreach (var actor in determination)
            {
                DeterminationDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
            }
	}


        }
    }
}