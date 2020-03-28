using System;
using System.Linq;
using SharpDX.DirectInput;
using SharpDX;
using Turbo.Plugins.Default;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

 
namespace Turbo.Plugins.TLHP
{
	public class TLHP_HighGRMobSongsPlugin : BasePlugin, IInGameTopPainter {

		public DAV_SkillPainter SkillPainter { get; set; }
		public IFont ClassFont { get; set; }
		public List<uint> WatchedSnos;
		public float StartXPos { get; set; }
		public float StartYPos { get; set; }
		public float IconSize { get; set; }
		private float _size;
		private float HudWidth { get { return Hud.Window.Size.Width; } }
		private float HudHeight { get { return Hud.Window.Size.Height; } }
		private Dictionary<HeroClass, string> _classShortName;
		
		
	 
		public TLHP_HighGRMobSongsPlugin() {
			Enabled = true;
		}
	   	
		public override void Load(IController hud) {
			base.Load(hud);
		   
			
			StartYPos = 0.840f;  
			StartXPos = 0.055f;   
			IconSize = 0.025f;   
			_size = HudWidth * 0.020f; 
			
			ClassFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, true, false, 255, 0, 0, 0, true);
			SkillPainter = new DAV_SkillPainter(Hud);
			_classShortName = new Dictionary<HeroClass, string> {
				{HeroClass.Barbarian, "\n(Barb)"},
				{HeroClass.Monk, "\n(Monk)"},
				{HeroClass.Necromancer, ""},
				{HeroClass.Wizard, ""},
				{HeroClass.WitchDoctor, "\n(WD)"},
				{HeroClass.Crusader, "\n(Sader)"},
				{HeroClass.DemonHunter, "\n(DH)"}
			};
			
			WatchedSnos = new List<uint> {
				//--- Necromancer
				Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno,
				Hud.Sno.SnoPowers.Necromancer_LandOfTheDead.Sno,
 
				//--- Wizard
				Hud.Sno.SnoPowers.Wizard_Archon.Sno,
			};	 
			
		
		
		}
		
	
		
		public void PaintTopInGame(ClipState clipState) {
			if (clipState != ClipState.BeforeClip) return;
 
			var xPos = HudWidth * StartXPos;
			var index = 0;
			foreach (var player in Hud.Game.Players.OrderBy(p => p.HeroClassDefinition.HeroClass)) {
				var foundCarrySkill = false;
				var flagIsFirstIterator = true;
				var yPos = HudHeight * StartYPos;
			   
				foreach (var skill in player.Powers.UsedSkills.OrderBy(p => p.SnoPower.Sno)) {
					if (skill == null || !WatchedSnos.Contains(skill.SnoPower.Sno)) continue;
				   
					foundCarrySkill = true;
					if (flagIsFirstIterator) {
						var layout = ClassFont.GetTextLayout(player.BattleTagAbovePortrait + _classShortName[player.HeroClassDefinition.HeroClass]);
						ClassFont.DrawText(layout, xPos, yPos);
						flagIsFirstIterator = false;
						yPos += 0.035f * HudHeight;
					}
				   
					var rect = new SharpDX.RectangleF(xPos, yPos, _size, _size);
					SkillPainter.Paint(skill, rect);
					yPos += _size*1.1f;
				}
			   
				var CheatDeathBuff = player.Powers.GetBuff(Hud.Sno.SnoPowers.WitchDoctor_Passive_SpiritVessel.Sno);
				switch (player.HeroClassDefinition.HeroClass) {
					case HeroClass.Crusader: CheatDeathBuff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Crusader_Passive_Indestructible.Sno); break;
					case HeroClass.Barbarian: CheatDeathBuff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Barbarian_Passive_NervesOfSteel.Sno); break;
					case HeroClass.Monk: CheatDeathBuff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Monk_Passive_NearDeathExperience.Sno); break;
					case HeroClass.Necromancer: CheatDeathBuff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Necromancer_Passive_FinalService.Sno); break;
					case HeroClass.DemonHunter: CheatDeathBuff = player.Powers.GetBuff(Hud.Sno.SnoPowers.DemonHunter_Passive_Awareness.Sno); break;
					case HeroClass.Wizard: CheatDeathBuff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Wizard_Passive_UnstableAnomaly.Sno); break;
				}
			   
				if (CheatDeathBuff != null && CheatDeathBuff.TimeLeftSeconds[1] > 0.0) {
					if (flagIsFirstIterator) {
						var layout = ClassFont.GetTextLayout(player.BattleTagAbovePortrait + _classShortName[player.HeroClassDefinition.HeroClass]);
						ClassFont.DrawText(layout, xPos, yPos);
						foundCarrySkill = true;
						yPos += 0.03f * HudHeight;
					}
					var Texture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.WitchDoctor_Passive_SpiritVessel.Icons[1].TextureId);
					switch (player.HeroClassDefinition.HeroClass) {
						case HeroClass.Crusader: Texture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.Crusader_Passive_Indestructible.Icons[1].TextureId); break;
						case HeroClass.Barbarian: Texture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.Barbarian_Passive_NervesOfSteel.Icons[1].TextureId); break;
						case HeroClass.Monk: Texture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.Monk_Passive_NearDeathExperience.Icons[1].TextureId); break;
						case HeroClass.Necromancer: Texture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.Necromancer_Passive_FinalService.Icons[1].TextureId); break;
						case HeroClass.DemonHunter: Texture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.DemonHunter_Passive_Awareness.Icons[1].TextureId); break;
						case HeroClass.Wizard: Texture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.Wizard_Passive_UnstableAnomaly.Icons[1].TextureId); break;
					}
					if (Texture != null) Texture.Draw(xPos, yPos, _size, _size);
					var layout2 = ClassFont.GetTextLayout(CheatDeathBuff.TimeLeftSeconds[1].ToString("0"));
					ClassFont.DrawText(layout2, xPos + (_size - (float)Math.Ceiling(layout2.Metrics.Width)) / 2, yPos + (_size - layout2.Metrics.Height) / 2);
				}
			   
				if (foundCarrySkill) xPos += _size * 1.51f;  // base 2.5f
				index++;
			}
		}
	}
   

	
	public class DAV_SkillPainter : ITransparentCollection {
		public bool Enabled { get; set; }
		public IController Hud { get; set; }
 
		public IFont CDFont { get; set; }
		public IFont BuffFont { get; set; }

        private bool Dead;
		//public SharpDX.DirectInput.Key Key { get; set; }
		
		private string chatEditLine = "Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline";
		private static System.Timers.Timer ReadEditLineTimer;
		
			bool Pack1 = false;
			bool Pack2 = true;
			bool Pack3 = false;
			bool Pack4 = false;
			bool Pack5 = false;
			bool Pack6 = false;
			
		
		public DAV_SkillPainter(IController hud) {
			Hud = hud;
			Enabled = true;
			Dead = false;
			CDFont = Hud.Render.CreateFont("arial", 9, 255, 255, 255, 255, true, false, 255, 0, 0, 0, true);
			BuffFont = Hud.Render.CreateFont("arial", 9, 255, 51, 255, 51, true, false, 255, 0, 0, 0, true);
			
			
			ReadEditLineTimer = new System.Timers.Timer();
			ReadEditLineTimer.Interval = 500;		// edit line filtering interval
			ReadEditLineTimer.Elapsed += ReadEditLine;
			ReadEditLineTimer.AutoReset = true;
			ReadEditLineTimer.Enabled = true;
		
			
			
			
			//Key = SharpDX.DirectInput.Key.NumberPad1;
			
		
		}
		
		// public void OnKeyEvent(IKeyEvent keyEvent)
        // {
            			
			// if (keyEvent.Key == Key && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            // {
			
				// pack1(false);
				// pack2(false);
				// pack3(false);
				// pack4(false);
				// pack5(true);
				// pack6(false);
				
				// try
                // {
                   // var soundPlayer = Hud.Sound.LoadSoundPlayer("pack5.wav");
                   // soundPlayer.Play();
                // }
				// catch (Exception){}
				
			
			// }
		// }
		
		public void ReadEditLine(Object source, System.Timers.ElapsedEventArgs e)
		{
			if (!Hud.Render.GetUiElement(chatEditLine).Visible)
        			return;
				
			string lineStr = Hud.Render.GetUiElement(chatEditLine).ReadText(System.Text.Encoding.UTF8, false).Trim();	// if error, change "UTF8" with "Default"...not tested though
        	lineStr = lineStr.Trim().ToLower();
			Match p1match = Regex.Match(lineStr, @"(?=/idol/)") ;  // .+(?=/)
			Match p2match = Regex.Match(lineStr, @"(?=/noidol/)") ;
			Match p3match = Regex.Match(lineStr, @"(?=/pio/)") ;
			Match p4match = Regex.Match(lineStr, @"(?=/noble/)") ;
			Match p5match = Regex.Match(lineStr, @"(?=/mixed/)") ;
			Match p6match = Regex.Match(lineStr, @"(?=/simple/)") ;
			Match p7match = Regex.Match(lineStr, @"(?=/none/)") ;
			
			
			if (p1match.Success)
			{
				pack1(true);
				pack2(false);
				pack3(false);
				pack4(false);
				pack5(false);
				pack6(false);
							
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("pack1.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
		
			if (p2match.Success)
			{
				pack1(false);
				pack2(true);
				pack3(false);
				pack4(false);
				pack5(false);
				pack6(false);
							
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("pack2.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
			
			if (p3match.Success)
			{
				pack1(false);
				pack2(false);
				pack3(true);
				pack4(false);
				pack5(false);
				pack6(false);
							
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("pack3.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
			
			if (p4match.Success)
			{
				pack1(false);
				pack2(false);
				pack3(false);
				pack4(true);
				pack5(false);
				pack6(false);
							
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("pack4.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
			
			if (p5match.Success)
			{
				pack1(false);
				pack2(false);
				pack3(false);
				pack4(false);
				pack5(true);
				pack6(false);
							
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("pack5.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
			
			if (p6match.Success)
			{
				pack1(false);
				pack2(false);
				pack3(false);
				pack4(false);
				pack5(false);
				pack6(true);
							
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("pack6.wav");  //pack6
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
			
			if (p7match.Success)
			{
				pack1(false);
				pack2(false);
				pack3(false);
				pack4(false);
				pack5(false);
				pack6(false);
							
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("aoff.wav");  //none
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
	    }
		
		
		public void Paint(IPlayerSkill skill, RectangleF rect) {
			if (skill == null) return;

			var texture = Hud.Texture.GetTexture(skill.SnoPower.NormalIconTextureId);
			if (texture != null) texture.Draw(rect.X, rect.Y, rect.Width, rect.Height);
 
			if (skill.BuffIsActive) {
				var ActiveTime = 0.0d;
				for (var i = 2; i >= 0; i--) {
					ActiveTime = skill.Buff.TimeLeftSeconds[i];
					if (ActiveTime > 0)
						break;
				}
			   
				if (ActiveTime > 0) {
					var bufftext = ActiveTime.ToString(ActiveTime > 3.0f ? "F0" : "F1", CultureInfo.InvariantCulture);
					var bufftextLayout = BuffFont.GetTextLayout(bufftext);
					BuffFont.DrawText(bufftextLayout, rect.X, rect.Y);
				}
			}

			int currentGrLevel = (int) Hud.Game.Me.InGreaterRiftRank;
            
			int monsterCount = Hud.Game.AliveMonsters.Count(m => m.SnoMonster.Priority == MonsterPriority.boss);
                
			var monsters = Hud.Game.AliveMonsters;
			
			int totalAliveMonsters = 0;
         
			var alive = monsters.ToList();

			var center = Hud.Window.CreateWorldCoordinate(0, 0, 0);

			var n = 0;
			var remainingSeconds = (skill.CooldownFinishTick - Hud.Game.CurrentGameTick) / 60.0d;
			
			center.Set(center.X / n, center.Y / n, center.Z / n);
			var centerScreenCoordinate = center.ToScreenCoordinate(false);
			var y = centerScreenCoordinate.Y;
			
			if (alive.Any(x => x.FloorCoordinate.IsOnScreen()))
			{
				foreach (var monster in alive.Where(x => x.FloorCoordinate.IsOnScreen()))
				{
					// Change the center positioning of the current cooridnate
					center.Add(monster.FloorCoordinate);
					
					totalAliveMonsters++;
					n++;
				}
			}
			else
			{
				foreach (var monster in alive)
				{
					center.Add(monster.FloorCoordinate);
					
					totalAliveMonsters++;
					n++;
				}
			}

			int music1 = oddNumberGenerator(1, 115); 
			int music2 = oddNumberGenerator(115, 295); 
			int music3 = oddNumberGenerator(115, 207); 
			int music4 = oddNumberGenerator(209, 239); 
			int music5 = oddNumberGenerator(1, 295);
			int music6 = 1501;
			
			if (skill != null && skill.SnoPower.Sno == Hud.Sno.SnoPowers.Wizard_Archon.Sno && skill.BuffIsActive) {
				var ActiveTime = skill.Buff.TimeLeftSeconds[2];
				
                if (ActiveTime > 0)
                    remainingSeconds = skill.CalculateCooldown(skill.Rune == 3.0 ? 100 : 120) - 20 + ActiveTime;
				
				if(Hud.Game.SpecialArea == SpecialArea.GreaterRift && currentGrLevel >= 130 && monsterCount != 1 && !checkWizardDeath() && totalAliveMonsters >= 2 && totalAliveMonsters <= 99 && Pack1) {
                	PlayWizSongs(ActiveTime, music1, false);
				}
					
				if(Hud.Game.SpecialArea == SpecialArea.GreaterRift && currentGrLevel >= 130 && monsterCount != 1 && !checkWizardDeath() && totalAliveMonsters >= 2 && totalAliveMonsters <= 99 && Pack2) {
	                PlayWizSongs(ActiveTime, music2, false);	
				}
					
				if(Hud.Game.SpecialArea == SpecialArea.GreaterRift && currentGrLevel >= 130 && monsterCount != 1 && !checkWizardDeath() && totalAliveMonsters >= 2 && totalAliveMonsters <= 99 && Pack3) {
	            	PlayWizSongs(ActiveTime, music3, false);
				}
	
				if(Hud.Game.SpecialArea == SpecialArea.GreaterRift && currentGrLevel >= 130 && monsterCount != 1 && !checkWizardDeath() && totalAliveMonsters >= 2 && totalAliveMonsters <= 99 && Pack4) {
	            	PlayWizSongs(ActiveTime, music4, false);	
				}
					
				if(Hud.Game.SpecialArea == SpecialArea.GreaterRift && currentGrLevel >= 130 && monsterCount != 1 && !checkWizardDeath() && totalAliveMonsters >= 2 && totalAliveMonsters <= 99 && Pack5) {
	            	PlayWizSongs(ActiveTime, music5, false);
				}	
				
				if(Hud.Game.SpecialArea == SpecialArea.GreaterRift && currentGrLevel >= 130 && monsterCount != 1 && !checkWizardDeath() && totalAliveMonsters >= 2 && totalAliveMonsters <= 99 && Pack6) {
	            	PlayWizSongs(ActiveTime, music6, false);
				}
				else if(Hud.Game.SpecialArea == SpecialArea.GreaterRift && currentGrLevel >= 130 && monsterCount != 1 && !checkWizardDeath() && totalAliveMonsters >= 100) {
	            	PlayWizSongs(ActiveTime, music5, true);
				}
				else if (Hud.Game.SpecialArea == SpecialArea.GreaterRift && currentGrLevel >= 130 && monsterCount != 1 && checkWizardDeath()){
	            	try {
	            		var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
	            		soundPlayer.Play();
	            	}
	            	catch (Exception){
		            }
				}
			
			} else if (skill != null && skill.SnoPower.Sno == Hud.Sno.SnoPowers.Wizard_Archon.Sno) {
				if(checkWizardDeath() && Hud.Game.SpecialArea == SpecialArea.GreaterRift && currentGrLevel >= 130) {
            		try {
                		var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                		soundPlayer.Play();
                	}
                	catch (Exception){
		            }
				}
			}
				
            if (remainingSeconds <= 0) return;
			   
			var text = remainingSeconds.ToString(remainingSeconds > 3.0f ? "F0" : "F1", CultureInfo.InvariantCulture);
			var textLayout = CDFont.GetTextLayout(text);
			var xp = rect.X + rect.Width - (float)Math.Ceiling(textLayout.Metrics.Width);
			var yp = rect.Y + rect.Height - textLayout.Metrics.Height;
			CDFont.DrawText(textLayout, xp, yp);
		}

		private int oddNumberGenerator(int start, int end) {  //idol
			Random random = new Random();
			int randomNumber = random.Next(start, end);
			if (randomNumber % 2 == 1) return randomNumber;
            else
            {
                if (randomNumber + 1 <= (end - 2))
                    return randomNumber + 1;
                else if (randomNumber - 1 >= start)
                    return randomNumber - 1;
                else return 1;
            }
		}

		private bool pack1 (bool Pack1check) 
		{
			if(Pack1check)
			{
				if(!Pack1) {
					Pack1 = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(Pack1) {
					Pack1 = false;
				}
				return false;
			}
		}
		
		private bool pack2 (bool Pack2check) 
		{
			if(Pack2check)
			{
				if(!Pack2) {
					Pack2 = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(Pack2) {
					Pack2 = false;
				}
				return false;
			}
		}
		
		private bool pack3 (bool Pack3check) 
		{
			if(Pack3check)
			{
				if(!Pack3) {
					Pack3 = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(Pack3) {
					Pack3 = false;
				}
				return false;
			}
		}
		
		private bool pack4 (bool Pack4check) 
		{
			if(Pack4check)
			{
				if(!Pack4) {
					Pack4 = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(Pack4) {
					Pack4 = false;
				}
				return false;
			}
		}
		
		private bool pack5 (bool Pack5check) 
		{
			if(Pack5check)
			{
				if(!Pack5) {
					Pack5 = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(Pack5) {
					Pack5 = false;
				}
				return false;
			}
		}
		
		private bool pack6 (bool Pack6check) 
		{
			if(Pack6check)
			{
				if(!Pack6) {
					Pack6 = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(Pack6) {
					Pack6 = false;
				}
				return false;
			}
		}
		
		private bool checkWizardDeath() {
       	foreach (var player in Hud.Game.Players) {
               if(player.HeroClassDefinition.HeroClass == HeroClass.Wizard) {
               	if(PlayerDeadTime(player)) {
                		return true;
               	} 
                }
            }
           return false;
		}
		
		private bool PlayerDeadTime(IPlayer player)
        {
            if (player.IsDead)   
            {
                if (Dead == false)
                {
                    Dead = true;
                    return true;
                }
               	return false;
            }
            else
            {
                if (Dead)
                {
                    Dead = false;
                }

                return false;
            }
        }
			
		private void PlayWizSongs (double ActiveTime, int musicNumber, bool handsClapCheck) {
			if (Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Wizard) {
            	if(ActiveTime == 10.0) {
                    try{
                    	if(checkWizardDeath()) {
                    		var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("baseone.wav");
                        	soundPlayer.Play();
                        }
                    }
                    catch (Exception){
                    }
            	} else if (ActiveTime == 9.0) {
                    try{
                        if(checkWizardDeath()) {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("basetwo.wav");
                        	soundPlayer.Play();
                        }
                    }
                    catch (Exception){
                    }
                } else if (ActiveTime == 8.0) {
                    try{
                        if(checkWizardDeath()) {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("basethree.wav");
                        	soundPlayer.Play();
                        }
                    }
                    catch (Exception){
                    }
				} else if (ActiveTime == 2.0) {
                    try{
                        if(checkWizardDeath()) {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
                        	if(!handsClapCheck) {
                        		var soundPlayer = Hud.Sound.LoadSoundPlayer(musicNumber + ".wav");
                        		soundPlayer.Play();
                        	} else {
                        		var soundPlayer = Hud.Sound.LoadSoundPlayer("gg1.wav");
                        		soundPlayer.Play();
                        	}	
                        }
                    }
                    catch (Exception){
                    }
				} else if (ActiveTime == 1.0) {
                    try{
                        if(checkWizardDeath()) {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
                        	if(!handsClapCheck) {
                        		var soundPlayer = Hud.Sound.LoadSoundPlayer((musicNumber + 1) + ".wav");
                        		soundPlayer.Play();
                        	} else {
                        		var soundPlayer = Hud.Sound.LoadSoundPlayer("gg2.wav");
                        		soundPlayer.Play();
                        	}
                        }
                    }
                    catch (Exception){
                    }
				} else {
            		try {
						if(checkWizardDeath()) {
                    		var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                    	}
            		} catch (Exception){
                    }	
            	}
            } else { //not wizard
                if (ActiveTime == 10.0) {
                    try{
                    	if(checkWizardDeath()) {
                    		var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("baseone.wav");
                        	soundPlayer.Play();
                        }
                    }
                    catch (Exception){
                    }
                } else if (ActiveTime == 9.0) {
                    try{
                        if(checkWizardDeath()) {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("basetwo.wav");
                        	soundPlayer.Play();
                        }
                    }
                    catch (Exception){
                    }
                } else if (ActiveTime == 8.0) {
                    try{
                        if(checkWizardDeath()) {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("basethree.wav");
                        	soundPlayer.Play();
                        }
                    }
                    catch (Exception){
                    }
                } else if (ActiveTime == 7.0) {
                    try{
                        if(checkWizardDeath()) {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("basefour.wav");
                        	soundPlayer.Play();
                        }
                    }
                    catch (Exception){
                    }
                } else if (ActiveTime == 6.0) {
                    try{
                        if(checkWizardDeath()) {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("basefive.wav");
                        	soundPlayer.Play();
                        }
                    }
                    catch (Exception){
                    }
                } else if (ActiveTime == 5.0) {
                    try{
                        if(checkWizardDeath()) {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("basesix.wav");
                        	soundPlayer.Play();
                        }
                    }
                    catch (Exception){
                    }
                } else if (ActiveTime == 4.0) {
                    try{
                        if(checkWizardDeath()) {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("baseseven.wav");
                        	soundPlayer.Play();
                        }
                    }
                    catch (Exception){
                    }
                } else if (ActiveTime == 3.0) {
                    try{
                        if(checkWizardDeath()) {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("baseeight.wav");
                        	soundPlayer.Play();
                        }
                    }
                    catch (Exception){
                    }
                } else if (ActiveTime == 2.0) {
                    try{
                        if(checkWizardDeath()) {
                        	var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
							if(!handsClapCheck) {
                        		var soundPlayer = Hud.Sound.LoadSoundPlayer(musicNumber + ".wav");
                        		soundPlayer.Play();
                        	} else {
                        		var soundPlayer = Hud.Sound.LoadSoundPlayer("gg1.wav");
                        		soundPlayer.Play();
                        	}	
                        }
                    }
                    catch (Exception){
                    }
                } else if (ActiveTime == 1.0) {
                    try{
			           	if(checkWizardDeath()) {
			           		var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                        } else {
                        	if(!handsClapCheck) {
                        		var soundPlayer = Hud.Sound.LoadSoundPlayer((musicNumber + 1) + ".wav");
                        		soundPlayer.Play();
                        	} else {
                        		var soundPlayer = Hud.Sound.LoadSoundPlayer("gg2.wav");
                        		soundPlayer.Play();
                        	}
                        }
                    }
                    catch (Exception){
                    }
				} else {
                	if(checkWizardDeath()) {
                		try{
                			var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        	soundPlayer.Play();
                		} catch(Exception){
                		}
                    }
                }
        	} 
		}

		public IEnumerable<ITransparent> GetTransparents() {
			yield return CDFont;
			yield return BuffFont;
		}
	}
}