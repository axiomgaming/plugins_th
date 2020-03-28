// TopStats Monsters Tracker by HaKache

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
    public class LiveStats_MonsterTracker : BasePlugin, ICustomizer, IMonsterKilledHandler //, IInGameWorldPainter
    {	
	public TopLabelDecorator Label { get; set; }
	public TopLabelDecorator Label0 { get; set; }
	public List<TopLabelDecorator> ExpandUpLabels { get; private set; }

	public IFont TitleFont { get; set; }
	public IFont EliteFont { get; set; }
	public IFont BossFont { get; set; }
	public IFont MonsterFont { get; set; }
	public IFont GoblinFont { get; set; }
	public IFont RareFont { get; set; }
	public IFont ChampionFont { get; set; }

	public IBrush BgBrush { get; set; }
	public IBrush BgBrushAlt { get; set; }
	public IBrush BgBrushTitle { get; set; }
	public IBrush HighlightBrush { get; set; }

	public int TotalRares { get; private set; } = 0;
	public int TotalChampions { get; private set; } = 0;
	public int TotalBosses { get; private set; } = 0;
	public int TotalGoblins { get; private set; } = 0;

        public int Priority { get; set; } = 4;
        public int Hook { get; set; } = 1;
		
        public LiveStats_MonsterTracker()
	{
	    Enabled = true;
	}
		
	public void Customize()
	{
		// Add this display to the LiveStats readout with a specified positional order priority of 4
		Hud.RunOnPlugin<LiveStatsPlugin>(plugin => {
			if (this.Hook == 0) plugin.Add(this.Label0, this.Priority, this.Hook);
				else plugin.Add(this.Label, this.Priority, this.Hook);			
		});
	}

        public override void Load(IController hud)
        {
            base.Load(hud);

	    TitleFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 120, 0, true, false, true);
	    MonsterFont = Hud.Render.CreateFont("tahoma", 7, 255, 191, 100, 47, false, false, true); //191, 100, 47
	    EliteFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 120, 0, false, false, true);
	    BossFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 0, 0, false, false, true);
	    GoblinFont = Hud.Render.CreateFont("tahoma", 7, 190, 0, 255, 0, false, false, true);
	    RareFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 148, 20, false, false, true);
	    ChampionFont = Hud.Render.CreateFont("tahoma", 7, 190, 64, 128, 255, false, false, true);
			
	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
	    BgBrush = plugin.BgBrush;
	    BgBrushAlt = plugin.BgBrushAlt;
	    BgBrushTitle = Hud.Render.CreateBrush(175, 152, 80, 20, 0);
	    HighlightBrush = Hud.Render.CreateBrush(200, 72, 132, 84, 0);
			
            Label = new TopLabelDecorator(Hud)
            {
                TextFont = MonsterFont,
		TextFunc = () => { return Hud.Tracker.Session.MonsterKill.ToString("F0") + (Hud.Tracker.Session.MonsterKill > 1 ? " monsters" : " monster"); },
                HintFunc = () => "Monsters Killed",
                ExpandUpLabels = new List<TopLabelDecorator>() {
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = TitleFont,
                        BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont =  TitleFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => "Account",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = MonsterFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  MonsterFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => string.Format("{0} ({1}/h)", Hud.Tracker.CurrentAccountTotal.MonsterKill, Hud.Tracker.CurrentAccountTotal.MonsterKillPerHour.ToString("F0", CultureInfo.InvariantCulture)),
                        HintFunc = () => "Monsters Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = EliteFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  EliteFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => string.Format("{0} ({1}/h)", Hud.Tracker.CurrentAccountTotal.EliteKill, Hud.Tracker.CurrentAccountTotal.EliteKillPerHour.ToString("F0", CultureInfo.InvariantCulture)),
                        HintFunc = () => "Elites Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = TitleFont,
                        BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont =  TitleFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => "Session",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = MonsterFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  MonsterFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => string.Format("{0} ({1}/h)", Hud.Tracker.Session.MonsterKill, Hud.Tracker.Session.MonsterKillPerHour.ToString("F0", CultureInfo.InvariantCulture)),
                        HintFunc = () => "Monsters Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = EliteFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  EliteFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => string.Format("{0} ({1}/h)", Hud.Tracker.Session.EliteKill, Hud.Tracker.Session.EliteKillPerHour.ToString("F0", CultureInfo.InvariantCulture)),
                        HintFunc = () => "Elites Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  ChampionFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  ChampionFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => TotalChampions.ToString() + " (" + (TotalChampions / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F0", CultureInfo.InvariantCulture) + "/h)",
                        HintFunc = () => "Champions Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  RareFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  RareFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => TotalRares.ToString() + " (" + (TotalRares / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F0", CultureInfo.InvariantCulture) + "/h)",
                        HintFunc = () => "Rares Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  GoblinFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  GoblinFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => TotalGoblins.ToString() + " (" + (TotalGoblins / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F0", CultureInfo.InvariantCulture) + "/h)",
                        HintFunc = () => "Goblins Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  BossFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  BossFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => TotalBosses.ToString() + " (" + (TotalBosses / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F0", CultureInfo.InvariantCulture) + "/h)",
                        HintFunc = () => "Bosses Killed",
                    },
		},
	    };

            Label0 = new TopLabelDecorator(Hud)
            {
                TextFont = MonsterFont,
		TextFunc = () => { return Hud.Tracker.Session.MonsterKill.ToString("F0") + (Hud.Tracker.Session.MonsterKill > 1 ? " monsters" : " monster"); },
                HintFunc = () => "Monsters Killed",
                ExpandUpLabels = new List<TopLabelDecorator>() {
                    new TopLabelDecorator(Hud) {
                        TextFont =  BossFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  BossFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => TotalBosses.ToString() + " (" + (TotalBosses / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F0", CultureInfo.InvariantCulture) + "/h)",
                        HintFunc = () => "Bosses Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  GoblinFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  GoblinFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => TotalGoblins.ToString() + " (" + (TotalGoblins / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F0", CultureInfo.InvariantCulture) + "/h)",
                        HintFunc = () => "Goblins Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  RareFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  RareFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => TotalRares.ToString() + " (" + (TotalRares / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F0", CultureInfo.InvariantCulture) + "/h)",
                        HintFunc = () => "Rares Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  ChampionFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  ChampionFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => TotalChampions.ToString() + " (" + (TotalChampions / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F0", CultureInfo.InvariantCulture) + "/h)",
                        HintFunc = () => "Champions Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = EliteFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  EliteFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => string.Format("{0} ({1}/h)", Hud.Tracker.Session.EliteKill, Hud.Tracker.Session.EliteKillPerHour.ToString("F0", CultureInfo.InvariantCulture)),
                        HintFunc = () => "Elites Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = MonsterFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  MonsterFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => string.Format("{0} ({1}/h)", Hud.Tracker.Session.MonsterKill, Hud.Tracker.Session.MonsterKillPerHour.ToString("F0", CultureInfo.InvariantCulture)),
                        HintFunc = () => "Monsters Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = TitleFont,
                        BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont =  TitleFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => "Session",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = EliteFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  EliteFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => string.Format("{0} ({1}/h)", Hud.Tracker.CurrentAccountTotal.EliteKill, Hud.Tracker.CurrentAccountTotal.EliteKillPerHour.ToString("F0", CultureInfo.InvariantCulture)),
                        HintFunc = () => "Elites Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = MonsterFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  MonsterFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => string.Format("{0} ({1}/h)", Hud.Tracker.CurrentAccountTotal.MonsterKill, Hud.Tracker.CurrentAccountTotal.MonsterKillPerHour.ToString("F0", CultureInfo.InvariantCulture)),
                        HintFunc = () => "Monsters Killed",
                    },
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = TitleFont,
                        BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont =  TitleFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => "Account",
                        HintFunc = () => " ",
                    },
		},
	    };
			
		if (Hook == 0) ExpandUpLabels = Label0.ExpandUpLabels;
			else ExpandUpLabels = Label.ExpandUpLabels;
	}

	public void OnMonsterKilled(IMonster monster)
	{
	
	    if (monster.Rarity == ActorRarity.Champion && monster.SummonerAcdDynamicId == 0) TotalChampions += 1;
	    if (monster.Rarity == ActorRarity.Rare && monster.SummonerAcdDynamicId == 0) TotalRares += 1;
	    if (monster.SnoMonster.Priority == MonsterPriority.goblin) TotalGoblins += 1;
	    if (monster.Rarity == ActorRarity.Boss && monster.SummonerAcdDynamicId == 0) TotalBosses += 1;

	}

    }
}