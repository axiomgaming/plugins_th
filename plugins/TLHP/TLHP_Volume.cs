// /볼륨 ?/ /volume ?/
using System;
using Turbo.Plugins.Default;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using SharpDX.DirectInput;
using System.Collections.Generic;
using System.Linq;

namespace Turbo.Plugins.TLHP
{
    public class TLHP_Volume : BasePlugin, INewAreaHandler, IKeyEventHandler, IInGameTopPainter
    {
		private string chatEditLine = "Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline";
		private static System.Timers.Timer ReadEditLineTimer;
		private int MasterVolume;
		private int savedVolume;
		private string culture;
		private readonly int[] _skillOrder = { 2, 3, 4, 5, 0, 1 };
		public SharpDX.DirectInput.Key Key { get; set; }
		public SharpDX.DirectInput.Key Key1 { get; set; }
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
		public SharpDX.DirectInput.Key Key12 { get; set; }
		
		bool SimBuffActive = false;
		bool manual = false;
		bool melon = false;


		public TLHP_Volume()
        	{
        		Enabled = true;
        	}

        	public override void Load(IController hud)
        	{
            	base.Load(hud);

			
			Key = SharpDX.DirectInput.Key.D0;
			Key1 = SharpDX.DirectInput.Key.Minus;
			Key2 = SharpDX.DirectInput.Key.Equals;
			Key3 = SharpDX.DirectInput.Key.Left;
			Key4 = SharpDX.DirectInput.Key.Right;
			Key5 = SharpDX.DirectInput.Key.Subtract;
			Key6 = SharpDX.DirectInput.Key.Add;
			Key7 = SharpDX.DirectInput.Key.Up;
			Key8 = SharpDX.DirectInput.Key.Down;
			Key9 = SharpDX.DirectInput.Key.NumberPad4;
			Key10 = SharpDX.DirectInput.Key.NumberPad1;
			Key11 = SharpDX.DirectInput.Key.NumberPad2;
			Key12 = SharpDX.DirectInput.Key.NumberPad3;
			
			
			
			
			Hud.Sound.VolumeMode = VolumeMode.Constant;
			MasterVolume = 30;
			savedVolume = 0;
			culture = System.Globalization.CultureInfo.CurrentCulture.ToString().Substring(0, 2);

		    ReadEditLineTimer = new System.Timers.Timer();
			ReadEditLineTimer.Interval = 500;		// edit line filtering interval
			ReadEditLineTimer.Elapsed += ReadEditLine;
			ReadEditLineTimer.AutoReset = true;
			ReadEditLineTimer.Enabled = true;
	   	}

		public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;

            foreach (IPlayer player in Hud.Game.Players)
            {
                lastboss(player);
            }

        }
		
		
		public void OnKeyEvent(IKeyEvent keyEvent)
        {
            			
					
			// if (keyEvent.Key == Key && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            // {
				
				
				// int yup = 20;
				// MasterVolume = yup;
				
				// if (MasterVolume != savedVolume)
				// {
					// savedVolume = MasterVolume;
					// Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				// }
			
			
			
			// }
			
			// if (keyEvent.Key == Key1 && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            // {

				// MasterVolume = MasterVolume - 10;
				// if(MasterVolume < 1) {
					// MasterVolume = 10;
				// }
				// if (MasterVolume != savedVolume)
				// {
					// savedVolume = MasterVolume;
					// Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				// }
			// }
			
			// if (keyEvent.Key == Key2 && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            // {
				// MasterVolume = MasterVolume + 10;
				// if(MasterVolume > 100) {
					// MasterVolume = 100;
				// }

				// if (MasterVolume != savedVolume)
				// {
					// savedVolume = MasterVolume;
					// Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				// }
			// }
			
			if (keyEvent.Key == Key3 && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            {

				MasterVolume = MasterVolume - 10;
				if(MasterVolume < 1) {
					MasterVolume = 10;
				}
				if (MasterVolume != savedVolume)
				{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				}
			}
			
			if (keyEvent.Key == Key4 && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            {
				MasterVolume = MasterVolume + 10;
				if(MasterVolume > 100) {
					MasterVolume = 100;
				}

				if (MasterVolume != savedVolume)
				{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				}
			}
			if (keyEvent.Key == Key5 && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            {

				MasterVolume = MasterVolume - 1;
				if(MasterVolume < 1) {
					MasterVolume = 10;
				}
				if (MasterVolume != savedVolume)
				{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				}
			}
			
			if (keyEvent.Key == Key6 && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            {
				MasterVolume = MasterVolume + 1;
				if(MasterVolume > 100) {
					MasterVolume = 100;
				}

				if (MasterVolume != savedVolume)
				{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				}
			}
			
			if (keyEvent.Key == Key7 && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            {
				manual = true;
				
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("manual.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
				
			}
			if (keyEvent.Key == Key8 && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            {
				manual = false;
				
				try
                {
                   var soundPlayer = Hud.Sound.LoadSoundPlayer("auto.wav");
                   soundPlayer.Play();
                }
				catch (Exception){}
				
				
			}
			// if (keyEvent.Key == Key9 && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            // {
				
				// melon = true;
				// int yup = 2;
				// MasterVolume = yup;
				
				// if (MasterVolume != savedVolume)
				// {
					// savedVolume = MasterVolume;
					// Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				// }
			
			// }
			
			// if (keyEvent.Key == Key10 && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            // {
				// melon = false;
				// int yup = 10;
				// MasterVolume = yup;
				
				// if (MasterVolume != savedVolume)
				// {
					// savedVolume = MasterVolume;
					// Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				// }
			// }
			// if (keyEvent.Key == Key11 && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            // {
				// //melon = false;
				// int yup = 10;
				// MasterVolume = yup;
				
				// if (MasterVolume != savedVolume)
				// {
					// savedVolume = MasterVolume;
					// Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				// }
			// }
			// if (keyEvent.Key == Key12 && !keyEvent.IsPressed) //&& ToggleKeyEvent.Matches(keyEvent))
            // {
				// //melon = false;
				// int yup = 10;
				// MasterVolume = yup;
				
				// if (MasterVolume != savedVolume)
				// {
					// savedVolume = MasterVolume;
					// Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				// }
			// }
		}
		
		public void OnNewArea(bool newGame, ISnoArea area)
        {
			if (manual) return;
			//if (!melon) {
			
			if (area.Code.Contains("_lr"))
            {
				
				int yup = 10;
				MasterVolume = yup;
				
				if (MasterVolume != savedVolume)
				{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				}
			}
		//	}
			
			// else if (melon) {
			
			// if (area.Code.Contains("_lr"))
            // {
				
				// int yup = 2;
				// MasterVolume = yup;
				
				// if (MasterVolume != savedVolume)
				// {
					// savedVolume = MasterVolume;
					// Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				// }
			// }
			// }
			
			
		}
		
		private void lastboss (IPlayer player)
		{
            //if (player == null || player.HeroClassDefinition.HeroClass != HeroClass.Necromancer) return;
            if (Hud.Game.SpecialArea != SpecialArea.GreaterRift) return;
			
			if (manual) return;
			
			bool PoisonSkeleton = false;
			int monsterCount = Hud.Game.AliveMonsters.Count(m => m.SnoMonster.Priority == MonsterPriority.boss);
            int currentGrLevel = (int)Hud.Game.Me.InGreaterRiftRank;
			
		
			foreach (var usedSkills in player.Powers.UsedSkills)
                {
                    if (usedSkills.RuneNameEnglish == "Kill Command")
                    {
                        PoisonSkeleton = true;
                    }
                }
				
			if (currentGrLevel == 20 && monsterCount == 1)
			{
				if (Hud.Game.NumberOfPlayersInGame <= 1)
				{
					int yap = 30;
					MasterVolume = yap;
				
					if (MasterVolume != savedVolume)
					{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
					}
				}
			}
			
			if (currentGrLevel == 70 && monsterCount == 1)
			{
				if (Hud.Game.NumberOfPlayersInGame <= 1)
				{
					int yap = 35;
					MasterVolume = yap;
				
					if (MasterVolume != savedVolume)
					{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
					}
				}
			}
			
			if (currentGrLevel >= 130 && monsterCount == 1)
            {
				if (player == null || player.HeroClassDefinition.HeroClass != HeroClass.Necromancer) return;
				SimBuffActive = false;
				
				foreach (var i in _skillOrder)
                {
                    var skill = player.Powers.SkillSlots[i];
                    if (skill == null || skill.SnoPower.Sno != 465350) continue;

                    if (player.Powers.BuffIsActive(465350, 1)) //Necromancer_Simulacrum { get; }
                    {
                        SimBuffActive = true;
                    }
				
				
				if (PoisonSkeleton)
				{	
					int yap = 25;
					MasterVolume = yap;
				
					if (MasterVolume != savedVolume)
					{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
					}
				}
				
				else if (!PoisonSkeleton & SimBuffActive)
				{	
					int yap = 20;
					MasterVolume = yap;
				
					if (MasterVolume != savedVolume)
					{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
					}
				}
				
				else if (!PoisonSkeleton & !SimBuffActive)
				{	
					int yap = 45;
					MasterVolume = yap;
				
					if (MasterVolume != savedVolume)
					{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
					}
				}
				
				}

			}
		}
			
			
		
		public void ReadEditLine(Object source, System.Timers.ElapsedEventArgs e)
        	{
        		// chat edit line
        		if (!Hud.Render.GetUiElement(chatEditLine).Visible)
        			return;

			int tmp = 0;
			string defaultVal = string.Empty;
        		string lineStr = Hud.Render.GetUiElement(chatEditLine).ReadText(System.Text.Encoding.UTF8, false).Trim();	// if error, change "UTF8" with "Default"...not tested though
        		lineStr = lineStr.Trim().ToLower();
	        	if (String.Equals(lineStr, "/volume/") || String.Equals(lineStr, "/music/") || String.Equals(lineStr, "/Volume/") || String.Equals(lineStr, "/sound/") || String.Equals(lineStr, "/볼륨/") )
			{
				//Hud.Sound.VolumeMode = VolumeMode.AutoMasterAndEffects;
				int vol = 0;	
				if (savedVolume == 0)
				{
					vol = (int)(Hud.Sound.IngameMasterVolume * Hud.Sound.IngameEffectsVolume * Hud.Sound.VolumeMultiplier / 100);
					Hud.Sound.ConstantVolume = vol;
				} else
					vol = savedVolume;
				if (Hud.Sound.LastSpeak.TimerTest(5000))
					
				return;
			}
			
        		Match e1match = Regex.Match(lineStr, @"(?<=/volume ).+(?=/)") ;
				Match e2match = Regex.Match(lineStr, @"(?<=/Volume ).+(?=/)") ;
				Match e3match = Regex.Match(lineStr, @"(?<=/music ).+(?=/)")  ;
				Match e4match = Regex.Match(lineStr, @"(?<=/sound ).+(?=/)")  ; 
				Match kmatch = Regex.Match(lineStr, @"(?=/tt/)")  ;// Regex.Match(lineStr, @"(?<=/volume ).+(?=/)");
			if (e1match.Success) // || e2match.Success || e3match.Success || e4match.Success)	// in the edit line, should type "/volume n/" <- n is from 0 to 100.
			{
				if (Char.IsDigit(e1match.Value[0]))
				{
					tmp = Int32.Parse(e1match.Value);
					if (tmp < 1 || tmp > 	100)
					{
						MasterVolume = 30;			// default volume
						defaultVal = (culture == "ko") ? "기본값 " : "default value ";
					} else
						MasterVolume = tmp;
				}
				if (MasterVolume != savedVolume)
				{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				}
			}
			
			if (e2match.Success)
			{
				if  (Char.IsDigit(e2match.Value[0]))
				{
					tmp = Int32.Parse(e2match.Value);
					if (tmp < 1 || tmp > 100)
					{
						MasterVolume = 30;			// default volume
						defaultVal = (culture == "ko") ? "기본값 " : "default value ";
					} else
						MasterVolume = tmp;
				}
				if (MasterVolume != savedVolume)
				{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 1001
				}
			}
			if (e3match.Success)
			{
				if  (Char.IsDigit(e3match.Value[0]))
				{
					tmp = Int32.Parse(e3match.Value);
					if (tmp < 1 || tmp > 100)
					{
						MasterVolume = 30;			// default volume
						defaultVal = (culture == "ko") ? "기본값 " : "default value ";
					} else
						MasterVolume = tmp;
				}
				if (MasterVolume != savedVolume)
				{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				}
			}
			
			if (e4match.Success)
			{
				if  (Char.IsDigit(e4match.Value[0]))
				{
					tmp = Int32.Parse(e4match.Value);
					if (tmp < 1 || tmp > 100)
					{
						MasterVolume = 30;			// default volume
						defaultVal = (culture == "ko") ? "기본값 " : "default value ";
					} else
						MasterVolume = tmp;
				}
				if (MasterVolume != savedVolume)
				{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				}
			

			}
			
			if (kmatch.Success)
			{
				
					int yup = 20;
					MasterVolume = yup;
				
				if (MasterVolume != savedVolume)
				{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100
				}
			

	
			
        	}
		}
	}	
}