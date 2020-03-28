// http://www.ownedcore.com/forums/diablo-3/turbohud/turbohud-plugin-review-zone/635586-v7-3-english-glq-baneofthestrickenplugin.html
// edited by Tommy (not for sharing or profit)
 
using Turbo.Plugins.Default;
using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SharpDX.DirectInput;

 
namespace Turbo.Plugins.TLHP
{
    public class TLHP_BaneOfTheStrickenAndNecromancerRGKSongPlugin : BasePlugin, IInGameTopPainter, INewAreaHandler, IKeyEventHandler
    {
        public bool DrawClassOnIcons { get; set; }      //Draw ClassNames per Dafault on every Stricken Icon
       
        public IFont TextFont { get; set; }
        private int[] StackCount { get; set; }
        private bool[] cooling { get; set; }
        public TopLabelDecorator StackCountDecorator { get; set; }
        public float xPos { get; set; }
        public float XWidth { get; set; }
        public float YHeight { get; set; }
        private IBrush StackBrush;
        private Dictionary<HeroClass, string> classShorts;
        public IFont ClassFont { get; set; }
        public bool Dead;
        public bool CheatDeath;
		//public IKeyEvent ToggleKeyEvent { get; set; }
		//public bool ToggleEnable { get; set; }
		public SharpDX.DirectInput.Key Key { get; set; }
		public SharpDX.DirectInput.Key Key2 { get; set; }
		public SharpDX.DirectInput.Key Key3 { get; set; }
		public SharpDX.DirectInput.Key Key4 { get; set; }
		public SharpDX.DirectInput.Key Key5 { get; set; }
		
		public SharpDX.DirectInput.Key Key6 { get; set; }
		public SharpDX.DirectInput.Key Key7 { get; set; }
		public SharpDX.DirectInput.Key Key8 { get; set; }
		public SharpDX.DirectInput.Key Key9 { get; set; }
		public SharpDX.DirectInput.Key Key10 { get; set; }
		public SharpDX.DirectInput.Key Key11 { get; set; }


		private readonly int[] _skillOrder = { 2, 3, 4, 5, 0, 1 };
		//bool LotDOnCooldown = false;
        bool LotDBuffActive = false;
		bool LotDBuffDiminish = false;
		bool SimBuffActive = false;
		//bool SimOnCooldown = false;
		bool PoisonConventionActive = false;
		bool PhysicalConventionActive = false;
		bool ColdConventionActive = false;
		bool fail = false;
		bool success = false;
		bool bossIsAlive = false;
		//bool introSong = false;
		bool noTown = false;
		bool townEdm = false;
		bool town = false;
		bool melon = false;
		bool ori = false;
		bool ori2 = false;
		bool noori = false;
		bool newboss = false;
		
		bool turnoff = false;
		//private bool CowStatus;
		
		
		private string chatEditLine = "Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline";
		private static System.Timers.Timer ReadEditLineTimer;
		
        public bool IsGuardianAlive
        {
            get
            {
                return riftQuest != null && (riftQuest.QuestStepId == 3 || riftQuest.QuestStepId == 16);
            }
        }

        private IQuest riftQuest
        {
            get
            {
                return Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 337492) ?? // rift
                       Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 382695);   // gr
            }
        }
       
        public TLHP_BaneOfTheStrickenAndNecromancerRGKSongPlugin()
        {
            Enabled = true;
            Dead = false;
            CheatDeath = false;
            DrawClassOnIcons = false;
        }
 
        public override void Load(IController hud)
        {
            base.Load(hud);
           
            StackCountDecorator = new TopLabelDecorator(Hud)
            {
                TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, false, false, 250, 0, 0, 0, true),
                HintFunc = () => "Bane Of The Stricken Count of trigger times(The monomeric BOSS is accurate)"
            };
           
            StackCount = new int[4];
            cooling = new bool[4];
			Key = SharpDX.DirectInput.Key.NumberPad1;
			Key2 = SharpDX.DirectInput.Key.NumberPad2;
			Key3 = SharpDX.DirectInput.Key.NumberPad3;
			Key4 = SharpDX.DirectInput.Key.NumberPad4;
			Key5 = SharpDX.DirectInput.Key.NumberPad6;
			Key6 = SharpDX.DirectInput.Key.NumberPad7;
			Key7 = SharpDX.DirectInput.Key.NumberPad8;
			Key8 = SharpDX.DirectInput.Key.Divide;
			Key9 = SharpDX.DirectInput.Key.NumberPad9;
			Key10 = SharpDX.DirectInput.Key.NumberPad0;
			Key11 = SharpDX.DirectInput.Key.Decimal;
			

			StackBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0);
           
            classShorts = new Dictionary<HeroClass, string>();
            classShorts.Add(HeroClass.Barbarian, "Barb");
            classShorts.Add(HeroClass.Monk, "Monk");
            classShorts.Add(HeroClass.Necromancer, "Necro");
            classShorts.Add(HeroClass.Wizard, "Wiz");
            classShorts.Add(HeroClass.WitchDoctor, "WD");
            classShorts.Add(HeroClass.Crusader, "Sader");
            classShorts.Add(HeroClass.DemonHunter, "DH");  
            ClassFont = Hud.Render.CreateFont("tahoma", 7, 230, 255, 255, 255, true, false, 255, 0, 0, 0, true);
        
			ReadEditLineTimer = new System.Timers.Timer();
			ReadEditLineTimer.Interval = 500;		// edit line filtering interval
			ReadEditLineTimer.Elapsed += ReadEditLine;
			ReadEditLineTimer.AutoReset = true;
			ReadEditLineTimer.Enabled = true;
			
			//CowStatus = false;
		
		}
       
        public void OnNewArea(bool newGame, ISnoArea area)
        {
            Random random = new Random();
			int townmusic = 9999;
			
			if (townEdm)
            {
                townmusic = random.Next(700, 713);
            }
			
			if (noTown)
			{
				townmusic = 9999;
			}
			
			if (town)
			{
                townmusic = random.Next(500,523);
				// townmusic = random.Next(2000,2053);
            }
			
			// if (melon)
			// {
                
				 // townmusic = random.Next(2000,2053);
            // }
			
			if (newGame)
            {
                for (int i = 0; i < 4; i++)
                {
                    StackCount[i] = 0;
                }

              		
				if(!noTown)
                {
                	try
	                {
	                   var soundPlayer = Hud.Sound.LoadSoundPlayer(townmusic + ".wav");
	                   soundPlayer.PlayLooping();
	                }
	                catch (Exception){}
                }
				
				if(noTown)
				{
                	try
	                {
	                   var soundPlayer = Hud.Sound.LoadSoundPlayer("blank.wav");
	                   soundPlayer.Play();
	                }
	                catch (Exception){}
                }
					
                bossIsAlive = false;
			}
		}
		
		public void OnKeyEvent(IKeyEvent keyEvent)
        {
            			
			Random rd = new Random();
			int tm = 9999;
			
			if (keyEvent.Key == Key && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            {
            town = false;
			townEdm = false;
			noTown = true;
			melon = false;
			try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("blank.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			// if (townEdm)
            // {
                // tm = rd.Next(700, 713);
            // }
			
			// if (noTown)
			// {
				// tm = 9999;
			// }
			
			// if (town)
			// {
                // tm = rd.Next(500,508);
            // }
            
			// if(!noTown)
                // {
                	// try
	                // {
	                   // var soundPlayer = Hud.Sound.LoadSoundPlayer(tm + ".wav");
	                   // soundPlayer.PlayLooping();
	                // }
	                // catch (Exception){}
                // }
				
				// if(noTown)
				// {
                	// try
	                // {
	                   // var soundPlayer = Hud.Sound.LoadSoundPlayer("blank.wav");
	                   // soundPlayer.Play();
	                // }
	                // catch (Exception){}
                // }
			
			}
        
			if (keyEvent.Key == Key2 && !keyEvent.IsPressed)
			{
				town = true;
				townEdm = false;
            	noTown = false;
				melon = false;
				tm = rd.Next(500, 523);
				//tm = rd.Next(2000, 2052);
				try
	                {
	                   var soundPlayer = Hud.Sound.LoadSoundPlayer(tm +".wav");
	                   soundPlayer.PlayLooping();
	                }
					catch (Exception){}
				
			}
			if (keyEvent.Key == Key3 && !keyEvent.IsPressed)
			{
				townEdm = true;
				town = false;
                noTown = false;
				melon = false;
                tm = rd.Next(700,712);
                if(!noTown)
				{
					try
	                {
	                   var soundPlayer = Hud.Sound.LoadSoundPlayer(tm +".wav");
	                   soundPlayer.PlayLooping();
	                }
					catch (Exception){}
				}
			}
		
			if (keyEvent.Key == Key4 && !keyEvent.IsPressed)
			{
				townEdm = false;
				town = false;
                noTown = false;
				melon = true;
                tm = rd.Next(2000, 2052);
                if(!noTown)
				{
					try
	                {
	                   var soundPlayer = Hud.Sound.LoadSoundPlayer(tm +".wav");
	                   soundPlayer.PlayLooping();
	                }
					catch (Exception){}
				}
			}
			
			
			
			if (keyEvent.Key == Key5 && !keyEvent.IsPressed)
			{
				BossStatus(false);
			}
			
			
			// if (keyEvent.Key == Key6 && !keyEvent.IsPressed)
			// {
				  // ori = true;
				  // ori2 = false;
				// noori = false;
				// newboss = false;
				// BossStatus(false);
			// }
			
			// if (keyEvent.Key == Key7 && !keyEvent.IsPressed)
			// {
				 // ori = false;
				 // ori2 = true;
				 // noori = false;
				 // newboss = false;
				 // BossStatus(false);
			// }
			
			// if (keyEvent.Key == Key8 && !keyEvent.IsPressed)
			// {
				// ori = false;
				// ori2 = false;
				// noori = true;
				// newboss = false;
				// BossStatus(false);
			// }
			
			// if (keyEvent.Key == Key9 && !keyEvent.IsPressed)
			// {
				// ori = false;
				// ori2 = false;
				// noori = false;
				// newboss = true;
				// BossStatus(false);
			// }
			
			if (keyEvent.Key == Key10 && !keyEvent.IsPressed)
			{
				turnoff = true;
				
				try
	                {
	                   var soundPlayer = Hud.Sound.LoadSoundPlayer("off.wav");
	                   soundPlayer.Play();
	                }
					catch (Exception){}
			}
			
			if (keyEvent.Key == Key11 && !keyEvent.IsPressed)
			{
				turnoff = false;
					
					try
	                {
	                   var soundPlayer = Hud.Sound.LoadSoundPlayer("on.wav");
	                   soundPlayer.Play();
	                }
					catch (Exception){}
			}
			

		}
       
        public void DrawStackCount(IPlayer player)
        {
            if (!player.Powers.BuffIsActive(Hud.Sno.SnoPowers.BaneOfTheStrickenPrimary.Sno, 0)) return;
            int count = Hud.Game.AliveMonsters.Count(m => m.SnoMonster.Priority == MonsterPriority.boss);
            if (count == 0) return;
           
            var uiBar = Hud.Render.MonsterHpBarUiElement;
            var monster = Hud.Game.SelectedMonster2 ?? Hud.Game.SelectedMonster1;
            if ((monster == null) || (uiBar == null) || (monster.SnoMonster.Priority != MonsterPriority.boss)) return;
            int HitnRng = 1;
            int _count = 0;
 
            _count = Hud.Game.AliveMonsters.Count(m => (player.FloorCoordinate.XYZDistanceTo(m.FloorCoordinate)) <= 10);
            if (_count < 1) _count = 1;
            Random rng;
            rng = new Random(Hud.Game.CurrentGameTick);
            HitnRng = rng.Next(1, _count);
           
            var w = uiBar.Rectangle.Height * 2;
            var h = uiBar.Rectangle.Height;
            var x = uiBar.Rectangle.Right + uiBar.Rectangle.Height * 5;
            var y = uiBar.Rectangle.Y + uiBar.Rectangle.Height * 0.3f;
           
            var bgTex = Hud.Texture.GetTexture(3166997520);
            var tex = Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Unique_Gem_018_x1);
            var rect = new RectangleF(uiBar.Rectangle.Right + uiBar.Rectangle.Height * 5f + xPos, uiBar.Rectangle.Y - uiBar.Rectangle.Height * 1.5f / 6, uiBar.Rectangle.Height * 1.5f, uiBar.Rectangle.Height * 1.5f);
           
            var index = player.PortraitIndex;
            if (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.BaneOfTheStrickenPrimary.Sno, 2))
            {
                if (!cooling[index])
                {
                    cooling[index] = true;
                    if(HitnRng == 1) StackCount[index]++;
                }
            }
            else
            {
                cooling[index] = false;
            }
           
            StackCountDecorator.TextFunc = () => StackCount[index].ToString();
            StackBrush.DrawRectangle(rect);
            bgTex.Draw(rect.Left, rect.Top, rect.Width, rect.Height);
            tex.Draw(rect.Left, rect.Top, rect.Width, rect.Height);
            var layout = ClassFont.GetTextLayout(player.BattleTagAbovePortrait + "\n(" + classShorts[player.HeroClassDefinition.HeroClass] + ")");
           
            if (DrawClassOnIcons || Hud.Window.CursorInsideRect(rect.X, rect.Y, rect.Width, rect.Height))
            {
                ClassFont.DrawText(layout, x - (layout.Metrics.Width * 0.1f) + xPos, y - h * 3);
            }
            StackCountDecorator.Paint(x + xPos, y, w, h, HorizontalAlign.Center);
            xPos += rect.Width + h;
        }
 
        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;

            if (!IsGuardianAlive)
            {
                for (int i = 0; i < 4; i++)
                {
                    StackCount[i] = 0;
                }
                bossIsAlive = false;
                return;
            }

            foreach (IPlayer player in Hud.Game.Players)
            {
                DrawStackCount(player);
                PlayDeathSongs(player);
                PlayNecSongs(player);
            }

            xPos = 0;
        }

        
		public void ReadEditLine(Object source, System.Timers.ElapsedEventArgs e)
	    {
			if (!Hud.Render.GetUiElement(chatEditLine).Visible)
        			return;
				
			string lineStr = Hud.Render.GetUiElement(chatEditLine).ReadText(System.Text.Encoding.UTF8, false).Trim();	// if error, change "UTF8" with "Default"...not tested though
        		lineStr = lineStr.Trim().ToLower();
			Match townMatch = Regex.Match(lineStr, @"(?=/town/)");  // .+(?=/)
			Match bossMatch = Regex.Match(lineStr, @"(?=/boss/)");
			Match townEdmMatch = Regex.Match(lineStr, @"(?=/townedm/)");
			Match noTownMatch = Regex.Match(lineStr, @"(?=/notown/)");
			
			Random random = new Random();
			int townmusic = 9999; //random.Next(500, 506);  default is notown
			
            if(noTownMatch.Success)
            {
            	noTown = true;
				town = false;
				townEdm = false;
				melon = false;
            	try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("blank.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
            }
            if(townMatch.Success)
            {
            	town = true;
				townEdm = false;
            	noTown = false;
				melon = false;
			
				townmusic = random.Next(500, 523);
				//townmusic = random.Next(2000, 2052);
				
				try
	                {
	                   var soundPlayer = Hud.Sound.LoadSoundPlayer(townmusic +".wav");
	                   soundPlayer.PlayLooping();
	                }
					catch (Exception){}
            }

           	if(townEdmMatch.Success) 
            {
                townEdm = true;
				town = false;
                noTown = false;
				melon = false;
                townmusic = random.Next(700,713);
                if(!noTown)
				{
					try
	                {
	                   var soundPlayer = Hud.Sound.LoadSoundPlayer(townmusic +".wav");
	                   soundPlayer.PlayLooping();
	                }
					catch (Exception){}
				}
            }
			
			if(townEdm) 
            {
                townmusic = random.Next(700,713);
            }

			if(town)
			{
                townmusic = random.Next(500,523);
				//townmusic = random.Next(2000,2052);
            }
			
			if(noTown)
			{
                townmusic = 9999;
            }
			
						
			if (bossMatch.Success)
			{
				BossStatus(false);
			}
			
	   }
		
		private void PlayDeathSongs(IPlayer player)
        {
            if (player == null || player.HeroClassDefinition.HeroClass != HeroClass.Necromancer) return;
            if (Hud.Game.SpecialArea != SpecialArea.GreaterRift) return;
            
			if (!turnoff) {
			
            bool IceSkeleton = false;
            bool PoisonSkeleton = false;

            if (player.HeroClassDefinition.HeroClass == HeroClass.Necromancer)
            { //player is Necro
                foreach (var skill in player.Powers.UsedSkills)
                {
                    //check for Command Skeletons
                    if (skill.RuneNameEnglish == "Freezing Grasp")
                    {
                        IceSkeleton = true;
                    }

                    if (skill.RuneNameEnglish == "Kill Command")
                    {
                        PoisonSkeleton = true;
                    }
                }
            }
            int monsterCount = Hud.Game.AliveMonsters.Count(m => m.SnoMonster.Priority == MonsterPriority.boss);
            int currentGrLevel = (int)Hud.Game.Me.InGreaterRiftRank;

            if (currentGrLevel >= 130 && monsterCount == 1)
            {
                if (PlayerDead(player))
                { // Necro died
                    try
                    {
                        var soundPlayer = Hud.Sound.LoadSoundPlayer("shutdown.wav");
                        soundPlayer.Play();
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (CheatDeathStatus(player))
                {
                    //Necro Passive Bursts (FinalService)
                    if (IceSkeleton || PoisonSkeleton)
                    {
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("sumgim1.wav");
                            soundPlayer.Play();
                        }
                        catch (Exception)
                        {
                        }
                    }

                }
            } 
			}
		}
		
		public void PlayNecSongs(IPlayer player)
        {
            //if (player == null || player.HeroClassDefinition.HeroClass != HeroClass.Necromancer) return;
            if (Hud.Game.SpecialArea != SpecialArea.GreaterRift) return;
			
			if (!turnoff) {
			
            int currentGrLevel = (int) Hud.Game.Me.InGreaterRiftRank;
            int monsterCount = Hud.Game.AliveMonsters.Count(m => m.SnoMonster.Priority == MonsterPriority.boss);
			
			bool GRGuardianIsDead = false;
            if (riftQuest != null)
            {
                if (Hud.Game.Monsters.Any(m => m.Rarity == ActorRarity.Boss && !m.IsAlive))
                {
                    GRGuardianIsDead = true;
                }
			}
			
            if (Hud.Game.SpecialArea == SpecialArea.GreaterRift && currentGrLevel >= 130 && monsterCount == 1)
            {
                if (player == null || player.HeroClassDefinition.HeroClass != HeroClass.Necromancer || player.HeroClassDefinition.HeroClass != HeroClass.Crusader ) return;
				
				SimBuffActive = false;
                LotDBuffActive = false;
				LotDBuffDiminish = false;
				PoisonConventionActive = false;
				PhysicalConventionActive = false;
				ColdConventionActive = false;
                bool PoisonSkeleton = false;
				bool fifthtierboss = false;
				bool fourthtierboss = false;
				bool thirdtierboss = false;
				bool secondtierboss = false;
				bool firsttierboss = false;
				bool blighter = false;
				bool bloodmaw = false;
				bool perdition = false;
				bool maiden = false;
				bool rime = false;
				bool agnidox = false;
				bool voracity = false;
				bool hamelin = false;

				bool znecStatus = false;
				var znec = player.Powers.GetBuff(402459);
				if(znec != null && znec.Active)
				{
					znecStatus = true;
				}
				
				bool rgkcrusader = false;
				var rgkcru = player.Powers.GetBuff(446162);
				if(rgkcru != null && rgkcru.Active)
				{
					rgkcrusader = true;
				}
				
                var bosses = Hud.Game.AliveMonsters.Where(m => m.Rarity == ActorRarity.Boss);
				foreach(IMonster monster in bosses) 
				{
					if (monster.SummonerAcdDynamicId != 0) continue;

					if (monster.SnoMonster.NameEnglish == ("Orlash") || monster.SnoMonster.NameEnglish == ("Bone Warlock") 
						|| monster.SnoMonster.NameEnglish == ("The Binder") 
					|| monster.SnoMonster.NameEnglish == ("The Choker") || monster.SnoMonster.NameEnglish == ("Vesalius") )
					
					{
						fifthtierboss = true;
					}
				
					else if (monster.SnoMonster.NameEnglish == ("Saxtris") || monster.SnoMonster.NameEnglish == ("Eskandiel") 
						|| monster.SnoMonster.NameEnglish == ("Stonesinger") || monster.SnoMonster.NameEnglish == ("Cold Snap") )
					{
						fourthtierboss = true;
					}
				
					else if (monster.SnoMonster.NameEnglish == ("Perendi") || monster.SnoMonster.NameEnglish == ("Man Carver") 
						|| monster.SnoMonster.NameEnglish == ("Sand Shaper") ) 
					{
						thirdtierboss = true;
					}
					
					else if (monster.SnoMonster.NameEnglish == ("Ember") || monster.SnoMonster.NameEnglish == ("Tethrys")  
					|| monster.SnoMonster.NameEnglish == ("Raiziel") )
					{
						secondtierboss = true;
					}
				
					else if (monster.SnoMonster.NameEnglish == ("Crusader King") || monster.SnoMonster.NameEnglish == ("Erethon") || monster.SnoMonster.NameEnglish == ("Voracity") )
					{
						firsttierboss = true;
					}
					
					else if (monster.SnoMonster.NameEnglish == ("Rime") )
					{
						rime = true;
					}
					
					else if (monster.SnoMonster.NameEnglish == ("Blighter") )
					{
						blighter = true;
					}
					
					else if (monster.SnoMonster.NameEnglish == ("Bloodmaw") )
					{
						bloodmaw = true;
					}
					
					else if (monster.SnoMonster.NameEnglish == ("Perdition") )
					{
						perdition = true;
					}
					
					else if (monster.SnoMonster.NameEnglish == ("Infernal Maiden") )
					{
						maiden = true;
					}
					
					else if (monster.SnoMonster.NameEnglish == ("Agnidox") )
					{
						agnidox = true;
					}					
					
					// else if (monster.SnoMonster.NameEnglish == ("Voracity") )	
					// {
						// voracity = true;
					// }
					else if (monster.SnoMonster.NameEnglish == ("Hamelin") )	
					{
						hamelin = true;
					}
			
			
				}
				
				foreach (var usedSkills in player.Powers.UsedSkills)
                {
                    if (usedSkills.RuneNameEnglish == "Kill Command")
                    {
                        PoisonSkeleton = true;
                    }
                }

                if ((PoisonSkeleton || znecStatus || rgkcrusader) && fifthtierboss)
				{
					var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("fifth.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 
				}
												
				else if ((PoisonSkeleton || znecStatus || rgkcrusader) && fourthtierboss)
				{
					var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("fourth.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 
				}
				
				else if ((PoisonSkeleton || znecStatus || rgkcrusader) && thirdtierboss)
				{
					var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("third.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 
				}
				
				else if ((PoisonSkeleton || znecStatus || rgkcrusader) && secondtierboss)
				{
					var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("second.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 
				}
				
				else if ((PoisonSkeleton || znecStatus || rgkcrusader) && firsttierboss)
				{
					var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("first.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 
				}
				
				else if ((PoisonSkeleton || znecStatus || rgkcrusader) && perdition)
				{
					var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("perdition.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 
				}
				
				else if ((PoisonSkeleton || znecStatus || rgkcrusader) && maiden)
				{
					var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("oriboss.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 
				}
				
				else if ((PoisonSkeleton || znecStatus || rgkcrusader) && bloodmaw)
				{
					var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("bloodmaw.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 
				}
				
				else if ((PoisonSkeleton || znecStatus || rgkcrusader) && blighter)
				{
					var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("blighter.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 
				}
				
				else if ((PoisonSkeleton || znecStatus || rgkcrusader) && rime)
				{
					var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("rime.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 
				}
				
				else if ((PoisonSkeleton || znecStatus || rgkcrusader) && agnidox)
				{
					var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("agnidox.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 
				}
				
				// else if ((PoisonSkeleton || znecStatus) && voracity)
				// {
					// var bossStatus = true;
					// if (BossStatus(bossStatus)) 
					// {
                        // try
                        // {
                            // var soundPlayer = Hud.Sound.LoadSoundPlayer("voracity.wav");
                            // soundPlayer.PlayLooping();
                        // }
                        // catch (Exception)
                        // {
                        // }
                    // } 
				// }
				
				else if ((PoisonSkeleton || znecStatus || rgkcrusader) && hamelin)
				{
					var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("hamelin.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 
				}
				
				else if (!PoisonSkeleton) //&& !ori && !ori2 && !newboss)
				{
					var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("oriboss.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 
				}
				
				// else if (!PoisonSkeleton && ori && !noori && !newboss)
				// {
					// var bossStatus = true;
					// if (BossStatus(bossStatus)) 
					// {
                        // try
                        // {
                            // var soundPlayer = Hud.Sound.LoadSoundPlayer("oriboss.wav");
                            // soundPlayer.PlayLooping();
                        // }
                        // catch (Exception)
                        // {
                        // }
                    // } 
				// }
				
				// else if (!PoisonSkeleton && ori2 && !noori && !newboss)
				// {
					// var bossStatus = true;
					// if (BossStatus(bossStatus)) 
					// {
                        // try
                        // {
                            // var soundPlayer = Hud.Sound.LoadSoundPlayer("oriboss2.wav");
                            // soundPlayer.PlayLooping();
                        // }
                        // catch (Exception)
                        // {
                        // }
                    // } 
				// }
				
				// else if (!PoisonSkeleton && newboss && !noori)
				// {
					// var bossStatus = true;
					// if (BossStatus(bossStatus)) 
					// {
                        // try
                        // {
                            // var soundPlayer = Hud.Sound.LoadSoundPlayer("newrgk2.wav");
                            // soundPlayer.PlayLooping();
                        // }
                        // catch (Exception)
                        // {
                        // }
                    // } 
				// }



				foreach (var i in _skillOrder)
                {
                    var skill = player.Powers.SkillSlots[i];
                    if (skill == null || ((skill.SnoPower.Sno != 465839) && (skill.SnoPower.Sno != 465350))) continue;

                    if (player.Powers.BuffIsActive(465350, 1)) //Necromancer_Simulacrum { get; }
                    {
                        SimBuffActive = true;
                    }

                    if (skill.SnoPower.Sno == 465839) //Necromancer_LandOfTheDead { get; }
                    {
                        var buff = skill.Buff;
                        if ((buff == null) || (buff.IconCounts[0] <= 0)) continue;
                        LotDBuffActive = buff.TimeLeftSeconds[0] >= 9.8;
						LotDBuffDiminish = buff.TimeLeftSeconds[0] >= 0.4 && buff.TimeLeftSeconds[0] <= 0.5; 
						//if buff.TimeLeftSeconds[0] == 0.5 
										//&& buff.TimeLeftSeconds[0] <= 1.0; 
					}
	
					if (player.Powers.BuffIsActive(430674, 7))
					{
						PoisonConventionActive = true;
					}
					
					if (player.Powers.BuffIsActive(430674, 6))
					{
						PhysicalConventionActive = true;
					}

					if (player.Powers.BuffIsActive(430674, 2))
					{
						ColdConventionActive = true;
					}
					
					// if (SimBuffActive && !PoisonConventionActive)
					// {
						// fail = true;
					// }
					
					if (LotDBuffActive && !PoisonSkeleton)
                    {
                        if (SimBuffActive && PoisonConventionActive)
                        {
                         	var index = player.PortraitIndex;
							success = true;
								                        
							if(StackCount[index] >= 175) 
	                        {
	                            try
	                            {
	                                var soundPlayer = Hud.Sound.LoadSoundPlayer("hstack1.wav");
	                                soundPlayer.Play();
	                            }
                                catch (Exception)
                                {
                                }
                            } 
                            else if (StackCount[index] < 175)
                            {
                                try
                                {
                                    var soundPlayer = Hud.Sound.LoadSoundPlayer("lstack1.wav");
                                    soundPlayer.Play();
                                }
                                catch (Exception)
                                {
                                }
                            }  
                        }
                        else if (SimBuffActive && !PoisonConventionActive)
                        {
                            fail = true;
							try
                            {
                                var soundPlayer = Hud.Sound.LoadSoundPlayer("jotmang1.wav");
                                soundPlayer.Play();
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
					if (LotDBuffDiminish && !PoisonSkeleton) //&& !ori && !ori2 &&!newboss)
                    {
						Random rg = new Random();
						int rgk = rg.Next(1, 3);

						if (SimBuffActive && success)
                        {
                         	var index = player.PortraitIndex;
							
								                        
							if(StackCount[index] >= 175) 
	                        {
	                            try
	                            {
	                                var soundPlayer = Hud.Sound.LoadSoundPlayer("highstackrgkvar" + rgk + ".wav");
	                                soundPlayer.Play();
	                            }
                                catch (Exception)
                                {
                                }
								LotDBuffDiminish = false;
								success = false;
							} 
                            else if (StackCount[index] < 175)
                            {
                                try
                                {
                                    var soundPlayer = Hud.Sound.LoadSoundPlayer("lowstackrgkvar" + rgk + ".wav");
                                    soundPlayer.Play();
                                }
                                catch (Exception)
                                {
                                }
								LotDBuffDiminish = false;
								success = false;
							}  
                        }
                        else if (SimBuffActive && fail)
                        {
                            try
                            {
                                var soundPlayer = Hud.Sound.LoadSoundPlayer("jotmangvar" + rgk + ".wav");
                                soundPlayer.Play();
                            }
                            catch (Exception)
                            {
                            }
                        LotDBuffDiminish = false;
						fail = false;
						}
					}
					
					// if (LotDBuffDiminish && !PoisonSkeleton && ori)
                    // {
						// Random rgori = new Random();
						// int rgkori = rgori.Next(1, 3);

						// if (SimBuffActive && success)
                        // {
                         	// var index = player.PortraitIndex;
							
								                        
							// if(StackCount[index] >= 175) 
	                        // {
	                            // try
	                            // {
	                                // var soundPlayer = Hud.Sound.LoadSoundPlayer("highstackrgkO" + rgkori + ".wav");
	                                // soundPlayer.Play();
	                            // }
                                // catch (Exception)
                                // {
                                // }
								// LotDBuffDiminish = false;
								// success = false;
							// } 
                            // else if (StackCount[index] < 175)
                            // {
                                // try
                                // {
                                    // var soundPlayer = Hud.Sound.LoadSoundPlayer("lowstackrgkO" + rgkori + ".wav");
                                    // soundPlayer.Play();
                                // }
                                // catch (Exception)
                                // {
                                // }
								// LotDBuffDiminish = false;
								// success = false;
							// }  
                        // }
                        // else if (SimBuffActive && fail)
                        // {
                            // try
                            // {
                                // var soundPlayer = Hud.Sound.LoadSoundPlayer("jotmangO" + rgkori + ".wav");
                                // soundPlayer.Play();
                            // }
                            // catch (Exception)
                            // {
                            // }
                        // LotDBuffDiminish = false;
						// fail = false;
						// }
					// }
		
					// if (LotDBuffDiminish && !PoisonSkeleton && ori2)
                    // {
						// Random rgori2 = new Random();
						// int rgkori2 = rgori2.Next(1, 3);

						// if (SimBuffActive && success)
                        // {
                         	// var index = player.PortraitIndex;
							
								                        
							// if(StackCount[index] >= 175) 
	                        // {
	                            // try
	                            // {
	                                // var soundPlayer = Hud.Sound.LoadSoundPlayer("highstackrgkR" + rgkori2 + ".wav");
	                                // soundPlayer.Play();
	                            // }
                                // catch (Exception)
                                // {
                                // }
								// LotDBuffDiminish = false;
								// success = false;
							// } 
                            // else if (StackCount[index] < 175)
                            // {
                                // try
                                // {
                                    // var soundPlayer = Hud.Sound.LoadSoundPlayer("lowstackrgkR" + rgkori2 + ".wav");
                                    // soundPlayer.Play();
                                // }
                                // catch (Exception)
                                // {
                                // }
								// LotDBuffDiminish = false;
								// success = false;
							// }  
                        // }
                        // else if (SimBuffActive && fail)
                        // {
                            // try
                            // {
                                // var soundPlayer = Hud.Sound.LoadSoundPlayer("jotmangR" + rgkori2 + ".wav");
                                // soundPlayer.Play();
                            // }
                            // catch (Exception)
                            // {
                            // }
                        // LotDBuffDiminish = false;
						// fail = false;
						// }
					// }
					
					// if (LotDBuffDiminish && !PoisonSkeleton && newboss)
                    // {
						// Random nnboss = new Random();
						// int nboss = nnboss.Next(1, 3);

						// if (SimBuffActive && success)
                        // {
                         	// var index = player.PortraitIndex;
							
								                        
							// if(StackCount[index] >= 175) 
	                        // {
	                            // try
	                            // {
	                                // var soundPlayer = Hud.Sound.LoadSoundPlayer("highstackrgk2var" + nboss + ".wav");
	                                // soundPlayer.Play();
	                            // }
                                // catch (Exception)
                                // {
                                // }
								// LotDBuffDiminish = false;
								// success = false;
							// } 
                            // else if (StackCount[index] < 175)
                            // {
                                // try
                                // {
                                    // var soundPlayer = Hud.Sound.LoadSoundPlayer("lowstackrgk2var" + nboss + ".wav");
                                    // soundPlayer.Play();
                                // }
                                // catch (Exception)
                                // {
                                // }
								// LotDBuffDiminish = false;
								// success = false;
							// }  
                        // }
                        // else if (SimBuffActive && fail)
                        // {
                            // try
                            // {
                                // var soundPlayer = Hud.Sound.LoadSoundPlayer("jotmang2var" + nboss + ".wav");
                                // soundPlayer.Play();
                            // }
                            // catch (Exception)
                            // {
                            // }
                        // LotDBuffDiminish = false;
						// fail = false;
						// }
					// }
					
					
				}       
            } 
			
			if (Hud.Game.SpecialArea == SpecialArea.GreaterRift && currentGrLevel == 20 && monsterCount == 1)
			{
			
				if (Hud.Game.NumberOfPlayersInGame <= 1)
				{
				var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("secondrgk.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 	
				}
			}
			
			if (Hud.Game.SpecialArea == SpecialArea.GreaterRift && currentGrLevel == 70 && monsterCount == 1)
			{
			
				if (Hud.Game.NumberOfPlayersInGame <= 1)
				{
				var bossStatus = true;
					if (BossStatus(bossStatus)) 
					{
                        try
                        {
                            var soundPlayer = Hud.Sound.LoadSoundPlayer("oriboss.wav");
                            soundPlayer.PlayLooping();
                        }
                        catch (Exception)
                        {
                        }
                    } 	
				}
			}
			
			}
		}
		
		private bool BossStatus(bool bossStatus) 
		{
			if(bossStatus)
			{
				if(!bossIsAlive) {
					bossIsAlive = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(bossIsAlive) {
					bossIsAlive = false;
				}
				return false;
			}
		}

       		
		private bool PlayerDead(IPlayer player)
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
		
		// private bool cowStatus (bool CowCheck) 
		// {
			// if(CowCheck)
			// {
				// if(!CowStatus) {
					// CowStatus = true;
					// return true;
				// }
				// return false;
			// } 
			// else 
			// {
				// if(CowStatus) {
					// CowStatus = false;
				// }
				// return false;
			// }
		// }

			
        private bool CheatDeathStatus(IPlayer player) 
        {
            var CheatDeathBuff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Necromancer_Passive_FinalService.Sno);
            if(CheatDeathBuff != null && CheatDeathBuff.TimeLeftSeconds[1] > 0.0) {
                if(!CheatDeath) {
                    CheatDeath = true;
                    return true;
                }
                return false;
            } else {
                if(CheatDeath) CheatDeath = false;
                return false;
            }
            return false;
        }
    }
}