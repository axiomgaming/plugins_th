// https://www.ownedcore.com/forums/diablo-3/turbohud/turbohud-community-plugins/800417-v9-0-eng-rnn-mybuffs.html  (Forum Post)
// https://pastebin.com/edit/TrzteSj0 (Download link)
// Thanks to JarJarD3 for his many contributions to this plugin.

using System.Collections.Generic;
using System.Linq;
using SharpDX.DirectInput;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.RNN
{
    public class MyBuffs : BasePlugin, IInGameTopPainter, IKeyEventHandler, INewAreaHandler
    {
        public IFont FuenteW { get; set; }
        public IFont FuenteY { get; set; }
        public IFont FuenteG { get; set; }
        public IFont FuenteB { get; set; }
        public int info { get; set; } = 1;

        public float XCoord { get; set; } = 300f;
        public float YCoord { get; set; } = 14f;
        public float LineHeightPC { get; set; } = 0.85f; // Line height modifier.

        public bool Debug_On { get; set; }
        public IKeyEvent ToggleKeyEvent1 { get; set; }  // Show/Hide.
        public IKeyEvent ToggleKeyEvent2 { get; set; }  // Display info mode.
        public IKeyEvent ToggleKeyEvent3 { get; set; }  // Reposition to mouse cursor.

		public bool ShowBuffsExcluded  { get; set; }
		
        public Dictionary<uint, int> FixIndexSkills { get; set; }

        public uint[] ExcludedSnos { get; set; } = new uint[]
        {
            220304,     // ActorInTownBuff
            439438,     // ActorInvulBuff
            212032,     // ActorLoadingBuff
            134334,     // Banter_Cooldown
            30145,      // BareHandedPassive
            225599,     // CannotDieDuringBuff
            134225,     // Callout_Cooldown
            30176,      // Cooldown
            428398,     // Cosmetic_SpectralHound_Buff
            193438,     // DualWieldBuff
            257687,     // Enchantress_MissileWard
            286747,     // g_paragonBuff
            30283,      // ImmuneToFearDuringBuff
            30284,      // ImmuneToRootDuringBuff
            30285,      // ImmuneToSnareDuringBuff
            30286,      // ImmuneToStunDuringBuff
            30290,      // InvulnerableDuringBuff
            349060,     // TeleportToWaypoint
            371141,     // TeleportToWaypoint_Cast
            79486,      // UninterruptibleDuringBuff
            30582,      // UntargetableDuringBuff
            30584,      // UseHealthGlyph
            132910,     // WarpInMagical
            223604,     // WorldCreatingBuff
        };

        public MyBuffs()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            Order = 30001;

			ShowBuffsExcluded = false;
			
            ToggleKeyEvent1 = Hud.Input.CreateKeyEvent(true, Key.F7, controlPressed: false, altPressed: false, shiftPressed: true);
            ToggleKeyEvent2 = Hud.Input.CreateKeyEvent(true, Key.F7, controlPressed: true, altPressed: false, shiftPressed: false);
            ToggleKeyEvent3 = Hud.Input.CreateKeyEvent(true, Key.F7, controlPressed: false, altPressed: true, shiftPressed: false);
            Debug_On = false;

            FuenteW = Hud.Render.CreateFont("consolas", 8f, 255, 200, 200, 200, false, false, 220, 32, 32, 32, false);
            FuenteY = Hud.Render.CreateFont("consolas", 9f, 255, 255, 255, 0, false, false, 220, 32, 32, 32, false);
            FuenteG = Hud.Render.CreateFont("consolas", 9f, 255, 0, 255, 0, false, false, 220, 32, 32, 32, false);
			FuenteB = Hud.Render.CreateFont("consolas", 9f, 255, 0, 128, 255, false, false, 220, 32, 32, 32, false); //FuenteB = Hud.Render.CreateFont("consolas", 9f, 255, 0, 128, 255, true, false, 220, 32, 32, 32, true);
			
            FixIndexSkills = new Dictionary<uint, int>	// Skill Default Index (info = 4). 
			{											//Por defecto se muestran los tiempos que correspondan al indice con mayor Timeleft , aquí se puede fijar un indice concreto para la habilidad
				{79528, 0}    					// Skill Ignore Pain, use index 0 always
			};
        }

        public void OnNewArea(bool newGame, ISnoArea area)
        {
            if (newGame)
            {
                Debug_On = false;
            }
        }

        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (keyEvent.IsPressed)
            {
                if (ToggleKeyEvent1.Matches(keyEvent))
                {
                    Debug_On = !Debug_On;
                }
                else if (ToggleKeyEvent2.Matches(keyEvent))
                {
                    if (Debug_On)
                    {
                        if (++info > 5)
                        {
                            info = 1;
                        }
						else if ((info == 4) && !ShowBuffsExcluded)
						{
							info = 5;
						}
                    }
                }
                else if (ToggleKeyEvent3.Matches(keyEvent))
                {
                    if (Debug_On)
                    {
                        var mouse = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY);
                        XCoord = mouse.X;
                        YCoord = mouse.Y;
                    }
                }
            }
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (Hud.Render.UiHidden)
                return;
            if (clipState != ClipState.BeforeClip)
                return;
            if (!Hud.Game.IsInGame || !Debug_On)
                return;

            var texto = string.Empty;
            var indice = 1;
            var xPos = XCoord;
            var yPos = YCoord;
            if (info < 5)
            {
				List<IBuff> powers = new List<IBuff>();
				if (info == 4)	powers = Hud.Game.Me.Powers.AllBuffs.Where(x => ExcludedSnos.Contains(x.SnoPower.Sno)).ToList();
				else powers = Hud.Game.Me.Powers.AllBuffs.Where(x => (x.Active || info == 1) && !ExcludedSnos.Contains(x.SnoPower.Sno)).ToList();	
                powers.Sort((a, b) => a.SnoPower.Code.CompareTo(b.SnoPower.Code));
                texto = "BUFFs " + ((info == 1) ? " <All> " : ((info == 2) ? " <Actives> " : ((info == 3) ? " <TimeLeft> " : " <Excluded> "))) + " (Shift+F7 = Show/Hide , Ctrl+F7 = BUFFs All/Actives/Timeleft" + (ShowBuffsExcluded? "/Excluded":"") + " and SKILLs) ";
                var layout = FuenteW.GetTextLayout(texto);
                FuenteW.DrawText(layout, xPos, yPos);
                yPos += layout.Metrics.Height * LineHeightPC;
                texto = "xx)     Sno   Active                        Code (Namelocalized)                          [index] Timeleft/Duration  ";
                FuenteW.DrawText(FuenteW.GetTextLayout(texto), xPos, yPos);
                yPos += layout.Metrics.Height * LineHeightPC;
                foreach (var power in powers)
                {
                    texto = string.Format("{0:00}) {1,7}  {2,-6} {3,-37}  {4,-21}",
						indice, 
						power.SnoPower.Sno, 
						power.Active, 
						power.SnoPower.Code,
						(power.SnoPower.NameLocalized != null)? "(" + power.SnoPower.NameLocalized + ")":""
						);
						;
                    var t = string.Empty;
                    var j = power.TimeLeftSeconds.Count();
                    var color = false;
                    for (var i = 0; i < j; i++)
                    {
                        if (power.TimeLeftSeconds[i] > 0)
                        {
                            t = string.Format("{0} [{1}] {2:F1}/{3:F1}   ", t, i, power.TimeLeftSeconds[i], power.TimeLeftSeconds[i] + power.TimeElapsedSeconds[i]);
                            color = true;
                        }
                    }
                    texto = texto + t;
					IFont font = null;
                    if (color)
                        font = FuenteG;
                    else if (info != 3)
                        font = FuenteY;
                    else
                        continue;
					layout = font.GetTextLayout(texto);
					font.DrawText(layout, xPos, yPos);
                    yPos += layout.Metrics.Height * LineHeightPC;
                    indice++;
                }
            }
            else if (info == 5)
            {
                texto = "SKILLs " + " (Shift+F7 = Show/Hide , Ctrl+F7 = BUFFs All/Active/Timeleft" + (ShowBuffsExcluded? "/Excluded":"") + " and SKILLs)";
                var layout = FuenteW.GetTextLayout(texto);
                FuenteW.DrawText(layout, xPos, yPos);
                yPos += layout.Metrics.Height * LineHeightPC;
                texto = "[Key] CurrentSno BuffActive InCoolDown                Code  (NameLocalized)                          Rune  (NameLocalized)        [Index] Timeleft (higher)";
                FuenteW.DrawText(FuenteW.GetTextLayout(texto), xPos, yPos);
                yPos += layout.Metrics.Height * LineHeightPC;
                foreach (var skill in Hud.Game.Me.Powers.CurrentSkills)
                {
                    texto = string.Empty;
                    double TimeLeftSec = 0;
                    if (skill.BuffIsActive)
                    {
                        if (FixIndexSkills.TryGetValue(skill.CurrentSnoPower.Sno, out var index))
                        {
                            TimeLeftSec = skill.Buff.TimeLeftSeconds[index];
                        }
                        else
                        {
                            for (var i = 0; i < skill.Buff.TimeLeftSeconds.Length; i++)
                            {
                                if (skill.Buff.TimeLeftSeconds[i] > TimeLeftSec)
                                {
                                    TimeLeftSec = skill.Buff.TimeLeftSeconds[i];
                                    index = i;
                                }
                            }
                        }

                        if (TimeLeftSec >= 0)
                        {
                            if (TimeLeftSec < 1.0f)
                            {
                                texto = string.Format("{0:N1}", TimeLeftSec);
                            }
                            else
                            {
                                TimeLeftSec = (int)(TimeLeftSec + 0.80);  // Redondeará a X si es menor  a X.20
                                texto = (TimeLeftSec < 60) ? string.Format("{0:0}", TimeLeftSec) : string.Format("{0:0}:{1:00}", (int)(TimeLeftSec / 60), TimeLeftSec % 60);
                            }
                        }
                        texto = " [" + index + "] " + texto;
                    }
                    texto = string.Format("  {0,-2} {1,8}    {7,-6}    {2,-6}   {3,-29} {4,-22} {5,3} {6,-22} {8}", 
                        skill.Key.ToString().Replace("Skill", "").Substring(0, 1), 
                        skill.CurrentSnoPower.Sno,
                        skill.IsOnCooldown, 
                        skill.CurrentSnoPower.Code, 
                        "(" + skill.CurrentSnoPower.NameLocalized + ")", 
                        skill.Rune, 
                        "(" + skill.RuneNameLocalized + ")", 
                        skill.BuffIsActive, 
                        texto);
					layout = FuenteB.GetTextLayout(texto);
                    FuenteB.DrawText(layout, xPos, yPos);
                    yPos += layout.Metrics.Height * LineHeightPC;
                    indice++;
                }
				
				texto = "Passive    Sno              Code  (NameLocalized) ";
				layout = FuenteW.GetTextLayout(texto);
				yPos += layout.Metrics.Height * LineHeightPC;
                FuenteW.DrawText(layout, xPos, yPos);
				yPos += layout.Metrics.Height * LineHeightPC;
				indice = 1;       
				foreach (var power in Hud.Game.Me.Powers.UsedPassives)
				{				
					texto = string.Format("{0,3}   {1,8}   {2,-40}  {3,-21}",
						indice, 
						power.Sno, 
						power.Code,
						(power.NameLocalized != null)? "(" + power.NameLocalized + ")":""
						);
					layout = FuenteB.GetTextLayout(texto);
					FuenteB.DrawText(layout, xPos, yPos);
					yPos += layout.Metrics.Height * LineHeightPC;
                    indice++;	
				}
            }
        }
    }
}