
using Turbo.Plugins.Default;
using System.Linq;

namespace Turbo.Plugins.TLHP
{
    public class TLHP_NewOculusPlugin : BasePlugin, IInGameWorldPainter
    {
	public WorldDecoratorCollection LoveDecorator { get; set; }
	public WorldDecoratorCollection DeterminationDecorator { get; set; }
	public WorldDecoratorCollection CreationDecorator { get; set; }
	public WorldDecoratorCollection RegularDecorator { get; set; }

	public WorldDecoratorCollection LoveInsideDecorator { get; set; }
	public WorldDecoratorCollection DeterminationInsideDecorator { get; set; }
	public WorldDecoratorCollection CreationInsideDecorator { get; set; }
	public WorldDecoratorCollection RegularInsideDecorator { get; set; }

	public bool EnableLove { get; set; }
	public bool EnableDetermination { get; set; }
	public bool EnableCreation { get; set; }
	public bool EnableInsideMarker { get; set; }
	public bool EnableRegular { get; set; }

	public TLHP_NewOculusPlugin()
    {
	    Enabled = true;

	    EnableLove = true;		// Damage Circle
	    EnableCreation = true;	// CDR Circle
	    EnableDetermination = false;	// RCR Circle
	  	EnableRegular = true;

	    EnableInsideMarker = true;	// Additional Circle Decorator when you are in it
    }

	public override void Load(IController hud)
    {
        base.Load(hud);


	    LoveInsideDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 220, 0, 64, -2, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 9.0f,
                },
				new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 220, 0, 64, -2, SharpDX.Direct2D1.DashStyle.DashDotDot),
                    Radius = 11.0f,
                }
                );

	    CreationInsideDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 0, 80, 150, -2, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 9.0f,
                },
				new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 0, 80, 150, -2, SharpDX.Direct2D1.DashStyle.DashDotDot),
                    Radius = 11.0f,
                }
                );
       RegularInsideDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 255, 255, -2, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = 9.0f,
                },
				new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 255, 255, -2, SharpDX.Direct2D1.DashStyle.DashDotDot),   //Brush = Hud.Render.CreateBrush(255, 128, 255, 0, -2),
                    Radius = 11.0f,
                }
                );


	    // DeterminationInsideDecorator = new WorldDecoratorCollection(
                // new GroundCircleDecorator(Hud)
                // {
                    // Brush = Hud.Render.CreateBrush(255, 164, 100, 32, -2, SharpDX.Direct2D1.DashStyle.Dash), // Dark Blue 0, 64, 255
                    // Radius = 9.0f,
                // }
                // );

	    LoveDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 0, 128, -2),
                    Radius = 10.0f,
                }
                );

	    CreationDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 64, 200, 144, -2),
                    Radius = 10.0f,
                }
                );

	    DeterminationDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 164, 164, 32, -2), // Dark Blue 0, 64, 255
                    Radius = 10.0f,
                }
                );
		RegularDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 255, 255, -2),   //Brush = Hud.Render.CreateBrush(255, 128, 255, 0, -2),
                    Radius = 10.0f,
                },
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 7,
                    TextFont = Hud.Render.CreateFont("tahoma", 11, 255, 255, 255, 255, true, false, 128, 0, 0, 0, true),  //("tahoma", 11, 255, 96, 255, 96, true, false, 128, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 7,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(200, 255, 255, 255, 0), //(200, 0, 192, 0, 0),
                    Radius = 30,
                }
                );



	}

	public void PaintWorld(WorldLayer layer)
    {
	    if (Hud.Game.IsInTown) return;

	    bool Inside = false;

	if (EnableLove) {
	    var love = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._generic_proxy && x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, 483606) == 1).OrderBy(d => d.CentralXyDistanceToMe);
	    if (EnableInsideMarker && Hud.Game.Me.Powers.BuffIsActive(483606, 2)) { Inside = true; }
	    foreach (var actor in love)
	    {
		LoveDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
		if (Inside) { LoveInsideDecorator.Paint(layer, actor, actor.FloorCoordinate, null); Inside = false; }
	    }
	}

	if (EnableCreation) {
            var creation = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._generic_proxy && x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_7_Visual_Effect_None, 483606) == 1).OrderBy(d => d.CentralXyDistanceToMe);
	    if (EnableInsideMarker && Hud.Game.Me.Powers.BuffIsActive(483606, 8)) { Inside = true; }
            foreach (var actor in creation)
            {
		CreationDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
		if (Inside) { CreationInsideDecorator.Paint(layer, actor, actor.FloorCoordinate, null); Inside = false; }
            }
	}

	if (EnableDetermination) {
            var determination = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._generic_proxy && x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_6_Visual_Effect_None, 483606) == 1).OrderBy(d => d.CentralXyDistanceToMe);
	    if (EnableInsideMarker && Hud.Game.Me.Powers.BuffIsActive(483606, 5)) { Inside = true; }
            foreach (var actor in determination)
            {
		DeterminationDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
		if (Inside) { DeterminationInsideDecorator.Paint(layer, actor, actor.FloorCoordinate, null); Inside = false; }
            }
	}

	if (EnableRegular) {

			var regular = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._generic_proxy && x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, Hud.Sno.SnoPowers.OculusRing.Sno) == 1);
	    if (EnableInsideMarker && Hud.Game.Me.Powers.BuffIsActive(402461, 2)) { Inside = true; }
            foreach (var actor in regular)
            {
		RegularDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
		if (Inside) { RegularInsideDecorator.Paint(layer, actor, actor.FloorCoordinate, null); Inside = false; }
            }
	}

     }
    }
}
