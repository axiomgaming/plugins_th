using System;
using System.Linq;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.TLHP
{
    public class WizardBuffCustomizerPlugin : BasePlugin, ICustomizer
    {
        public WizardBuffCustomizerPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }

        public void Customize()
        {
            Hud.GetPlugin<PlayerTopBuffListPlugin>().BuffPainter.TimeLeftFont = Hud.Render.CreateFont("tahoma", 12, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true);
            Hud.GetPlugin<PlayerTopBuffListPlugin>().BuffPainter.StackFont = Hud.Render.CreateFont("tahoma", 12, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true);
            Hud.GetPlugin<PlayerTopBuffListPlugin>().BuffPainter.ShowTooltips = true;
            Hud.GetPlugin<PlayerTopBuffListPlugin>().PositionOffset = -0.26f;
            Hud.GetPlugin<PlayerTopBuffListPlugin>().RuleCalculator.SizeMultiplier = 1.0f;

            // Hud.GetPlugin<PlayerTopBuffListPlugin>().RuleCalculator.Rules.Add(new BuffRule(134456) { IconIndex = 5, MinimumIconCount = 0, ShowStacks = false, ShowTimeLeft = false }); // Arcane Torrent
            // Hud.GetPlugin<PlayerTopBuffListPlugin>().RuleCalculator.Rules.Add(new BuffRule(134872) { IconIndex = 5, MinimumIconCount = 0, ShowTimeLeft = false, ShowStacks = true }); // Archon
            // Hud.GetPlugin<PlayerTopBuffListPlugin>().RuleCalculator.Rules.Add(new BuffRule(134872) { IconIndex = 6, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true }); // Swami Archon Stacks
            Hud.GetPlugin<PlayerTopBuffListPlugin>().RuleCalculator.Rules.Add(new BuffRule(208823) { IconIndex = 1, MinimumIconCount = 1, ShowStacks = true, ShowTimeLeft = false }); // Arcane Dynamo
	    
			Hud.GetPlugin<PlayerTopBuffListPlugin>().RuleCalculator.Rules.Add(new BuffRule(483552) { IconIndex = 1, MinimumIconCount = 5, ShowStacks = true, ShowTimeLeft = true }); // Squirt's Necklace

            // Hud.GetPlugin<PlayerTopBuffListPlugin>().RuleCalculator.Rules.Add(new BuffRule(243141) { IconIndex = 2, MinimumIconCount = 1, ShowStacks = true, ShowTimeLeft = false }); // BlackHole
            // Hud.GetPlugin<PlayerTopBuffListPlugin>().RuleCalculator.Rules.Add(new BuffRule(30796) { IconIndex = 3, MinimumIconCount = 1, ShowStacks = true, ShowTimeLeft = false }); // Wafe of Force
            // Hud.GetPlugin<PlayerTopBuffListPlugin>().RuleCalculator.Rules.Add(new BuffRule(359581) { IconIndex = 7, MinimumIconCount = 1, ShowStacks = true, ShowTimeLeft = false }); // Firebird's Finery 6set
            // Hud.GetPlugin<PlayerTopBuffListPlugin>().RuleCalculator.Rules.Add(new BuffRule(74499) { IconIndex = 4, MinimumIconCount = 1, ShowStacks = false, ShowTimeLeft = true }); // Halo Of Karini
            // Hud.GetPlugin<PlayerTopBuffListPlugin>().RuleCalculator.Rules.Add(new BuffRule(91549) { IconIndex = 5, MinimumIconCount = 0, ShowStacks = false, ShowTimeLeft = false }); // Disintegrate
            // Hud.GetPlugin<PlayerTopBuffListPlugin>().RuleCalculator.Rules.Add(new BuffRule(93395) { IconIndex = 5, MinimumIconCount = 0, ShowStacks = false, ShowTimeLeft = false }); // Ray of Frost
         }
	}
}