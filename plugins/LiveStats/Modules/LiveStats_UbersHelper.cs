// TopStats Ubers Helper by HaKache

using Turbo.Plugins.Default;
using System.Drawing;
using SharpDX.DirectInput;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Threading;

namespace Turbo.Plugins.LiveStats.Modules
{
    public class LiveStats_UbersHelper : BasePlugin, ICustomizer, IMonsterKilledHandler, INewAreaHandler, IInGameTopPainter
    {
 	public int UberHistoryTimeout { get; set; } = 120;	// Stop showing old uber kills after this many seconds (0 = never time out)
	public int UberHistoryHighlight { get; set; } = 10;	// Highlight if the uber kill is new for this many seconds
	public int UberHistoryShown { get; set; } = 10;		// Max # of ubers to show for uber history
	public List<UberSnapshot> UberHistory { get; private set; }
		
	public TopLabelDecorator Label { get; set; }
	public List<TopLabelDecorator> ExpandUpLabels { get; private set; }
	
	public IFont UberFont { get; set; }
	public IFont BossFont { get; set; }
	public IFont TimeFont { get; set; }

	public IBrush BgBrush { get; set; }
	public IBrush BgBrushAlt { get; set; }
	public IBrush HighlightBrush { get; set; }

	public int TotalAct1 { get; private set; } = 0;
	public bool Act1_Clear { get; private set; } = false;
	public bool Act1_King { get; private set; } = false;
	public bool Act1_Witch { get; private set; } = false;

	public int TotalAct2 { get; private set; } = 0;
	public bool Act2_Clear { get; private set; } = false;
	public bool Act2_Ghom { get; private set; } = false;
	public bool Act2_Despair { get; private set; } = false;

	public int TotalAct3 { get; private set; } = 0;
	public bool Act3_Clear { get; private set; } = false;
	public bool Act3_Undying { get; private set; } = false;
	public bool Act3_Behemoth { get; private set; } = false;

	public int TotalAct4 { get; private set; } = 0;
	public bool Act4_Clear { get; private set; } = false;
	public bool Act4_Diablo { get; private set; } = false;
	public bool Act4_FirstSpawn { get; private set; } = false;
	public bool Act4_SecondSpawn { get; private set; } = false;

	public int TotalClears { get; private set; } = 0;

        private List<uint> UberMap { get; set; }

	public class UberSnapshot {
		public string Id { get; set; }
		public DateTime Timestamp { get; set; }
		public string BossName { get; set; }
		public string Difficulty { get; set; }
	    }

        public int Priority { get; set; } = 100;
        public int Hook { get; set; } = 1;
		
        public LiveStats_UbersHelper()
	{
	    Enabled = true;
	}
		
	public void Customize()
	{
	    // Add this display to the LiveStats readout with a(n optional) specified positional order priority
	    Hud.RunOnPlugin<LiveStatsPlugin>(plugin => {
		plugin.Add(this.Label, this.Priority, this.Hook);
	    });
	}

        public override void Load(IController hud)
        {
            base.Load(hud);

	    UberHistory = new List<UberSnapshot>();

            UberMap = new List<uint> { 257116, 256767, 256106, 256742, 374239 };

	    TimeFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 255, 255, false, false, true);
	    UberFont = Hud.Render.CreateFont("tahoma", 7, 255, 134, 67, 75, false, false, true); //205, 102, 255
	    BossFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 0, 0, false, false, true);
			
	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
	    BgBrush = plugin.BgBrush;
	    BgBrushAlt = plugin.BgBrushAlt;
	    HighlightBrush = Hud.Render.CreateBrush(200, 72, 132, 84, 0);
			
            Label = new TopLabelDecorator(Hud)
            {
		Enabled = false,
                TextFont = UberFont,
		TextFunc = () => Act1_Clear && Act2_Clear && Act3_Clear && Act4_Clear ? "4/4 ubers" : (TotalClears > 1 ? TotalClears.ToString() + " ubers runs" : TotalClears.ToString() + " ubers run"),
                HintFunc = () => "Ubers Runs Done",
                ExpandUpLabels = new List<TopLabelDecorator>() {
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = UberFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  UberFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => Act1_Clear ? TotalAct1.ToString() + "\u2800\u2714" : TotalAct1.ToString() + " \u2800 \u2800",
                        HintFunc = () => "Act I Clears",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = UberFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  UberFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => Act2_Clear ? TotalAct2.ToString() + "\u2800\u2714" : TotalAct2.ToString() + " \u2800 \u2800",
                        HintFunc = () => "Act II Clears",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = UberFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  UberFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => Act3_Clear ? TotalAct3.ToString() + "\u2800\u2714" : TotalAct3.ToString() + " \u2800 \u2800",
                        HintFunc = () => "Act III Clears",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = UberFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  UberFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => Act4_Clear ? TotalAct4.ToString() + "\u2800\u2714" : TotalAct4.ToString() + " \u2800 \u2800",
                        HintFunc = () => "Act IV Clears",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = UberFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  UberFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => Act1_Clear && Act2_Clear && Act3_Clear && Act4_Clear ? TotalClears.ToString() + "\u2800\u2714" : TotalClears.ToString() + " \u2800 \u2800",
                        HintFunc = () => "Full Clears",
                    },
		    new TopLabelDecorator(Hud) {
			TextFont = UberFont,
			TextFunc = () => {
			float w = plugin.WidthFunc();
			float h = plugin.HeightFunc();
			float x = Hook == 0 ? (plugin.PinnedLabel == Label ? plugin.ExpandLabelHook : plugin.SelectedRectangle.X) : (plugin.PinnedTopLabel == Label ? plugin.ExpandLabelTopHook : plugin.SelectedTopRectangle.X);
			float y = Hook == 0 ? (plugin.SelectedRectangle.Y - h * (ExpandUpLabels.Count - 0.5f)) : (plugin.SelectedTopRectangle.Y + h * (ExpandUpLabels.Count - 0.5f));
						
			DrawUberHistory(new RectangleF(x, y, w, h));
							
			return " ";
			    },
			},
		    },
		};
			
		ExpandUpLabels = Label.ExpandUpLabels;
	}

        public void PaintTopInGame(ClipState clipState)
        {

	    // Initialize the Uber Menu Label only under certain circumstances
	    Label.Enabled = UberMap.Contains(Hud.Game.Me.SnoArea.Sno) ? true : ((Act1_Clear || Act2_Clear || Act3_Clear || Act4_Clear || TotalClears > 0) ? true : false);

	}

	public void OnNewArea(bool newGame, ISnoArea area)
	{

	    // If you killed a boss and then died before killing the last, we reset all the temporary Boss Kills variables
	    Act1_King = false; Act1_Witch = false;
	    Act2_Ghom = false; Act2_Despair = false;
	    Act3_Behemoth = false; Act3_Undying = false;
	    Act4_Diablo = false; Act4_FirstSpawn = false; Act4_SecondSpawn = false;

	    // If you enter a new game, we reset all the temporary Act Clears variables
	    if (newGame)
	    {
		Act1_Clear = false;
		Act2_Clear = false;
		Act3_Clear = false;
		Act4_Clear = false;
	    }
	}

	public void OnMonsterKilled(IMonster monster)
	{
	if (monster.Rarity != ActorRarity.Boss) return;
	if (monster.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_None, 375929) == 1) return; // Uber Diablo Mirror Image...
	if (!UberMap.Contains(Hud.Game.Me.SnoArea.Sno)) return; // Return if you're not in a Uber Zone

	// Act 1
	if (monster.SnoActor.Sno == ActorSnoEnum._uber_skeletonkingred) Act1_King = true;
	if (monster.SnoActor.Sno == ActorSnoEnum._uber_maghda) Act1_Witch = true;
		if (Act1_King && Act1_Witch) { TotalAct1 += 1; Act1_Clear = true; Act1_King = false; Act1_Witch = false; }

	// Act 2
	if (monster.SnoActor.Sno == ActorSnoEnum._uber_gluttony) Act2_Ghom = true;
	if (monster.SnoActor.Sno == ActorSnoEnum._uber_despair) Act2_Despair = true;
		if (Act2_Ghom && Act2_Despair) { TotalAct2 += 1; Act2_Clear = true; Act2_Ghom = false; Act2_Despair = false; }

	// Act 3
	if (monster.SnoActor.Sno == ActorSnoEnum._uber_siegebreakerdemon) Act3_Behemoth = true;
	if (monster.SnoActor.Sno == ActorSnoEnum._uber_zoltunkulle) Act3_Undying = true;
		if (Act3_Behemoth && Act3_Undying) { TotalAct3 += 1; Act3_Clear = true; Act3_Behemoth = false; Act3_Undying = false; }

	// Act 4
	if (monster.SnoActor.Sno == ActorSnoEnum._uber_siegebreakerdemon_diablo || monster.SnoActor.Sno == ActorSnoEnum._uber_skeletonkingred_diablo || monster.SnoActor.Sno == ActorSnoEnum._uber_gluttony_diablo) Act4_FirstSpawn = true;
	if (monster.SnoActor.Sno == ActorSnoEnum._uber_zoltunkulle_diablo || monster.SnoActor.Sno == ActorSnoEnum._uber_despair_diablo || monster.SnoActor.Sno == ActorSnoEnum._uber_maghda_diablo) Act4_SecondSpawn = true;
	if (monster.SnoActor.Sno == ActorSnoEnum._uber_terrordiablo) Act4_Diablo = true;
		if (Act4_Diablo && Act4_FirstSpawn && Act4_SecondSpawn) { TotalAct4 += 1; Act4_Clear = true; Act4_Diablo = false; Act4_FirstSpawn = false; Act4_SecondSpawn = false; }

		if (Act1_Clear && Act2_Clear && Act3_Clear && Act4_Clear) { TotalClears += 1; }

	// Add Uber killed into Uber History
	var uber = new UberSnapshot() { BossName = monster.SnoMonster.NameLocalized, Timestamp = Hud.Time.Now, Difficulty = Hud.Game.GameDifficulty.ToString("") };
		UberHistory.Add(uber);

	}

		
	private void DrawUberHistory(RectangleF rect) //x,y coordinates of the bottom left of the rift history window (above the other labels + space gap)
	{
		if (UberHistory == null || UberHistory.Count == 0) return;

		DateTime now = Hud.Time.Now;
		IEnumerable<UberSnapshot> history = (UberHistoryTimeout > 0 ?
			UberHistory.Where(d => (now - d.Timestamp).TotalSeconds < UberHistoryTimeout) :
			UberHistory
		).OrderByDescending(d => d.Timestamp).Take(UberHistoryShown);
		List<Tuple<TextLayout, TextLayout, TextLayout, IFont, IBrush>> lines = new List<Tuple<TextLayout, TextLayout, TextLayout, IFont, IBrush>>(UberHistoryShown + 1);
			
		foreach (UberSnapshot uber in history)
		{
			// How long ago was the drop
			TimeSpan elapsed = now - uber.Timestamp;				
			string time;
			if (elapsed.TotalSeconds < 60)
				time = elapsed.TotalSeconds.ToString("F0") + "s ago";
			else if (elapsed.TotalMinutes < 10)
				time = elapsed.TotalMinutes.ToString("F0") + "m ago";
			else
				time = uber.Timestamp.ToString("hh:mm tt");

			// IS GR ?
			string textrift;
			textrift = "killed in " + uber.Difficulty.ToUpper() + "";

			// Name
			IFont color = UberFont;

			// Store line data
			lines.Add(new Tuple<TextLayout, TextLayout, TextLayout, IFont, IBrush>(
				UberFont.GetTextLayout(time),
				BossFont.GetTextLayout(uber.BossName),
				color.GetTextLayout(textrift),
				color,
				(elapsed.TotalSeconds <= UberHistoryHighlight ? HighlightBrush : BgBrush)
			));
		}
			
		// Compute width of the loot history display
		float widthTime = lines.Select(t => t.Item1.Metrics.Width).Max();
		float widthBoss = lines.Select(t => t.Item2.Metrics.Width).Max();
		float widthInfos = lines.Select(t => t.Item3.Metrics.Width).Max();
		float width = Math.Max(rect.Width, 5 + widthTime + 10 + widthBoss + 10 + widthInfos + 5);			
			
		//x += width;
		float x = rect.X;
		if (x + width > Hud.Window.Size.Width)
			x = Hud.Window.Size.Width - width;
		float y = rect.Y;
			
		foreach (Tuple<TextLayout, TextLayout, TextLayout, IFont, IBrush> line in lines)
		{
			TextLayout layout = line.Item1;
			float height = Math.Max(layout.Metrics.Height, rect.Height);
			if (Hook == 0) y -= height;
				else y += height;
				
			//draw background
			line.Item5.DrawRectangle(x, y, width, height); 
				
			//draw timestamp
			TimeFont.DrawText(layout, x + 5 + widthTime - layout.Metrics.Width, y + height*0.5f - layout.Metrics.Height*0.5f); 
				
			//draw boss name
			layout = line.Item2;
			if (layout != null)
				BossFont.DrawText(line.Item2, x + 5 + widthTime + 5 + widthBoss * 0.5f - layout.Metrics.Width *0.5f, y + height*0.5f - layout.Metrics.Height*0.5f);
				
			//draw rift infos
			layout = line.Item3;
			line.Item4.DrawText(layout, x + 5 + widthTime + 5 + widthBoss + 5, y + height*0.5f - layout.Metrics.Height*0.5f);
		}
	}

    }
}