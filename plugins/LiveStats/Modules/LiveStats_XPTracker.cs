// TopStats XP Tracker by HaKache
// The Pool Tracker Function is an adaptation of RNN PoolsBanditsList Plugin.

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
    public class LiveStats_XPTracker : BasePlugin, ICustomizer, IAfterCollectHandler, INewAreaHandler, IInGameWorldPainter //, IInGameTopPainter //, IKeyEventHandler
    {
	private Dictionary<int, List<string>> ActAreaList { get; set; }
	private Dictionary<string, List<string>> AreaPoolList { get; set; }
	private Dictionary<string, int> PoolOperated { get; set; }
	private Dictionary<string, int> PoolReappears { get; set; }
	private Dictionary<uint, int> ActFix { get; set; }
	private Dictionary<string, string> AreaFix { get; set; }
	private Dictionary<string, string> AreaNameFix { get; set; }

	public IFont ShrineFont { get; set; }
	public IFont ShrineOpFont { get; set; }
	public IFont PoolFont { get; set; }
	public IFont PoolOpFont { get; set; }

	public List<PoolSnapshot> PoolTracker { get; private set; }
        public WorldDecoratorCollection MapMarkerPool { get; set; } // Marker style for the minimap
        public WorldDecoratorCollection MapMarkerShrine { get; set; } // Marker style for the minimap

	public class PoolSnapshot {
		public string Id { get; set; }
		public uint WorldId { get; set; }
		public IWorldCoordinate FloorCoordinate { get; set; }
		public string Name { get; set; }
		public ShrineType Type { get; set; }

		public PoolSnapshot(IShrine shrine)
		{
		    Name = shrine.SnoActor.NameLocalized;
		    FloorCoordinate = shrine.FloorCoordinate;
		    WorldId = shrine.WorldId;
		    Type = shrine.Type;
		    Id = Type.ToString() + FloorCoordinate.ToString() + WorldId.ToString(); // Creating some kind of unique ID
		}
		    
	}

	public TopLabelDecorator Label { get; set; }
	public TopLabelDecorator Label0 { get; set; }
	public List<TopLabelDecorator> ExpandUpLabels { get; private set; }

	public IFont TextFont { get; set; }
	public IFont TitleFont { get; set; }
	public IBrush BgBrush { get; set; }
	public IBrush BgBrushAlt { get; set; }
	public IBrush BgBrushTitle { get; set; }

	public uint SessionStartParagon { get; private set; } = 0;
        public int MyIndex { get; set; } = -1;

	public bool EnablePoolTracker { get; set; } = true; // Enable or disable the Pool Tracker function
	public bool MapMarker { get; set; } = true; // Enable or disable the Map Marker function

        public int Priority { get; set; } = 1;
        public int Hook { get; set; } = 1;
		
        public LiveStats_XPTracker()
	{
	    Enabled = true;
	}
		
	public void Customize()
	{
		// Add this display to the LiveStats readout with a specified positional order priority of 1
		Hud.RunOnPlugin<LiveStatsPlugin>(plugin => {
			if (this.Hook == 0) plugin.Add(this.Label0, this.Priority, this.Hook);
				else plugin.Add(this.Label, this.Priority, this.Hook);			
		});
	}

        public override void Load(IController hud)
        {
            base.Load(hud);

	    PoolTracker = new List<PoolSnapshot>();

	    ActAreaList = new Dictionary<int, List<string>>();
	    AreaPoolList = new Dictionary<string, List<string>>();
	    PoolOperated = new Dictionary<string, int>();
	    PoolReappears = new Dictionary<string, int>();

	    ActFix = new Dictionary<uint, int>();
	    AreaFix = new Dictionary<string, string>();
	    AreaNameFix = new Dictionary<string, string>();

	    for (int i = 0; i < 6; i++) { ActAreaList.Add(i, new List<string>() ); }
			
	    AreaFix.Add("855.431, 1411.451, 197.8", "Alcarnus"); AreaFix.Add("951.572, 1302.670, 197.3", "Alcarnus"); AreaFix.Add("2170.000, 1775.000, 0.0", "Cemetery of the Forsaken");

	    AreaNameFix.Add("Fractured Fate: The Festering Woods", "Fractured Fate: Festering Woods"); AreaNameFix.Add("Fractured Fate: Battlefields of Eternity", "Fractured Fate: Battlefields");
	    AreaNameFix.Add("Upper Infernal Fate: Halls of Agony", "Upper Fate: Halls of Agony"); AreaNameFix.Add("Upper Infernal Fate: Arreat Crater", "Upper Fate: Arreat Crater");
	    AreaNameFix.Add("Lower Cursed Fate: Briarthorn Cemetery", "Lower Cursed Fate: Cemetery"); AreaNameFix.Add("Lower Cursed Fate: The Silver Spire", "Lower Cursed Fate: Silver Spire");
	    AreaNameFix.Add("Sundered Canyon", "Howling Plateau"); AreaNameFix.Add("Pandemonium Fortress Level 1", "Pandemonium Level 1"); AreaNameFix.Add("Pandemonium Fortress Level 2", "Pandemonium Level 2");
	    AreaNameFix.Add("Cave of the Moon Clan Level 1", "Moon Clan Cave Level 1"); AreaNameFix.Add("Cave of the Moon Clan Level 2", "Moon Clan Cave Level 2");

	    // (SnoArea.Sno,act) for new areas not tied to an Act.
	    ActFix.Add(63666, 1); ActFix.Add(445426, 1);
	    ActFix.Add(456638, 2); ActFix.Add(460671, 2);  ActFix.Add(464092, 2); ActFix.Add(464830, 2); ActFix.Add(465885, 2); ActFix.Add(467383, 2);
	    ActFix.Add(444307, 3); ActFix.Add(445762, 3);

	    ActFix.Add(444396, 4); ActFix.Add(445792, 4); ActFix.Add(446367, 4); ActFix.Add(446550, 4);  ActFix.Add(448011, 4);  ActFix.Add(448039, 4); 
	    ActFix.Add(464063, 4); ActFix.Add(464065, 4); ActFix.Add(464066, 4);                                   
	    ActFix.Add(464810, 4); ActFix.Add(464820, 4); ActFix.Add(464821, 4); ActFix.Add(464822, 4); ActFix.Add(464857, 4); ActFix.Add(464858, 4);                   
	    ActFix.Add(464865, 4); ActFix.Add(464867, 4); ActFix.Add(464868, 4); ActFix.Add(464870, 4); ActFix.Add(464871, 4); ActFix.Add(464873, 4);
	    ActFix.Add(464874, 4); ActFix.Add(464875, 4); ActFix.Add(464882, 4); ActFix.Add(464886, 4); ActFix.Add(464889, 4); ActFix.Add(464890, 4);                        
	    ActFix.Add(464940, 4); ActFix.Add(464941, 4); ActFix.Add(464942, 4); ActFix.Add(464943, 4); ActFix.Add(464944, 4);
	    ActFix.Add(475854, 4); ActFix.Add(475856, 4); 

	    ActFix.Add(448391, 5); ActFix.Add(448368, 5); ActFix.Add(448375, 5); ActFix.Add(448398, 5); ActFix.Add(448404,5); ActFix.Add(448411,5);
	
	    ShrineFont = Hud.Render.CreateFont("tahoma", 7, 250, 120, 210, 70, true, false, true);
	    ShrineOpFont = Hud.Render.CreateFont("tahoma", 7, 160, 120, 210, 70, true, false, true);
	    PoolFont = Hud.Render.CreateFont("tahoma", 7, 250, 25, 225, 255, true, false, true); // ("tahoma", 7, 250, 25, 225, 255, true, false, 128, 0, 0, 0, true);
	    PoolOpFont = Hud.Render.CreateFont("tahoma", 7, 160, 180, 147, 109, true, false, true);

	    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 25, 225, 255, false, false, true);
	    TitleFont = Hud.Render.CreateFont("tahoma", 7, 255, 25, 225, 255, true, false, true);
			
	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
	    BgBrush = plugin.BgBrush; 
	    BgBrushAlt = plugin.BgBrushAlt;
	    BgBrushTitle = Hud.Render.CreateBrush(175, 50, 84, 132, 0);
			
	    Label = new TopLabelDecorator(Hud) // Menu for TopStats
            {
                TextFont = TextFont,
                TextFunc = () => (Hud.Game.Me.CurrentLevelNormal >= Hud.Game.Me.CurrentLevelNormalCap && Hud.Game.Me.ParagonExpToNextLevel > 0 ?
			(((decimal)Hud.Game.Me.BonusPoolRemaining / (decimal)Hud.Game.Me.ParagonExpToNextLevel)*10) <= 1 ?
			(((decimal)Hud.Game.Me.BonusPoolRemaining / (decimal)Hud.Game.Me.ParagonExpToNextLevel)*10).ToString("F2", CultureInfo.InvariantCulture) + " pool" :
			(((decimal)Hud.Game.Me.BonusPoolRemaining / (decimal)Hud.Game.Me.ParagonExpToNextLevel)*10).ToString("F2", CultureInfo.InvariantCulture) + " pools" :
			"N/A" // Haven't figured out how to access the pre paragon xp tables yet
		),
                HintFunc = () => "Bonus Pools",
		ExpandUpLabels = new List<TopLabelDecorator>()
                {
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Account",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.CurrentAccountTotal.GainedExperience, ValueFormat.ShortNumber) + " xp",
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountTotal.GainedExperiencePerHourFull, ValueFormat.ShortNumber) + " xp/h",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Yesterday",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = TextFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.CurrentAccountYesterday.GainedExperience, ValueFormat.ShortNumber) + " xp",
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountYesterday.GainedExperiencePerHourFull, ValueFormat.ShortNumber) + " xp/h",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Today",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.CurrentAccountToday.GainedExperience, ValueFormat.ShortNumber) + " xp",
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountToday.GainedExperiencePerHourFull, ValueFormat.ShortNumber) + " xp/h",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Session",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = TextFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.Session.GainedExperience, ValueFormat.ShortNumber) + " xp",
                        HintFunc = () => ValueToString(Hud.Tracker.Session.GainedExperiencePerHourPlay, ValueFormat.ShortNumber) + " xp/h",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Session ABS",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.SessionAlwaysRunning.GainedExperience, ValueFormat.ShortNumber) + " xp",
                        HintFunc = () => ValueToString(Hud.Tracker.SessionAlwaysRunning.GainedExperiencePerHourFull, ValueFormat.ShortNumber) + " xp/h",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = TextFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ((Hud.Game.Me.CurrentLevelNormal < Hud.Game.Me.CurrentLevelNormalCap) ? Hud.Game.Me.CurrentLevelNormal.ToString("0") : (Hud.Game.Me.CurrentLevelParagon - SessionStartParagon).ToString()),
                        HintFunc = () => ((Hud.Game.Me.CurrentLevelNormal < Hud.Game.Me.CurrentLevelNormalCap) ? "Current Level" : "Paragon Earned"),
                    },
		    new TopLabelDecorator(Hud) {
			TextFont = TextFont,
			TextFunc = () => {
			float w = plugin.WidthFunc();
			float h = plugin.HeightFunc();
			float x = Hook == 0 ? (plugin.PinnedLabel == Label ? plugin.ExpandLabelHook : plugin.SelectedRectangle.X) : (plugin.PinnedTopLabel == Label ? plugin.ExpandLabelTopHook : plugin.SelectedTopRectangle.X);
			float y = Hook == 0 ? (plugin.SelectedRectangle.Y - h * (ExpandUpLabels.Count - 0.5f)) : (plugin.SelectedTopRectangle.Y + h * (ExpandUpLabels.Count - 0.5f));
					
			DrawPoolTracker(new RectangleF(x, y, w, h));
							
			return " ";
			},
		    },
		},
	    };

	    Label0 = new TopLabelDecorator(Hud) // Menu for RunStats
            {
                TextFont = TextFont,
                TextFunc = () => (Hud.Game.Me.CurrentLevelNormal >= Hud.Game.Me.CurrentLevelNormalCap && Hud.Game.Me.ParagonExpToNextLevel > 0 ?
			(((decimal)Hud.Game.Me.BonusPoolRemaining / (decimal)Hud.Game.Me.ParagonExpToNextLevel)*10) <= 1 ?
			(((decimal)Hud.Game.Me.BonusPoolRemaining / (decimal)Hud.Game.Me.ParagonExpToNextLevel)*10).ToString("F2", CultureInfo.InvariantCulture) + " pool" :
			(((decimal)Hud.Game.Me.BonusPoolRemaining / (decimal)Hud.Game.Me.ParagonExpToNextLevel)*10).ToString("F2", CultureInfo.InvariantCulture) + " pools" :
			"N/A" // Haven't figured out how to access the pre paragon xp tables yet
		),
                HintFunc = () => "Bonus Pools",
		ExpandUpLabels = new List<TopLabelDecorator>()
                {
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ((Hud.Game.Me.CurrentLevelNormal < Hud.Game.Me.CurrentLevelNormalCap) ? Hud.Game.Me.CurrentLevelNormal.ToString("0") : (Hud.Game.Me.CurrentLevelParagon - SessionStartParagon).ToString()),
                        HintFunc = () => ((Hud.Game.Me.CurrentLevelNormal < Hud.Game.Me.CurrentLevelNormalCap) ? "Current Level" : "Paragon Earned"),
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = TextFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.SessionAlwaysRunning.GainedExperience, ValueFormat.ShortNumber) + " xp",
                        HintFunc = () => ValueToString(Hud.Tracker.SessionAlwaysRunning.GainedExperiencePerHourFull, ValueFormat.ShortNumber) + " xp/h",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Session ABS",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.Session.GainedExperience, ValueFormat.ShortNumber) + " xp",
                        HintFunc = () => ValueToString(Hud.Tracker.Session.GainedExperiencePerHourPlay, ValueFormat.ShortNumber) + " xp/h",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Session",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = TextFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.CurrentAccountToday.GainedExperience, ValueFormat.ShortNumber) + " xp",
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountToday.GainedExperiencePerHourFull, ValueFormat.ShortNumber) + " xp/h",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Today",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.CurrentAccountYesterday.GainedExperience, ValueFormat.ShortNumber) + " xp",
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountYesterday.GainedExperiencePerHourFull, ValueFormat.ShortNumber) + " xp/h",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Yesterday",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = TextFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => ValueToString(Hud.Tracker.CurrentAccountTotal.GainedExperience, ValueFormat.ShortNumber) + " xp",
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountTotal.GainedExperiencePerHourFull, ValueFormat.ShortNumber) + " xp/h",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Account",
                        HintFunc = () => " ",
                    },
		    new TopLabelDecorator(Hud) {
			TextFont = TextFont,
			TextFunc = () => {
			float w = plugin.WidthFunc();
			float h = plugin.HeightFunc();
			float x = Hook == 0 ? (plugin.PinnedLabel == Label0 ? plugin.ExpandLabelHook : plugin.SelectedRectangle.X) : (plugin.PinnedTopLabel == Label0 ? plugin.ExpandLabelTopHook : plugin.SelectedTopRectangle.X);
			float y = Hook == 0 ? (plugin.SelectedRectangle.Y - h * (ExpandUpLabels.Count - 0.5f)) : (plugin.SelectedTopRectangle.Y + h * (ExpandUpLabels.Count - 0.5f));
					
			DrawPoolTracker(new RectangleF(x, y, w, h));
							
			return " ";
			},
		    },
		},
	    };
			
		if (Hook == 0) ExpandUpLabels = Label0.ExpandUpLabels;
			else ExpandUpLabels = Label.ExpandUpLabels;

            MapMarkerPool = new WorldDecoratorCollection(
                new MapShapeDecorator(Hud)
                {
                    ShapePainter = new CircleShapePainter(Hud),
                    Brush = Hud.Render.CreateBrush(225, 25, 225, 255, 3),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 4f,
                    RadiusTransformator = new StandardPingRadiusTransformator(Hud, 333),
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 25, 155, 175, true, false, 128, 0, 0, 0, true),
                    //Up = true,
                    //RadiusOffset = 6.0f,
                }
            );	
            MapMarkerShrine = new WorldDecoratorCollection(
                new MapShapeDecorator(Hud)
                {
                    ShapePainter = new RectangleShapePainter(Hud),
                    Brush = Hud.Render.CreateBrush(225, 120, 210, 70, 3),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 4f,
                    RadiusTransformator = new StandardPingRadiusTransformator(Hud, 333),
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 120, 210, 70, true, false, 128, 0, 0, 0, true),
                    Up = true,
                    //RadiusOffset = 6.0f,
                }
            );	
	}

	public void OnNewArea(bool newGame, ISnoArea area)
	{
	    if (newGame || (MyIndex != Hud.Game.Me.Index)) // We clear all dictionaries and snapshots upon entering a new game or if our index change
	    {
		MyIndex = Hud.Game.Me.Index;
		for (int i = 0; i < 6; i++)  ActAreaList[i].Clear(); 
		foreach (string keyarea in AreaPoolList.Keys)  AreaPoolList[keyarea].Clear(); 			      	
		AreaPoolList.Clear();                                
		PoolOperated.Clear();
		PoolReappears.Clear();

		PoolTracker.Clear();
	    }

	    if (Hud.Game.Me.CurrentLevelNormal < Hud.Game.Me.CurrentLevelNormalCap && Hud.Game.Me.ParagonExpToNextLevel <= 0) return;
	    if (SessionStartParagon == 0) SessionStartParagon = Hud.Game.Me.CurrentLevelParagon;
	}

        public void PaintWorld(WorldLayer layer)
        {
	// Draw markers on the world map if MapMarker option is enabled
	if (!MapMarker) return;

            foreach (PoolSnapshot pool in PoolTracker.Where(s => s.WorldId == Hud.Game.Me.WorldId && s.Type == ShrineType.PoolOfReflection && s.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) > 150))
                MapMarkerPool.Paint(layer, null, pool.FloorCoordinate, pool.Name);

            foreach (PoolSnapshot bandit in PoolTracker.Where(s => s.WorldId == Hud.Game.Me.WorldId && s.Type == ShrineType.BanditShrine && s.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) > 150))
                MapMarkerShrine.Paint(layer, null, bandit.FloorCoordinate, bandit.Name);
        }

	public void AfterCollect()
	{

	if (Hud.Game.Me.SnoArea == null || Hud.Game.Me.SnoArea.Code == null || Hud.Game.Me.SnoArea.Code.Contains("x1_lr_level")) return; // Safecheck

	var shrines = Hud.Game.Shrines.Where(s => (s.Type == ShrineType.PoolOfReflection || s.Type == ShrineType.BanditShrine) && s.FloorCoordinate.IsValid); 
	    foreach (var shrine in shrines)  {

		// Add Bandit & Pools to the PoolTracker to mark them on the map
		if ((!shrine.IsDisabled || !shrine.IsOperated) && !PoolTracker.Any(s => s.Id.Equals(shrine.Type.ToString() + shrine.FloorCoordinate.ToString() + shrine.WorldId.ToString()))) // IsSameShrine (Hopefully)
		{
		    var pool = new PoolSnapshot(shrine);
		    PoolTracker.Add(pool);
		}
		// Remove Bandit & Pools that have been clicked from the PoolTracker
		if ((shrine.IsDisabled || shrine.IsOperated) && PoolTracker.Any(s => s.Id.Equals(shrine.Type.ToString() + shrine.FloorCoordinate.ToString() + shrine.WorldId.ToString())))
		{
		    PoolTracker.RemoveAll(s => s.Id.Equals(shrine.Type.ToString() + shrine.FloorCoordinate.ToString() + shrine.WorldId.ToString()));
		}

		if (!EnablePoolTracker) return; // Disable the PoolTracker List

		// Add Bandit & Pools to the dictionaries to track them in a list
		if ((shrine.Scene.SnoArea == null) || (shrine.Scene.SnoArea.NameLocalized == null)) continue;
		string coord = shrine.FloorCoordinate.ToString();
		int active = ((shrine.Type == ShrineType.PoolOfReflection)? 0 : 2) + ((shrine.IsDisabled || shrine.IsOperated)? 0 : 1);
		if (PoolOperated.Keys.Contains(coord)) {
			if ((active % 2 == 0) && (PoolOperated[coord] % 2 == 1))  PoolOperated[coord] = active; // Dealing with weird bug : Sometimes empty pools are considered not operated
			if ((PoolReappears[coord]) < 100)  PoolReappears[coord] = PoolReappears[coord] + 1; // And goes empty again instantly after. 
		}
		else
		{	
			string areaname = shrine.Scene.SnoArea.NameLocalized;
			// Fixing some specific pools area or specific area names
			string area = (AreaFix.Keys.Contains(coord)) ? AreaFix[coord] : (AreaNameFix.Keys.Contains(areaname)) ? AreaNameFix[areaname] : areaname;

			PoolOperated.Add(coord, active); PoolReappears.Add(coord,0);
			if (AreaPoolList.Keys.Contains(area)) { AreaPoolList[area].Add(coord); }
			else
			{ 
			    int act = 0;
			    if ((shrine.Scene.SnoArea.Act > 0) && (shrine.Scene.SnoArea.Act < 6)) act = shrine.Scene.SnoArea.Act; 
			    else if (ActFix.Keys.Contains(shrine.Scene.SnoArea.Sno)) act = ActFix[shrine.Scene.SnoArea.Sno];
			    AreaPoolList.Add(area, new List<string>()); AreaPoolList[area].Add(coord);

			    if (ActAreaList.Keys.Contains(act)) { ActAreaList[act].Add(area); }
				else { ActAreaList.Add(act, new List<string>()); ActAreaList[act].Add(area); }
			}
		} 	
	    } 
	}

	private void DrawPoolTracker(RectangleF rect) // x,y coordinates of the top left of the pool tracker window (below the other labels + space gap)
	{
	    int PoolNum = 0; int BanditNum = 0;

	    int h = 1;
	    int alternate = 0; // Initialize the alternate brush function in the Pool List

		for (int act = 0; act < 6; act++) {     
		    foreach (string area in ActAreaList[act]) {

			int ActivePools = 0; int TotalPools = 0; int ActiveBandits = 0; int TotalBandits = 0;

			    foreach (string pool in AreaPoolList[area]) {
				if (PoolReappears[pool] > 0) {  
					if (PoolOperated[pool] == 0) { TotalPools++; }
					else if (PoolOperated[pool] == 1) { ActivePools++; TotalPools++; }
					else if (PoolOperated[pool] == 2) { TotalBandits++; }
					else if (PoolOperated[pool] == 3) { ActiveBandits++; TotalBandits++; }
				}
			    }

		    float width = Hud.Window.Size.Height * 0.2325f;
		    float x = rect.X;
		    if (x + width > Hud.Window.Size.Width)
			x = Hud.Window.Size.Width - width;
		    float y = rect.Y;

		    var AlternateBrush = (alternate++ % 2 == 0 ? BgBrush : BgBrush); // Alternate Brush disabled (At least for now)

		    if (TotalPools > 0 && ActivePools > 0)
		    {
			TextLayout layout = PoolFont.GetTextLayout(" " + ((act == 0) ? "A?" : "A" + act) + " - " + area + ((TotalPools > 1) ? " (" + ((ActivePools > 0 && ActivePools != TotalPools) ? ActivePools + "/" : "") + TotalPools + ")" : ""));

			float height = Math.Max(layout.Metrics.Height, rect.Height);
			if (Hook == 0) y -= height * h;
				else y += height * h;

			AlternateBrush.DrawRectangle(x, y, width, height);
			PoolFont.DrawText(layout, x + 2, y + height*0.5f - layout.Metrics.Height*0.5f); 
			h++;

			PoolNum = PoolNum + TotalPools;
		    }
		    if (TotalBandits > 0 && ActiveBandits > 0)
		    {
			TextLayout layout = ShrineFont.GetTextLayout(" " + ((act == 0) ? "A?" : "A" + act) + " - " + area + ((TotalBandits > 1) ? " (" + ((ActiveBandits > 0 && ActiveBandits != TotalBandits) ? ActiveBandits + "/" : "") + TotalBandits + ")" : ""));

			float height = Math.Max(layout.Metrics.Height, rect.Height);
			if (Hook == 0) y -= height * h;
				else y += height * h;

			AlternateBrush.DrawRectangle(x, y, width, height);
			ShrineFont.DrawText(layout, x + 2, y + height*0.5f - layout.Metrics.Height*0.5f); 
			h++;

			BanditNum = BanditNum + TotalBandits;
		    } 
			
		}
	    }
	}

    }
}