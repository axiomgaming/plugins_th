//################################################################################
//# ..:: created with TCT Version 5.2 for THUD v7.6 (17.11.20.0) ::.. by RealGsus #
//################################################################################

using Turbo.Plugins.Default;

namespace Turbo.Plugins.CB
{

    public class MonsterCircleDisableStandardPlugin : BasePlugin, ICustomizer
    {

        public MonsterCircleDisableStandardPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }

        public void Customize()
        {
            Hud.GetPlugin<StandardMonsterPlugin>().EliteLeaderDecorator.Enabled = false;
            Hud.GetPlugin<StandardMonsterPlugin>().EliteMinionDecorator.Enabled = false;
            Hud.GetPlugin<StandardMonsterPlugin>().EliteChampionDecorator.Enabled = false;
            Hud.GetPlugin<StandardMonsterPlugin>().EliteUniqueDecorator.Enabled = false;
            Hud.GetPlugin<StandardMonsterPlugin>().BossDecorator.Enabled = false;
     }

    }

}
