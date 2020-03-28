// Originally made by Jack, edited by Tommy 07-11-2019

namespace Turbo.Plugins.TLHP
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Turbo.Plugins.Default;
	using System.Text.RegularExpressions;
	using System.Collections.Generic;
	using SharpDX.DirectInput;

    public class TLHP_RiftInfoAvengersPlugin : BasePlugin, IInGameTopPainter, IAfterCollectHandler, ICustomizer, INewAreaHandler, IKeyEventHandler
    {
        public IFont ProgressBarTimerFont { get; set; }
        public IFont ObjectiveProgressFont { get; set; }

        public string ObjectiveProgressSymbol { get; set; }
        public string GuardianAliveSymbol { get; set; }
        public string GuardianDeadSymbol { get; set; }

        public string SecondsFormat { get; set; }
        public string MinutesSecondsFormat { get; set; }
        public string ProgressPercentFormat { get; set; }
        public string ClosingSecondsFormat { get; set; }

        public bool GreaterRiftCountdown { get; set; }
        public bool ShowGreaterRiftTimer { get; set; }
        public bool ShowGreaterRiftCompletedTimer { get; set; }
        public bool ShowClosingTimer { get; set; }

        public int CompletionDisplayLimit { get; set; }
        public Func<string> RiftCompletionTitleFunc { get; set; }
        public TopLabelWithTitleDecorator CompletionLabelDecorator { get; set; }
		private string chatEditLine = "Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline";
		private static System.Timers.Timer ReadEditLineTimer;

		public List<ISnoQuest> BountiesToPopup { get; private set; }
		
		public SharpDX.DirectInput.Key Key { get; set; }
		public SharpDX.DirectInput.Key Key2 { get; set; }
		public SharpDX.DirectInput.Key Key3 { get; set; }
		
		bool turnoff = false;

        public bool IsGuardianAlive
        {
            get
            {
                return riftQuest != null && (riftQuest.QuestStepId == 3 || riftQuest.QuestStepId == 16);
            }
        }

        public bool IsGuardianDead
        {
            get
            {
                if (Hud.Game.Monsters.Any(m => m.Rarity == ActorRarity.Boss && !m.IsAlive))
                    return true;

                return riftQuest != null && (riftQuest.QuestStepId == 5 || riftQuest.QuestStepId == 10 || riftQuest.QuestStepId == 34 || riftQuest.QuestStepId == 46);
            }
        }

        public bool IsNephalemRift
        {
            get
            {
                return riftQuest != null && (riftQuest.QuestStepId == 1 || riftQuest.QuestStepId == 3 || riftQuest.QuestStepId == 10);
            }
        }

        public bool IsGreaterRift
        {
            get
            {
                return riftQuest != null &&
                       (riftQuest.QuestStepId == 13 || riftQuest.QuestStepId == 16 || riftQuest.QuestStepId == 34 ||
                        riftQuest.QuestStepId == 46);
            }
        }

        private bool show
        {
            get
            {
                if (riftQuest == null) return false;
                if (riftQuest.State == QuestState.none) return false;
                if (Hud.Inventory.InventoryMainUiElement.Visible) return false;

                return true;
            }
        }

        private const uint riftClosingMilliseconds = 30000;
        private SpecialArea? currentRun;

        private IQuest riftQuest
        {
            get
            {
                return Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 337492) ?? // rift
                       Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 382695);   // gr
            }
        }

        private IUiElement uiProgressBar
        {
            get
            {
                return IsNephalemRift ? Hud.Render.NephalemRiftBarUiElement : Hud.Render.GreaterRiftBarUiElement;
            }
        }

        private IWatch riftTimer;
        private IWatch guardianTimer;
        private IWatch pauseTimer;

        private const long greaterRiftMaxTime = 15 * 60 * 1000;

        private readonly StringBuilder textBuilder;

        public TLHP_RiftInfoAvengersPlugin()
        {
            Enabled = true;
			BountiesToPopup = new List<ISnoQuest>();
            textBuilder = new StringBuilder();
        }

        private IBrush brus;
        private bool introSongStatus;
        private bool lastThirtyStatus;
		
		//pools
		private bool BFTownStatus;
		//cow level
		private bool CowStatus;
		private bool InnStatus;
		private bool LeahStatus;
		private bool RiftRng;
		private bool QStatus;
		
        public override void Load(IController hud)
        {
            base.Load(hud);

            Key = SharpDX.DirectInput.Key.NumberPad5;
			Key2 = SharpDX.DirectInput.Key.NumberPad0;
			Key3 = SharpDX.DirectInput.Key.Decimal;
			introSongStatus = false;
            lastThirtyStatus = false;
			//pools
			BFTownStatus = false;
			//cow level
			CowStatus = false; 
			InnStatus = false;
			LeahStatus = false;
			RiftRng = false;
			QStatus = false;
			
			ReadEditLineTimer = new System.Timers.Timer();
			ReadEditLineTimer.Interval = 500;		// edit line filtering interval
			ReadEditLineTimer.Elapsed += ReadEditLine;
			ReadEditLineTimer.AutoReset = true;
			ReadEditLineTimer.Enabled = true;


		   brus = Hud.Render.CreateBrush(255, 0, 0, 0, 0);
            ShowClosingTimer = false;
            GreaterRiftCountdown = false;
            ShowGreaterRiftTimer = true;
            ShowGreaterRiftCompletedTimer = true;
            CompletionDisplayLimit = 90;

            ObjectiveProgressSymbol = "\u2694"; //⚔
            GuardianAliveSymbol = "\uD83D\uDC7F"; //👿
            GuardianDeadSymbol = "\uD83D\uDC80"; //💀

            ObjectiveProgressSymbol = "";
            GuardianAliveSymbol = "\uD83D\uDC7F"; //??
            GuardianDeadSymbol = "\uD83D\uDC80"; //??uDC80"; //??

            MinutesSecondsFormat = "{0:%m}:{0:ss}";
            SecondsFormat = "{0:%s}";

            ProgressPercentFormat = "({0:F1}%)";
            ClosingSecondsFormat = "({0:%s})";

            ProgressBarTimerFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 210, 150, true, false, 160, 0, 0, 0, true);

            ObjectiveProgressFont = Hud.Render.CreateFont("tahoma", 8, 224, 240, 240, 240, false, false, false);
            ObjectiveProgressFont.SetShadowBrush(222, 0, 0, 0, true);

            RiftCompletionTitleFunc = () => riftQuest.QuestStep.SplashLocalized.Trim();
            CompletionLabelDecorator = new TopLabelWithTitleDecorator(Hud)
            {
                BorderBrush = Hud.Render.CreateBrush(255, 180, 147, 109, -1),
                BackgroundBrush = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                TextFont = Hud.Render.CreateFont("tahoma", 9, 255, 255, 210, 150, true, false, false),
                TitleFont = Hud.Render.CreateFont("tahoma", 6, 255, 180, 147, 109, true, false, false),
            };

            pauseTimer = Hud.Time.CreateWatch();
            riftTimer = Hud.Time.CreateWatch();
            guardianTimer = Hud.Time.CreateWatch();
			
			BountiesToPopup.Add(Hud.Sno.SnoQuests.Bounty_TheCursedPeat_375278);
			BountiesToPopup.Add(Hud.Sno.SnoQuests.Bounty_TheCursedCellar_369944);
			BountiesToPopup.Add(Hud.Sno.SnoQuests.Bounty_TheCursedBellows_369789);
			BountiesToPopup.Add(Hud.Sno.SnoQuests.Bounty_TheCursedCourt_375191);
			BountiesToPopup.Add(Hud.Sno.SnoQuests.Bounty_TheCursedSpire_375257);
			
        }

        public void Customize()
        {
            Hud.TogglePlugin<NotifyAtRiftPercentagePlugin>(false);
        }

      
		
		public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;
            if (!show) return;
            if (currentRun == null)
            {
                currentRun = IsNephalemRift ? SpecialArea.Rift : SpecialArea.GreaterRift;
            }

            if (Hud.Game.RiftPercentage >= CompletionDisplayLimit && Hud.Game.RiftPercentage < 100)
            {
                var text = Hud.Game.RiftPercentage.ToString("F1", CultureInfo.InvariantCulture) + "%";
                var title = RiftCompletionTitleFunc.Invoke();
                var layout = CompletionLabelDecorator.TextFont.GetTextLayout(title);
                var w = layout.Metrics.Width * 0.8f;
                var h = Hud.Window.Size.Height * 0.04f;
                CompletionLabelDecorator.Paint(Hud.Window.Size.Width * 0.5f - w / 2, Hud.Window.Size.Height * 0.18f - h / 2, w, h, text, title);
            }

            if (IsNephalemRift && uiProgressBar.Visible)
            {
                var layout = ProgressBarTimerFont.GetTextLayout(GetText(true));
                var x = uiProgressBar.Rectangle.Left - layout.Metrics.Width / 2 + uiProgressBar.Rectangle.Width * (float)Hud.Game.RiftPercentage / 100.0f;
                var y = uiProgressBar.Rectangle.Bottom + uiProgressBar.Rectangle.Height * 0.1f;

                ProgressBarTimerFont.DrawText(layout, x, y);
            }
            else
            {
                var layout = ObjectiveProgressFont.GetTextLayout(GetText(false));
                var x = Hud.Render.MinimapUiElement.Rectangle.Right - layout.Metrics.Width - Hud.Window.Size.Height * 0.033f;
                var y = Hud.Render.MinimapUiElement.Rectangle.Bottom + Hud.Window.Size.Height * 0.0033f;
                //brus.DrawRectangle(x, y, 100, 100);
                ObjectiveProgressFont.DrawText(layout, x, y);

                var texture = Hud.Texture.GetTexture(currentRun == SpecialArea.Rift ? 1528804216 : 3075014090);
                texture.Draw(x - texture.Width * 0.75f, Hud.Render.MinimapUiElement.Rectangle.Bottom - texture.Height/4, texture.Width, texture.Height);
            }
        
		}
       
	   
	   public void OnKeyEvent(IKeyEvent keyEvent)
        {
	
			if (keyEvent.Key == Key && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            {
				cowStatus(false);
				innStatus(false);
				leahStatus(false);
				qStatus(false);
			}
			
			if (keyEvent.Key == Key2 && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            {
				turnoff = true;
			}
			
			if (keyEvent.Key == Key3 && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            {
				turnoff = false;
			}
		
		}
	   
	   
	   
	   public void ReadEditLine(Object source, System.Timers.ElapsedEventArgs e)
	   {
			if (!Hud.Render.GetUiElement(chatEditLine).Visible)
        			return;
				
			string lineStr = Hud.Render.GetUiElement(chatEditLine).ReadText(System.Text.Encoding.UTF8, false).Trim();	// if error, change "UTF8" with "Default"...not tested though
        		lineStr = lineStr.Trim().ToLower();
			Match resetMatch = Regex.Match(lineStr, @"(?=/reset/)") ;  // .+(?=/)
			Match avengersMatch = Regex.Match(lineStr, @"(?=/avengers/)") ;
			Match handsClapMatch = Regex.Match(lineStr, @"(?=/handsclap/)") ;
			Match americanoMatch = Regex.Match(lineStr, @"(?=/americano/)") ;
			Match cubeMatch = Regex.Match(lineStr, @"(?=/cube/)") ;
			//Match cube2Match = Regex.Match(lineStr, @"(?=/cube2/)") ;
			Match babayetuMatch = Regex.Match(lineStr, @"(?=/babayetu/)") ;
			Match anthemMatch = Regex.Match(lineStr, @"(?=/anthem/)") ;
			Match sexyMatch = Regex.Match(lineStr, @"(?=/sexy/)") ;
			Match leahMatch = Regex.Match(lineStr, @"(?=/leah/)") ;
			Match genesisMatch = Regex.Match(lineStr, @"(?=/genesis/)") ;
			Match oriMatch = Regex.Match(lineStr, @"(?=/ori/)") ;
			
			// Random random = new Random();
			// int townedm = random.Next(700, 713);
			
			if (resetMatch.Success)
			{
				cowStatus(false);
				innStatus(false);
				leahStatus(false);
				qStatus(false);
			}

			if (avengersMatch.Success)
			{
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("first.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
			if (handsClapMatch.Success)
			{
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("blighter.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
			if (americanoMatch.Success)
			{
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("bloodmaw.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
			if (cubeMatch.Success)
			{
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("cube.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
			// if (cube2Match.Success)
			// {
				// try
                // {
                   // var soundPlayer = Hud.Sound.LoadSoundPlayer("cube2.wav");
                   // soundPlayer.Play();
                // }
				// catch (Exception){}
			// }
			if (babayetuMatch.Success)
			{
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("babayetu.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
			if (anthemMatch.Success)
			{
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("anthem.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
			if (sexyMatch.Success)
			{
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("agnidox.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
			if (leahMatch.Success)
			{
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("leah.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			}
			if (genesisMatch.Success)
			{
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("515.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
			}	
			if (oriMatch.Success)
			{
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("514.wav");
                   soundPlayer.PlayLooping();
                }
				catch (Exception){}
			}	
		
	   }

	   public void AfterCollect()
        {
            // if (!turnoff) {
			// foreach (var quest in Hud.Game.Quests.Where(quest => quest.SnoQuest.Type == QuestType.Bounty))
					// // 
				// // foreach (SnoQuest.Type == QuestType.Bounty)
					// {
						
						// //if (turnoff) return;
						
						// Random EDM = new Random();
						// int edm = EDM.Next(700,712);
						// if (BountiesToPopup.Contains(quest.SnoQuest))
						// // if (BountiesToPopup.Contains(quest.SnoQuest))
							// {
								
								
								// if (quest.State == QuestState.started)
								// {
									// if (quest.Counter >= 1)  // 이거 됨
									
									// // if (quest.QuestStep != null && quest.QuestStep.SplashEnglish.Contains("Inspect")  || 
									// // //quest.QuestStep.SplashEnglish.Contains("Cursed Chest"))  //|| 
									// // quest.QuestStep.SplashEnglish.Contains("Kill")  || 
									// // quest.QuestStep.BnetTitleEnglish.Contains("Inspect") || 
									// // quest.QuestStep.BnetTextEnglish.Contains("Inspect")) //|| riftQuest.QuestStepId == 3)
									// {
									
									// if (cowStatus(true))
									// {
										// try
										// {
										// var soundPlayer = Hud.Sound.LoadSoundPlayer(edm +".wav");
										// soundPlayer.Play();
										// }
										// catch (Exception){}
									// }
									// }
								// }
								
								// if (quest.State == QuestState.completed)
								// {
																		
									// if (qStatus(true))
									// {
										// try
										// {
										// var soundPlayer = Hud.Sound.LoadSoundPlayer("clear.wav");
										// soundPlayer.Play();
										// }
										// catch (Exception){}
									// }
								// }
								// }
								
					// }
				// }
			
			
			
			if (ResetTimers()) return;

            GuardianTimers();

            if (GamePauseTimers()) return;

            RestartStopTimers();
			
			Random random = new Random();
			int intromusic = random.Next(983, 1000);
			
			// Random rng = new Random();
			// int riftmusic = rng.Next(600, 609);
			
			// Random town = new Random();
			// int tmusic = town.Next(700, 713);
			
			int currentGrLevel = (int)Hud.Game.Me.InGreaterRiftRank;
			
			 if (!turnoff)
			 {
				
			// if (currentRun == SpecialArea.Rift) 
			// {  if (riftRng(true))
				// {
            		// try
	                // {
	                    // var soundPlayer = Hud.Sound.LoadSoundPlayer(riftmusic + ".wav");
	                    // soundPlayer.Play();
	                // }
	                // catch (Exception)
	                // {
	                // }
            	// }
			// }
			
            if(Hud.Game.SpecialArea == SpecialArea.GreaterRift && !IsGuardianDead && IntroSong(true))
            {
            	
            	if(currentGrLevel == 20)
            	{
            		if (Hud.Game.NumberOfPlayersInGame <= 1)
					{
					try
	                {
	                    var soundPlayer = Hud.Sound.LoadSoundPlayer("secondgr.wav");
	                    soundPlayer.PlayLooping();
	                }
	                catch (Exception)
	                {
	                }
					}
            	}
				
				if(currentGrLevel == 70)
            	{
            		if (Hud.Game.NumberOfPlayersInGame <= 1)
					{
					try
	                {
	                    var soundPlayer = Hud.Sound.LoadSoundPlayer("firstgr.wav");
	                    soundPlayer.PlayLooping();
	                }
	                catch (Exception)
	                {
	                }
					}
            	}
				// if(currentGrLevel == 108)
            	// {
            		// try
	                // {
	                    // var soundPlayer = Hud.Sound.LoadSoundPlayer(tmusic + ".wav");
	                    // soundPlayer.Play();
	                // }
	                // catch (Exception)
	                // {
	                // }
            	// }
				
				if(currentGrLevel >= 130)
            	{
            		try
	                {
	                    var soundPlayer = Hud.Sound.LoadSoundPlayer(intromusic + ".wav");
	                    soundPlayer.Play();
	                }
	                catch (Exception)
	                {
	                }
            	}
				
            }

            if (Hud.Game.SpecialArea == SpecialArea.GreaterRift && riftTimer.ElapsedMilliseconds >= 870000 && LastThirtyStatus(true))
            {
            	if(currentGrLevel >= 130)
            	{
            		try
	                {
	                   var soundPlayer = Hud.Sound.LoadSoundPlayer("lastthirty.wav");
	                   soundPlayer.Play();
	                }
	                catch (Exception){}
            	}
            }
			
			//pools & rebang
			
			if (guardianTimer.IsRunning && IsGreaterRift && Hud.Game.Me.IsInTown) 
			{
				if (bfTownStatus(true))
			    {
					if (Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Wizard) // pio )
					{				
						try
	                	{
	                  		var soundPlayer = Hud.Sound.LoadSoundPlayer("newpool.wav");
	                   		soundPlayer.Play();
	                	}
	                	catch (Exception){}
					}
					else  // others
					{
						try
	                	{
	                   		var soundPlayer = Hud.Sound.LoadSoundPlayer("rebang.wav");
	                   		soundPlayer.Play();
	                	}
	                	catch (Exception){}
					}
			  	}
			}
			
			}
						
		}

		//test for cow
		
		// public void Chest(ISnoQuest SnoQuest)
        // {
            // foreach (var quest in Hud.Game.Quests.Where(quest => quest.SnoQuest.Type == QuestType.Bounty))
					// // 
				// // foreach (SnoQuest.Type == QuestType.Bounty)
					// {
						// if (BountiesToPopup.Contains(quest.SnoQuest))
						// // if (BountiesToPopup.Contains(quest.SnoQuest))
							// {
								
								
								// if (quest.State == QuestState.started)
								// {
									// if (//quest.QuestStep.SplashEnglish.Contains("Inspect")  || 
									// quest.QuestStep.SplashEnglish.Contains("Cursed Chest"))  //|| 
									// //quest.QuestStep.SplashEnglish.Contains("Kill")) //  || 
									// //quest.QuestStep.BnetTitleEnglish.Contains("Inspect") || 
									// //quest.QuestStep.BnetTextEnglish.Contains("Inspect")) //|| riftQuest.QuestStepId == 3)
									// {
									
									// if (leahStatus(true))
									// {
										// try
										// {
										// var soundPlayer = Hud.Sound.LoadSoundPlayer("anthem.wav");
										// soundPlayer.Play();
										// }
										// catch (Exception){}
									// }
									// }
								// }
							// }
					// }
			
			// // get
            // // {
                // // return Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 337492) ?? // rift
                       // // Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 382695);   // gr
            // // }
        // }
		
		
		public void OnNewArea(bool newGame, ISnoArea area)
        {
			
			if (!turnoff) {
			
			if (newGame)
            {
				cowStatus(false);
				innStatus(false);
				leahStatus(false);
				riftRng(false);
				qStatus(false);
			}
			// _p2_totallynotacowlevel = 434649   <--- 카우방
			//if (Hud.Game.Me.SnoArea.Act > 1 && Hud.Game.Me.SnoArea.Act < 3 && Hud.Game.Me.IsInTown) // 나중용 마을코드. 정상작동
			//if (Hud.Game.Me.SnoArea.Code.Contains("_trout")) 
		
			if (area.Code.Contains("cow"))
			{	
            	if (cowStatus(true))
				{				
					try
	                {
	                   var soundPlayer = Hud.Sound.LoadSoundPlayer("newcow.wav");
	                   soundPlayer.Play();
	                }
					catch (Exception){}
				} 
			}
			
			
			if (area.Code.Contains("tristram_inn")) //
			{	
            	if (innStatus(true))
				{				
					try
	                {
	                   var soundPlayer = Hud.Sound.LoadSoundPlayer("inn.wav");
	                   soundPlayer.Play();
	                }
					catch (Exception){}
				} 
			}
			
			if (area.Code.Contains("_leahsroom")) //"_leahsroom"
			{	
            	if (leahStatus(true))
				{				
					try
	                {
	                   var soundPlayer = Hud.Sound.LoadSoundPlayer("leahroom.wav");
	                   soundPlayer.Play();
	                }
					catch (Exception){}
				} 
			}
		
			// if (area.Code.Contains("_ruins_frost_city_a_01")) //"_leahsroom"
			// {	
            	// if (leahStatus(true))
				// {				
					// try
	                // {
	                   // var soundPlayer = Hud.Sound.LoadSoundPlayer("frost1.wav");
	                   // soundPlayer.Play();
	                // }
					// catch (Exception){}
				// } 
			// }
		
			if (area.Code.Contains("_ruins_frost_city_a_02")) //"_leahsroom"
			{	
            	if (innStatus(true))
				{				
					try
	                {
	                   var soundPlayer = Hud.Sound.LoadSoundPlayer("frost2.wav");
	                   soundPlayer.Play();
	                }
					catch (Exception){}
				} 
			}
			
			}
			
		}

        
		
		
		private void RestartStopTimers()
        {
            // (re)start/stop timers if needed
            if (!riftTimer.IsRunning && !IsGuardianDead)
                riftTimer.Start();

            if (!guardianTimer.IsRunning && IsGuardianAlive) {
                guardianTimer.Start();
            }

            if (pauseTimer.IsRunning)
                pauseTimer.Stop();

            if (IsGreaterRift && IsGuardianDead && riftTimer.IsRunning)
                riftTimer.Stop();

            if (IsNephalemRift && riftQuest.State == QuestState.completed && riftTimer.IsRunning)
                riftTimer.Stop();
        }

        private bool GamePauseTimers()
        {
            // game pause
            if (Hud.Game.IsPaused || (IsGreaterRift && Hud.Game.NumberOfPlayersInGame == 1 && Hud.Game.IsLoading))
            {
                if (!pauseTimer.IsRunning)
                    pauseTimer.Start();

                if (riftTimer.IsRunning)
                    riftTimer.Stop();

                if (guardianTimer.IsRunning)
                    guardianTimer.Stop();

                return true;
            }
            return false;
        }

        private void GuardianTimers()
        {
            // guardian
            if (IsGuardianAlive)
            {
                if (!guardianTimer.IsRunning)
                    guardianTimer.Start();
            }
            else if (IsGuardianDead && guardianTimer.IsRunning)
            {
                guardianTimer.Stop();
                int currentGrLevel = (int) Hud.Game.Me.InGreaterRiftRank;
                
				 if (!turnoff) 
				 {
				if(riftTimer.ElapsedMilliseconds <= greaterRiftMaxTime && IsGreaterRift && currentGrLevel == 20)
                {
					if (Hud.Game.NumberOfPlayersInGame <= 1)
					{
                    try
                    {
                       var soundPlayer = Hud.Sound.LoadSoundPlayer("victoryL.wav");
                       soundPlayer.Play();
                    }
                    catch (Exception){}
					}
                }
				else if(riftTimer.ElapsedMilliseconds <= greaterRiftMaxTime && IsGreaterRift && currentGrLevel == 70)
                {
					if (Hud.Game.NumberOfPlayersInGame <= 1)
					{
                    try
                    {
                       var soundPlayer = Hud.Sound.LoadSoundPlayer("victoryF.wav");
                       soundPlayer.Play();
                    }
                    catch (Exception){}
					}
                }
				// else if(riftTimer.ElapsedMilliseconds <= greaterRiftMaxTime && IsGreaterRift && currentGrLevel == 90)
                // {
                    // try
                    // {
                       // var soundPlayer = Hud.Sound.LoadSoundPlayer("victoryO.wav");
                       // soundPlayer.Play();
                    // }
                    // catch (Exception){}
                // }
				else if(riftTimer.ElapsedMilliseconds <= greaterRiftMaxTime && IsGreaterRift && currentGrLevel >= 130 && currentGrLevel <= 135) //was <=greaterRiftMaxTime
                {
                    try
                    {
                       var soundPlayer = Hud.Sound.LoadSoundPlayer("victoryL.wav");
                       soundPlayer.Play();
                    }
                    catch (Exception){}
                }
				else if(riftTimer.ElapsedMilliseconds <= greaterRiftMaxTime && IsGreaterRift && currentGrLevel >= 136 && currentGrLevel <= 140) //was <=greaterRiftMaxTime
                {
                    try
                    {
                       var soundPlayer = Hud.Sound.LoadSoundPlayer("victory.wav");
                       soundPlayer.Play();
                    }
                    catch (Exception){}
                }
                else if(riftTimer.ElapsedMilliseconds <= greaterRiftMaxTime && IsGreaterRift && currentGrLevel >= 141 && currentGrLevel <= 145) //was <=greaterRiftMaxTime
                {
                    try
                    {
                       var soundPlayer = Hud.Sound.LoadSoundPlayer("victoryE.wav");
                       soundPlayer.Play();
                    }
                    catch (Exception){}
                }
                else if(riftTimer.ElapsedMilliseconds <= greaterRiftMaxTime && IsGreaterRift && currentGrLevel >= 146 && currentGrLevel <= 149) //was <=greaterRiftMaxTime
                {
                    try
                    {
                       var soundPlayer = Hud.Sound.LoadSoundPlayer("victoryN.wav");
                       soundPlayer.Play();
                    }
                    catch (Exception){}
                }
				else if(riftTimer.ElapsedMilliseconds <= greaterRiftMaxTime && IsGreaterRift && currentGrLevel == 150) //was <=greaterRiftMaxTime
                {
                    try
                    {
                       var soundPlayer = Hud.Sound.LoadSoundPlayer("victoryO.wav");
                       soundPlayer.Play();
                    }
                    catch (Exception){}
                }
                else if (riftTimer.ElapsedMilliseconds >= greaterRiftMaxTime && IsGreaterRift && currentGrLevel >= 130) // was  >= greaterRiftMaxTime
                {
                    try
                    {
                       var soundPlayer = Hud.Sound.LoadSoundPlayer("defeat.wav");
                       soundPlayer.Play();
                    }
                    catch (Exception){}
                }
				
				//for wizard not in GR
				else if (riftTimer.ElapsedMilliseconds <= greaterRiftMaxTime && IsGreaterRift && Hud.Game.SpecialArea != SpecialArea.GreaterRift && Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Wizard) // && Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Wizard)
				{
                    try
                    {
                       var soundPlayer = Hud.Sound.LoadSoundPlayer("victory.wav");
                       soundPlayer.Play();
                    }
                    catch (Exception){}
                }
				else if (riftTimer.ElapsedMilliseconds >= greaterRiftMaxTime && IsGreaterRift && Hud.Game.SpecialArea != SpecialArea.GreaterRift && Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Wizard) // && Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Wizard)
				{
                    try
                    {
                       var soundPlayer = Hud.Sound.LoadSoundPlayer("defeat.wav");
                       soundPlayer.Play();
                    }
                    catch (Exception){}
                }
				else if (riftTimer.ElapsedMilliseconds <= greaterRiftMaxTime && IsGreaterRift && Hud.Game.SpecialArea != SpecialArea.GreaterRift && Hud.Game.Me.HeroClassDefinition.HeroClass != HeroClass.Wizard) // && Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Wizard)
				{
                    try
                    {
                       var soundPlayer = Hud.Sound.LoadSoundPlayer("clear.wav");
                       soundPlayer.Play();
                    }
                    catch (Exception){}
                }
				
				}
			}
        }

        private bool ResetTimers()
        {
            // reset states if needed
            if (riftQuest == null || (riftQuest != null && riftQuest.State == QuestState.none))
            {
                if (riftTimer.IsRunning || riftTimer.ElapsedMilliseconds > 0)
                {
                    riftTimer.Reset();
                    IntroSong(false);
                    LastThirtyStatus(false);
					bfTownStatus(false);
					riftRng(false);
					
					
                }
                if (guardianTimer.IsRunning || guardianTimer.ElapsedMilliseconds > 0)
                {
                    guardianTimer.Reset(); 
                }
                if (pauseTimer.IsRunning || pauseTimer.ElapsedMilliseconds > 0)
                {
                    pauseTimer.Reset();
                }


                currentRun = null;

                return true;
            }
            return false;
        }

        private string GetText(bool onlyTimer)
        {
            textBuilder.Clear();

            SetTimerText(onlyTimer);

            if (onlyTimer)
                return textBuilder.ToString();

            SetProgessText();

            SetClosingTimerText();

            return textBuilder.ToString();
        }

        private void SetClosingTimerText()
        {
            if (!ShowClosingTimer || IsGreaterRift || riftQuest.State != QuestState.completed) return;

            textBuilder.Append(" ");
            textBuilder.AppendFormat(ClosingSecondsFormat, TimeSpan.FromMilliseconds(riftClosingMilliseconds - riftQuest.CompletedOn.ElapsedMilliseconds));
        }

        private void SetProgessText()
        {
            if (Hud.Game.RiftPercentage < 100)
            {
                if ((!IsNephalemRift && uiProgressBar.Visible) || !(Hud.Game.RiftPercentage > 0.1)) return;

                textBuilder.Append(" ");
                textBuilder.AppendFormat(CultureInfo.InvariantCulture, ProgressPercentFormat, Hud.Game.RiftPercentage);
            }
            else if (IsGuardianAlive || IsGuardianDead || !guardianTimer.IsRunning)
            {
                textBuilder.Append(" ");
                textBuilder.Append(IsGuardianAlive ? GuardianAliveSymbol : GuardianDeadSymbol);

                var guardianTimeSpan = TimeSpan.FromMilliseconds(guardianTimer.ElapsedMilliseconds);
                textBuilder.Append(" ");
                textBuilder.AppendFormat(CultureInfo.InvariantCulture, (guardianTimeSpan.Minutes < 1) ? SecondsFormat : MinutesSecondsFormat, guardianTimeSpan);
            }
        }

        private void SetTimerText(bool onlyTimer)
        {
            if (!onlyTimer && !string.IsNullOrWhiteSpace(ObjectiveProgressSymbol))
            {
                if (currentRun == SpecialArea.Rift)
                {
                    textBuilder.Append(ObjectiveProgressSymbol);
                    textBuilder.Append(" ");
                }
                else if (currentRun == SpecialArea.GreaterRift &&
                         (ShowGreaterRiftTimer || !uiProgressBar.Visible || (ShowGreaterRiftCompletedTimer && IsGuardianDead)))
                {
                    textBuilder.Append(ObjectiveProgressSymbol);
                    textBuilder.Append(" ");
                }
            }

            if (currentRun == SpecialArea.GreaterRift)
            {
                if (ShowGreaterRiftTimer || !uiProgressBar.Visible || (ShowGreaterRiftCompletedTimer && IsGuardianDead))
                {
                    var timeSpan = GreaterRiftCountdown && !IsGuardianDead
                        ? TimeSpan.FromMilliseconds(greaterRiftMaxTime - riftTimer.ElapsedMilliseconds)
                        : TimeSpan.FromMilliseconds(riftTimer.ElapsedMilliseconds);

                    textBuilder.AppendFormat(CultureInfo.InvariantCulture, (timeSpan.Minutes < 1) ? SecondsFormat : MinutesSecondsFormat, timeSpan);
                }
            }
            else
            {
                var timeSpan = TimeSpan.FromMilliseconds(riftQuest.StartedOn.ElapsedMilliseconds - riftQuest.CompletedOn.ElapsedMilliseconds - pauseTimer.ElapsedMilliseconds);
                textBuilder.AppendFormat(CultureInfo.InvariantCulture, (timeSpan.Minutes < 1) ? SecondsFormat : MinutesSecondsFormat, timeSpan);
            }
        }

        private bool IntroSong (bool introSong) 
		{
			if(introSong)
			{
				if(!introSongStatus) {
					introSongStatus = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(introSongStatus) {
					introSongStatus = false;
				}
				return false;
			}
		}

		private bool LastThirtyStatus (bool lastThirtyCheck) 
		{
			if(lastThirtyCheck)
			{
				if(!lastThirtyStatus) {
					lastThirtyStatus = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(lastThirtyStatus) {
					lastThirtyStatus = false;
				}
				return false;
			}
		}
    
		private bool bfTownStatus (bool BFTownCheck) 
		{
			if(BFTownCheck)
			{
				if(!BFTownStatus) {
					BFTownStatus = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(BFTownStatus) {
					BFTownStatus = false;
				}
				return false;
			}
		}
	
		private bool cowStatus (bool CowCheck) 
		{
			if(CowCheck)
			{
				if(!CowStatus) {
					CowStatus = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(CowStatus) {
					CowStatus = false;
				}
				return false;
			}
		}
	
		private bool innStatus (bool InnCheck) 
		{
			if(InnCheck)
			{
				if(!InnStatus) {
					InnStatus = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(InnStatus) {
					InnStatus = false;
				}
				return false;
			}
		}
	
		private bool leahStatus (bool LeahCheck) 
		{
			if(LeahCheck)
			{
				if(!LeahStatus) {
					LeahStatus = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(LeahStatus) {
					LeahStatus = false;
				}
				return false;
			}
		}
		
		private bool qStatus (bool QCheck) 
		{
			if(QCheck)
			{
				if(!QStatus) {
					QStatus = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(QStatus) {
					QStatus = false;
				}
				return false;
			}
		}
		
		private bool riftRng (bool RiftCheck) 
		{
			if(RiftCheck)
			{
				if(!RiftRng) {
					RiftRng = true;
					return true;
				}
				return false;
			} 
			else 
			{
				if(RiftRng) {
					RiftRng = false;
				}
				return false;
			}
		}
	
	}
}