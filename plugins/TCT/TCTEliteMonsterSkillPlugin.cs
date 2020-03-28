//################################################################################
//# ..:: created with TCT Version 5.2 for THUD v7.6 (17.11.20.0) ::.. by RealGsus #
//################################################################################

using Turbo.Plugins.Default;

namespace Turbo.Plugins.TCT
{

    public class TCTEliteMonsterSkillPlugin : BasePlugin, ICustomizer
    {

        public TCTEliteMonsterSkillPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }

        public void Customize()
        {
            Hud.GetPlugin<EliteMonsterSkillPlugin>().PlaguedDecorator.Enabled = false;
        }

    }

}
