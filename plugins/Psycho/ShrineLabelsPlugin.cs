//################################################################################
//# ..:: created with TCT Version 5.2 for THUD v7.6 (17.11.20.0) ::.. by RealGsus #
//################################################################################

using Turbo.Plugins.Default;
using System.Collections.Generic;
using System.Linq;
namespace Turbo.Plugins.Psycho
{
    public class ShrineLabelsPlugin : BasePlugin, ICustomizer, IInGameWorldPainter
    {
        public bool UseCustomColors { get; set; }
        public bool UseCustomNames { get; set; }
        public string PossibleRiftPylonName { get; set; }
        public Dictionary<ShrineType, WorldDecoratorCollection> ShrineDecorators { get; set; }
        public Dictionary<ShrineType, string> ShrineCustomNames { get; set; }
        public WorldDecoratorCollection PossibleRiftPylonDecorators { get; set; }

        public ShrineLabelsPlugin()
        {
            Enabled = true;
            UseCustomColors = true;
            UseCustomNames = true;
        }

        public WorldDecoratorCollection CreateDecorators(float size = 7f, int a = 255, int r = 255, int g = 255, int b = 55, float radiusOffset = 5.0f)
        {
            return new WorldDecoratorCollection(
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", size, a, r, g, b, true, false, 128, 0, 0, 0, true),
                    RadiusOffset = radiusOffset,
                }
            );
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            ShrineDecorators = new Dictionary<ShrineType, WorldDecoratorCollection>();
            ShrineCustomNames = new Dictionary<ShrineType, string>();

            ShrineDecorators[ShrineType.BlessedShrine] = CreateDecorators(7f, 255, 208, 178, 0);
            ShrineDecorators[ShrineType.EnlightenedShrine] = CreateDecorators(7f, 255, 208, 178, 0);
            ShrineDecorators[ShrineType.FortuneShrine] = CreateDecorators(7f, 255, 208, 178, 0);
            ShrineDecorators[ShrineType.FrenziedShrine] = CreateDecorators(7f, 255, 208, 178, 0);
            ShrineDecorators[ShrineType.EmpoweredShrine] = CreateDecorators(7f, 255, 208, 178, 0);
            ShrineDecorators[ShrineType.FleetingShrine] = CreateDecorators(7f, 255, 208, 178, 0);
            ShrineDecorators[ShrineType.PowerPylon] = CreateDecorators(7f, 255, 255, 255, 0);
            ShrineDecorators[ShrineType.ConduitPylon] = CreateDecorators(7f, 255, 255, 255, 0);
            ShrineDecorators[ShrineType.ChannelingPylon] = CreateDecorators(7f, 255, 255, 255, 0);
            ShrineDecorators[ShrineType.ShieldPylon] = CreateDecorators(7f, 255, 255, 255, 0);
            ShrineDecorators[ShrineType.SpeedPylon] = CreateDecorators(7f, 255, 255, 255, 0);
            ShrineDecorators[ShrineType.BanditShrine] = CreateDecorators(7f, 255, 255, 0, 0);
            ShrineDecorators[ShrineType.PoolOfReflection] = CreateDecorators(7f, 255, 255, 216, 0);
            ShrineDecorators[ShrineType.HealingWell] = CreateDecorators(7f, 255, 230, 184, 183);

            PossibleRiftPylonDecorators = CreateDecorators(7f, 255, 255, 255, 0);

            ShrineCustomNames[ShrineType.BlessedShrine] = "Protection Shrine";
            ShrineCustomNames[ShrineType.EnlightenedShrine] = "Enlightened Shrine";
            ShrineCustomNames[ShrineType.FortuneShrine] = "Fortune Shrine";
            ShrineCustomNames[ShrineType.FrenziedShrine] = "Frenzied Shrine";
            ShrineCustomNames[ShrineType.EmpoweredShrine] = "Empowered Shrine";
            ShrineCustomNames[ShrineType.FleetingShrine] = "Fleeting Shrine";
            ShrineCustomNames[ShrineType.PowerPylon] = "Power Pylon";
            ShrineCustomNames[ShrineType.ConduitPylon] = "Conduit Pylon";
            ShrineCustomNames[ShrineType.ChannelingPylon] = "Channeling Pylon";
            ShrineCustomNames[ShrineType.ShieldPylon] = "Shield Pylon";
            ShrineCustomNames[ShrineType.SpeedPylon] = "Speed Pylon";
            ShrineCustomNames[ShrineType.BanditShrine] = "Bandit Shrine";
            ShrineCustomNames[ShrineType.PoolOfReflection] = "Pool of Reflection";
            ShrineCustomNames[ShrineType.HealingWell] = "Healing Well";

            PossibleRiftPylonName = "Pylon?";
        }

        public void Customize()
        {
            if (UseCustomColors == false && UseCustomNames == false)
            {
                Hud.RunOnPlugin<ShrinePlugin>(plugin =>
                {
                    plugin.AllShrineDecorator.Add(new MapLabelDecorator(Hud)
                    {
                        LabelFont = Hud.Render.CreateFont("tahoma", 6f, 192, 255, 255, 55, false, false, 128, 0, 0, 0, true),
                        RadiusOffset = 5.0f,
                    });
                });
            }
        }

        public void PaintWorld(WorldLayer layer)
        {
            if (UseCustomColors != true && UseCustomNames != true) return;

            var shrines = Hud.Game.Shrines.Where(s => s.DisplayOnOverlay);
            foreach (var shrine in shrines)
            {
                if (!ShrineDecorators.ContainsKey(shrine.Type)) continue;

                var shrineName = UseCustomNames ? ShrineCustomNames[shrine.Type] : shrine.SnoActor.NameLocalized;
                ShrineDecorators[shrine.Type].Paint(layer, shrine, shrine.FloorCoordinate, shrineName);
            }

            var riftPylonSpawnPoints = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._markerlocation_tieredriftpylon);
            foreach (var actor in riftPylonSpawnPoints)
            {
                PossibleRiftPylonDecorators.Paint(layer, actor, actor.FloorCoordinate, UseCustomNames ? PossibleRiftPylonName : "Pylon?");
            }
        }
    }
}
