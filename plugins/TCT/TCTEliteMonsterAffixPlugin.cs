//################################################################################
//# ..:: created with TCT Version 5.2 for THUD v7.6 (17.11.20.0) ::.. by RealGsus #
//################################################################################

using Turbo.Plugins.Default;

namespace Turbo.Plugins.TCT
{

    public class TCTEliteMonsterAffixPlugin : BasePlugin, ICustomizer
    {

        public TCTEliteMonsterAffixPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }

        public void Customize()
        {
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Arcane);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Avenger);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Desecrator);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Electrified);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.ExtraHealth);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Fast);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.FireChains);
			// Fire Chains//
			// Hud.GetPlugin<EliteMonsterAffixPlugin>().CustomAffixNames.Add(MonsterAffix.Frozen, "Frozen");
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Frozen);
			// Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Add(MonsterAffix.Frozen, new WorldDecoratorCollection(
            // new GroundLabelDecorator(Hud)
            // {
                // BorderBrush = Hud.Render.CreateBrush(128, 0, 0, 255, 2),
                // TextFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 255, 255, true, false, false),
                // BackgroundBrush = Hud.Render.CreateBrush(255, 0, 25, 255, 0),
            // }
            // ));
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.FrozenPulse);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.HealthLink);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Horde);
            //##### ILLUSIONIST #####
            Hud.GetPlugin<EliteMonsterAffixPlugin>().CustomAffixNames.Add(MonsterAffix.Illusionist, "Illusionist");
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Illusionist);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Add(MonsterAffix.Illusionist, new WorldDecoratorCollection(
            new GroundLabelDecorator(Hud)
            {
                BorderBrush = Hud.Render.CreateBrush(128, 255, 0, 0, 2),
                TextFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 255, 255, true, false, false),
                BackgroundBrush = Hud.Render.CreateBrush(255, 255, 25, 0, 0),
            }
            ));
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Jailer);
            //##### JUGGERNAUT #####
            Hud.GetPlugin<EliteMonsterAffixPlugin>().CustomAffixNames.Add(MonsterAffix.Juggernaut, "Juggernaut");
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Juggernaut);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Add(MonsterAffix.Juggernaut, new WorldDecoratorCollection(
            new GroundLabelDecorator(Hud)
            {
                BorderBrush = Hud.Render.CreateBrush(128, 255, 0, 0, 2),
                TextFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 255, 255, true, false, false),
                BackgroundBrush = Hud.Render.CreateBrush(255, 255, 0, 0, 0),
            }
            ));
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Knockback);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.MissileDampening);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Molten);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Mortar);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Nightmarish);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Orbiter);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Plagued);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Poison);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Reflect);
            //##### SHIELDING #####
            Hud.GetPlugin<EliteMonsterAffixPlugin>().CustomAffixNames.Add(MonsterAffix.Shielding, "Shielding");
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Shielding);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Add(MonsterAffix.Shielding, new WorldDecoratorCollection(
            new GroundLabelDecorator(Hud)
            {
                BorderBrush = Hud.Render.CreateBrush(128, 255, 0, 0, 2),
                TextFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 255, 255, true, false, false),
                BackgroundBrush = Hud.Render.CreateBrush(255, 255, 25, 0, 0),
            }
            ));
            //##### TELEPORTER #####
            Hud.GetPlugin<EliteMonsterAffixPlugin>().CustomAffixNames.Add(MonsterAffix.Teleporter, "Teleporter");
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Teleporter);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Add(MonsterAffix.Teleporter, new WorldDecoratorCollection(
            new GroundLabelDecorator(Hud)
            {
                BorderBrush = Hud.Render.CreateBrush(128, 255, 0, 0, 2),
                TextFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 255, 255, true, false, false),
                BackgroundBrush = Hud.Render.CreateBrush(255, 255, 25, 0, 0),
            }
            ));
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Thunderstorm);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Vortex);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Waller);
            //##### WORMHOLE #####
            Hud.GetPlugin<EliteMonsterAffixPlugin>().CustomAffixNames.Add(MonsterAffix.Wormhole, "Wormhole");
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Remove(MonsterAffix.Wormhole);
            Hud.GetPlugin<EliteMonsterAffixPlugin>().AffixDecorators.Add(MonsterAffix.Wormhole, new WorldDecoratorCollection(
            new GroundLabelDecorator(Hud)
            {
                BorderBrush = Hud.Render.CreateBrush(128, 255, 0, 0, 2),
                TextFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 255, 255, true, false, false),
                BackgroundBrush = Hud.Render.CreateBrush(255, 255, 25, 0, 0),
            }
            ));
        }

    }

}
