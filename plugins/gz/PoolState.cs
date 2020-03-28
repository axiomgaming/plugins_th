using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.gz
{
	public class PoolState : BasePlugin, IInGameTopPainter, IAfterCollectHandler
	{
		public int DeathsTotal { get; private set; }
		public int DeathsInRift { get; private set; }
		private SpecialArea? currentRun;
		private bool alive;
		
		public string DeathsTotalSymbol { get; set; }
		public string DeathsInRiftSymbol { get; set; }
		public string HasPoolSymbol { get; set; }
		public string EmptyPoolSymbol { get; set; }
		
		public bool IsNephalemRift { get{ return riftQuest != null && (riftQuest.QuestStepId == 1 || riftQuest.QuestStepId == 3 || riftQuest.QuestStepId == 10); } }
		public bool IsGreaterRift { get{ return riftQuest != null && (riftQuest.QuestStepId == 13 || riftQuest.QuestStepId == 16 || riftQuest.QuestStepId == 34 || riftQuest.QuestStepId == 46); } }
		private IQuest riftQuest { get{ return Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 337492) /*rift*/ ?? Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 382695); /*gr*/ } }
		
		public IFont PortraitInfoFont { get; set; }
        public float OffsetXmultiplier { get; set; }
        public float OffsetYmultiplier { get; set; }
		
		public bool ShowDeathCounter { get; set; }
		
		public PoolState()
		{
			Enabled = true;
			
			ShowDeathCounter = true;
		}
		
		public override void Load(IController hud)
		{
			base.Load(hud);
			
			DeathsTotal = 0;
			DeathsInRift = 0;
			alive = true;
			
			DeathsTotalSymbol = "\u2620";
			DeathsInRiftSymbol = "\ud83d\udd48";
			HasPoolSymbol = "\u2B1F";
			EmptyPoolSymbol = "\u2B20";
			
			PortraitInfoFont = Hud.Render.CreateFont("tahoma", 7f, 255, 180, 147, 109, false, false, true);
			OffsetXmultiplier = 0.05f;
			OffsetYmultiplier = 0.117f;
		}
		
		public void AfterCollect()
		{
			if (riftQuest == null || (riftQuest != null && riftQuest.State == QuestState.none))
			{
				DeathsInRift = 0;
				currentRun = null;
			}
		}
		
		public void PaintTopInGame(ClipState clipState)
		{
			if (clipState != ClipState.BeforeClip) return;
			
			if (currentRun == null)
			{
				currentRun = IsNephalemRift ? SpecialArea.Rift : SpecialArea.GreaterRift;
			}
			
			CheckDeathState();
			foreach (IPlayer player in Hud.Game.Players)
			{
				DrawPlayerInfo(player);
			}
		}
		
		private string GetPlayerInfoText(IPlayer player)
		{	
			if (!ShowDeathCounter || !player.IsMe)
				return String.Format("{1} {0:0.##}", (player.BonusPoolRemaining > 0 ? 10*((float)player.BonusPoolRemaining / player.ParagonExpToNextLevel) : 0f), (player.BonusPoolRemaining > 0 ? HasPoolSymbol : EmptyPoolSymbol));
				
			if (IsGreaterRift)
				return String.Format("{1} {0:0.##}\t{3} {2}  {5} {4}  {6}s", (player.BonusPoolRemaining > 0 ? 10*((float)player.BonusPoolRemaining / player.ParagonExpToNextLevel) : 0f), 
										(player.BonusPoolRemaining > 0 ? HasPoolSymbol : EmptyPoolSymbol), DeathsTotal, DeathsTotalSymbol, DeathsInRift, DeathsInRiftSymbol, (DeathsInRift > 5 ? 30 : DeathsInRift*5)); 
				
			return String.Format("{1} {0:0.##}\t{3} {2}", (player.BonusPoolRemaining > 0 ? 10*((float)player.BonusPoolRemaining / player.ParagonExpToNextLevel) : 0f),
										(player.BonusPoolRemaining > 0 ? HasPoolSymbol : EmptyPoolSymbol), DeathsTotal, DeathsTotalSymbol); 
		}
		
		private void DrawPlayerInfo(IPlayer player)
		{
			var OffsetX = Hud.Window.Size.Width * OffsetXmultiplier;
			var OffsetY = Hud.Window.Size.Height * OffsetYmultiplier;
			var portraitRect = player.PortraitUiElement.Rectangle;
			var YPos = portraitRect.Y + OffsetY;
			var XPos = portraitRect.X + OffsetX;
			
			var Layout = PortraitInfoFont.GetTextLayout(GetPlayerInfoText(player));
			PortraitInfoFont.DrawText(Layout, XPos, YPos);
		}
		
		private void CheckDeathState()
		{
			var me = Hud.Game.Me; //player
			if(me.IsDeadSafeCheck && alive)
			{
				DeathsTotal++;
				if (currentRun != null)
					DeathsInRift++;
				alive = !alive;
			}
			if (!me.IsDeadSafeCheck && !alive)
				alive = !alive;
		}
	}
}