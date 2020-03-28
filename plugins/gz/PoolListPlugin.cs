using System.Linq;
using System.Collections.Generic;
using SharpDX.DirectInput;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.gz
{
    public class PoolListPlugin : BasePlugin, IInGameTopPainter, IAfterCollectHandler, INewAreaHandler, IKeyEventHandler
    {
		public Dictionary<int, List<string>> PoolList { get; set; }
		public Dictionary<string, TopLabelDecorator> PoolDecorators { get; set; }
		
		public Dictionary<int, List<string>> OperatedPoolList { get; set; }
		public Dictionary<string, TopLabelDecorator> OperatedPoolDecorators { get; set; }
		
		public bool Show { get; set; }
		public IKeyEvent ToggleKeyEvent { get; set; }
		
		public float PosXMultiplier { get; set; }
		public float PosYMultiplier { get; set; }
		
		public bool ShowOperated { get; set; }
		public ITexture OperatedPoolTexture { get; set; }
		public ITexture PoolTexture { get; set; }
		
        public PoolListPlugin()
        {
            Enabled = true;
			
			Show = false;
			ShowOperated = true;
        }
		
        public override void Load(IController hud)
        {
            base.Load(hud);
			
			PoolList = new Dictionary<int, List<string>>();
			OperatedPoolList = new Dictionary<int, List<string>>();
			for (int i = 0; i <= 5; i++)
			{
				PoolList.Add(i, new List<string>());
				OperatedPoolList.Add(i, new List<string>());
			}
			
			PoolDecorators = new Dictionary<string, TopLabelDecorator>();
			OperatedPoolDecorators = new Dictionary<string, TopLabelDecorator>();
			
			PoolTexture = Hud.Texture.BackgroundTextureBlue;
			OperatedPoolTexture = Hud.Texture.BackgroundTextureOrange;
			
			ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.F6, false, false, false);
        }
		
		private TopLabelDecorator CreatePoolListLocationLabel(string text, bool operated)
		{
			return new TopLabelDecorator(Hud)
			{
				TextFont = Hud.Render.CreateFont("tahoma", 8, 120, 255, 255, 255, true, false, false),
				// Option #2: Use textures for background
				BackgroundTexture1 = Hud.Texture.ButtonTextureGray,
                BackgroundTexture2 = (operated ? OperatedPoolTexture : PoolTexture),
                BackgroundTextureOpacity2 = 0.25f,
				//delegate
                HintFunc = () => "", //delegate string
                TextFunc = () => text
			};
		}
		
		public void OnNewArea(bool newGame, ISnoArea area)
        {
            if (newGame)
            {
                Show = false;
				
				//reset
				for (int i = 0; i <= 5; i++)
				{
					PoolList[i].Clear();
					OperatedPoolList[i].Clear();
				}
				
				PoolDecorators.Clear();
				OperatedPoolDecorators.Clear();
            }
        }
		
		public void AfterCollect()
        {
			if (Hud.Game.Me.SnoArea == null || Hud.Game.Me.SnoArea.Code == null || Hud.Game.Me.SnoArea.Code.Contains("x1_lr_level"))
				return;
				
			var shrines = Hud.Game.Shrines.Where(actor => actor.Type == ShrineType.PoolOfReflection);
			
			foreach (var shrine in shrines)
			{
				if (shrine.Scene.SnoArea == null)
					continue;
				else if (shrine.Scene.SnoArea.NameLocalized == null)
					continue;
				else if (shrine.Scene.SnoArea.Act < 0 || shrine.Scene.SnoArea.Act > 5)
					continue;
				
				if (shrine.IsClickable && !shrine.IsOperated)
				{
					if (!PoolDecorators.Keys.Contains(shrine.Scene.SnoArea.NameLocalized))
					{
						PoolList[shrine.Scene.SnoArea.Act].Add(shrine.Scene.SnoArea.NameLocalized);
						PoolDecorators.Add(shrine.Scene.SnoArea.NameLocalized, CreatePoolListLocationLabel(shrine.Scene.SnoArea.NameLocalized, shrine.IsOperated));
					}
				}
				
				if (shrine.IsClickable && shrine.IsOperated)
				{
					if (PoolDecorators.Keys.Contains(shrine.Scene.SnoArea.NameLocalized))
					{
						PoolList[shrine.Scene.SnoArea.Act].Remove(shrine.Scene.SnoArea.NameLocalized);
						PoolDecorators.Remove(shrine.Scene.SnoArea.NameLocalized);
					}
					
					if (!OperatedPoolDecorators.Keys.Contains(shrine.Scene.SnoArea.NameLocalized))
					{
						OperatedPoolList[shrine.Scene.SnoArea.Act].Add(shrine.Scene.SnoArea.NameLocalized);
						OperatedPoolDecorators.Add(shrine.Scene.SnoArea.NameLocalized, CreatePoolListLocationLabel(shrine.Scene.SnoArea.NameLocalized, shrine.IsOperated));
					}
				}
			}
		}
 
        public void PaintTopInGame(ClipState clipState)
        {
			if (clipState != ClipState.BeforeClip) return;
			if (!Show) return;
			
			var PoolX = Hud.Window.Size.Width / 6f;
			var PoolW = Hud.Window.Size.Width / 8f;
			
			var PoolY = Hud.Window.Size.Height * 61f / 108f;
			var PoolH = Hud.Window.Size.Height * 0.025f;
			
			var spacingX = Hud.Window.Size.Width / 100f;
			var spacingY = Hud.Window.Size.Height / 216f;
			
			for (int act = 0; act <= 5; act++)
			{
				int Row = 0;
				foreach (var pool in PoolList[act])
				{
					PoolDecorators[pool].Paint(PoolX + (act - 1) * (PoolW + spacingX), PoolY + Row * (PoolH + spacingY), PoolW, PoolH, HorizontalAlign.Center);
					Row++;
				}
				
				if (!ShowOperated)
					continue;
				
				if (Row > 0)
					Row++;
				
				foreach (var operatedPool in OperatedPoolList[act])
				{
					OperatedPoolDecorators[operatedPool].Paint(PoolX + (act - 1) * (PoolW + spacingX), PoolY + Row * (PoolH + spacingY), PoolW, PoolH, HorizontalAlign.Center);
					Row++;
				}
			}
        }
		
		public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (keyEvent.IsPressed && ToggleKeyEvent.Matches(keyEvent))
            {
                Show = !Show;
            }
        }
    }
}