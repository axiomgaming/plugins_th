using Turbo.Plugins.Default;
using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace Turbo.Plugins.TLHP
{
    public class TLHP_ArchonIcon : BasePlugin, IInGameTopPainter
    {
        public bool HideWhenUiIsHidden { get; set; } = false;
        public BuffPainter BuffPainter { get; set; }

        private BuffRuleCalculator _ruleCalculator;

        public TLHP_ArchonIcon()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            BuffPainter = new BuffPainter(Hud, true)
            {
                Opacity = 1.0f,
                TimeLeftFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 255, 255, true, false, 255, 0, 0, 0, true),
            };

            _ruleCalculator = new BuffRuleCalculator(Hud)
            {
                SizeMultiplier = 0.55f
            };

            //_ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 1, MinimumIconCount = 0, DisableName = true }); // Arcane
			
			
			
			_ruleCalculator.Rules.Add(new BuffRule(Hud.Sno.SnoPowers.Wizard_Archon.Sno) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}); 
			_ruleCalculator.Rules.Add(new BuffRule(Hud.Sno.SnoPowers.Wizard_Archon.Sno) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}); 
           
        }


        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip)
                return;
            if (HideWhenUiIsHidden && Hud.Render.UiHidden)
                return;

            foreach (var player in Hud.Game.Players)
            {
                if (!player.HasValidActor)
                    continue;

                var buff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Wizard_Archon.Sno);
                if ((buff == null) || (buff.IconCounts[0] <= 0))
                    continue;
				
				 if (_ruleCalculator.PaintInfoList.Count == 0)
                    return;
                if (!_ruleCalculator.PaintInfoList.Any(info => info.TimeLeft > 0))
                    return;

                for (var i = 0; i < _ruleCalculator.PaintInfoList.Count; i++)
                {
                    var info = _ruleCalculator.PaintInfoList[0];
                    if (info.TimeLeft <= 0)
                    {
                        _ruleCalculator.PaintInfoList.RemoveAt(0);
                        _ruleCalculator.PaintInfoList.Add(info);
                    }
                    else
                    {
                        break;
                    }
                }
				
                var portraitRect = player.PortraitUiElement.Rectangle;

                var x = portraitRect.Right;
                var y = portraitRect.Top + (portraitRect.Height * 0.81f);

                BuffPainter.PaintHorizontal(_ruleCalculator.PaintInfoList, x, y, _ruleCalculator.StandardIconSize, 0);
            }
        }
    }
}