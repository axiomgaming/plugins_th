// RunStats Loot Helper by Razor
// Added Gambled Items per Hour & Map Markers now only appear when you're not in detection range of the item (By HaKache)

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
    public class LiveStats_LootHelper : BasePlugin, ILootGeneratedHandler, IItemPickedHandler, ICustomizer, INewAreaHandler, IInGameWorldPainter //, IInGameTopPainter
    {
	public Func<IItem, bool> FilterLoot { get; set; }	// What kind of items should show up on the loot history list?
	public Func<IItem, bool> FilterMap { get; set; }	// What kind of items should show up marked (with name) on the minimap?
 	public int LootHistoryTimeout { get; set; } = 0;	// Stop showing old drops after this many seconds (0 = never time out)
	public int LootHistoryHighlight { get; set; } = 20;	// Highlight if the drop is new for this many seconds
	public int LootHistoryShown { get; set; } = 12;		// Max # of items to show for drop history

	public int MapHistoryShown { get; set; } = 100;		// Max # of items to remember for map marking
	public float MapMarkerTreshold { get; set; } = 130;	// Range at which the Map Marker will be enabled (130 yards is approx. 2 screens, HUD detection range)
        public WorldDecoratorCollection MapMarkerLegendary { get; set; } 	// Marker style for the minimap
        public WorldDecoratorCollection MapMarkerSet { get; set; } 		// Marker style for the minimap
	
	public int DropsGenerated { get; private set; } = 0;
	public int DropsGambled { get; private set; } = 0;
	public List<DropSnapshot> LootHistory { get; private set; }
	public List<DropSnapshot> MapHistory { get; private set; }
		
	public TopLabelDecorator Label { get; set; }
	public List<TopLabelDecorator> ExpandUpLabels { get; private set; }
	
	public IFont LegendaryFont { get; set; }
	public IFont AncientFont { get; set; }
	public IFont PrimalFont { get; set; }
	public IFont SetFont { get; set; }
	public IFont GambleFont { get; set; }
	public IFont DefaultFont { get; set; }

	public IFont TimeFont { get; set; }
	public IBrush BgBrush { get; set; }
	public IBrush BgBrushAlt { get; set; }
	public IBrush HighlightBrush { get; set; }
		
	public class DropSnapshot {
		public string Id { get; set; }
		public DateTime Timestamp { get; set; }
		public uint WorldId { get; set; }
		public IWorldCoordinate FloorCoordinate { get; set; }
		public string ItemName { get; set; }
		public bool IsGambled { get; set; }
		public bool IsAncient { get; set; }
		public bool IsPrimal { get; set; }
		public bool IsSet { get; set; }
			
		public DropSnapshot(IItem item)
		{
		    if (item.Location == ItemLocation.Floor)
		    {
			WorldId = item.WorldId;
			FloorCoordinate = item.FloorCoordinate;
		    }
		    ItemName = item.SnoItem.NameLocalized;
		    Id = ItemName + item.Seed.ToString(); //item.ItemUniqueId;
		    IsAncient = (item.AncientRank > 0);
		    IsPrimal = (item.AncientRank > 1);
		    IsSet = (item.SetSno != uint.MaxValue);
		    }
		}

	public bool MapMarker { get; set; } // Enable or disable the Map Marker function

        public int Priority { get; set; } = 1;
        public int Hook { get; set; } = 0;
		
        public LiveStats_LootHelper()
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

	    MapMarker = true; 

	    FilterLoot = (item) => item.Quality == ItemQuality.Legendary; // && item.AncientRank > 0;
	    FilterMap = (item) => item.Quality == ItemQuality.Legendary && item.AncientRank > 0;
			
	    LootHistory = MapHistoryShown > 0 ? new List<DropSnapshot>(MapHistoryShown+1) : new List<DropSnapshot>();
	    MapHistory = MapHistoryShown > 0 ? new List<DropSnapshot>(MapHistoryShown+1) : new List<DropSnapshot>();
			
	    LegendaryFont = Hud.Render.CreateFont("tahoma", 7, 255, 191, 100, 47, false, false, true); //191, 100, 47
	    AncientFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 120, 0, false, false, true);
	    PrimalFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 0, 0, false, false, true);
	    SetFont = Hud.Render.CreateFont("tahoma", 7, 190, 0, 255, 0, false, false, true);
	    GambleFont = Hud.Render.CreateFont("tahoma", 7, 190, 27, 229, 199, false, false, true);
	    TimeFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 255, 255, false, false, true);
	    DefaultFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 140, 0, false, false, true);
			
	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
	    BgBrush = plugin.BgBrush;
	    BgBrushAlt = plugin.BgBrushAlt;
	    HighlightBrush = Hud.Render.CreateBrush(200, 72, 132, 84, 0);
			
	    Label = new TopLabelDecorator(Hud)
	    {
		TextFont = DefaultFont,
		TextFunc = () => {
			double lootsack = (DropsGenerated + DropsGambled);
			return lootsack.ToString() + (lootsack > 1 ? " loots" : " loot"); 
				},
		//ValueToString(Hud.Tracker.Session.DropAncient, ValueFormat.ShortNumber) + " {{A}}", //ValueToString(Hud.Game.Me.Defense.EhpCur, ValueFormat.ShortNumber),
		HintFunc = () => "# Items matching the drop filter dropped or gambled this session",
		ExpandUpLabels = new List<TopLabelDecorator>() {
		    new TopLabelDecorator(Hud) {
			//Enabled = false,
			TextFont = LegendaryFont,
			BackgroundBrush = BgBrush,
			ExpandedHintFont = LegendaryFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => Hud.Tracker.Session.DropLegendary.ToString() + " (" + (Hud.Tracker.Session.DropLegendary / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F1", CultureInfo.InvariantCulture) + "/h)",
			//TextFunc = () => string.Format("{0} ({1}/h)", Hud.Tracker.Session.DropLegendary, Hud.Tracker.Session.DropLegendaryPerHour.ToString("F1", CultureInfo.InvariantCulture)),
			HintFunc = () => "Legendary Drops",
		    },
		    new TopLabelDecorator(Hud) {
			TextFont = AncientFont,
			BackgroundBrush = BgBrushAlt,
			ExpandedHintFont = AncientFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => Hud.Tracker.Session.DropAncient.ToString() + " (" + (Hud.Tracker.Session.DropAncient / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F1", CultureInfo.InvariantCulture) + "/h)",
			//TextFunc = () => string.Format("{0} ({1}/h)", Hud.Tracker.Session.DropAncient, Hud.Tracker.Session.DropAncientPerHour.ToString("F1", CultureInfo.InvariantCulture)),
			HintFunc = () => "Ancient Drops",
		    },
		    new TopLabelDecorator(Hud) {
			TextFont = PrimalFont,
			BackgroundBrush = BgBrush,
			ExpandedHintFont = PrimalFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => Hud.Tracker.Session.DropPrimalAncient.ToString() + " (" + (Hud.Tracker.Session.DropPrimalAncient / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F1", CultureInfo.InvariantCulture) + "/h)",
			//TextFunc = () => string.Format("{0} ({1}/h)", Hud.Tracker.Session.DropPrimalAncient, Hud.Tracker.Session.DropPrimalAncientPerHour.ToString("F1", CultureInfo.InvariantCulture)),
			HintFunc = () => "Primal Drops",
		    },
		    new TopLabelDecorator(Hud) {
			TextFont = GambleFont,
			BackgroundBrush = BgBrushAlt,
			ExpandedHintFont = GambleFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			TextFunc = () => this.DropsGambled.ToString() + " (" + (this.DropsGambled / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F1", CultureInfo.InvariantCulture) + "/h)",
			//TextFunc = () => this.DropsGambled.ToString(),
			HintFunc = () => "Gambled",
		    },
		    new TopLabelDecorator(Hud) {
			TextFont = DefaultFont,
			TextFunc = () => {
			float w = plugin.WidthFunc();
			float h = plugin.HeightFunc();
			float x = Hook == 0 ? (plugin.PinnedLabel == Label ? plugin.ExpandLabelHook : plugin.SelectedRectangle.X) : (plugin.PinnedTopLabel == Label ? plugin.ExpandLabelTopHook : plugin.SelectedTopRectangle.X);
			float y = Hook == 0 ? (plugin.SelectedRectangle.Y - h * (ExpandUpLabels.Count - 0.5f)) : (plugin.SelectedTopRectangle.Y + h * (ExpandUpLabels.Count - 0.5f));
					
			DrawLootHistory(new RectangleF(x, y, w, h));
							
			return " ";
			},
		    },
		},
	    };
			
		ExpandUpLabels = Label.ExpandUpLabels;

            MapMarkerLegendary = new WorldDecoratorCollection(
                new MapShapeDecorator(Hud)
                {
                    ShapePainter = new RotatingTriangleShapePainter(Hud),
                    Brush = Hud.Render.CreateBrush(225, 255, 120, 0, 3),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 11f,
                    RadiusTransformator = new StandardPingRadiusTransformator(Hud, 333),
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 120, 0, true, false, 128, 0, 0, 0, true),
                    RadiusOffset = 15f, // 16f
                    Up = true,
                }
            );
            
            MapMarkerSet = new WorldDecoratorCollection(
                new MapShapeDecorator(Hud)
                {
                    ShapePainter = new RotatingTriangleShapePainter(Hud),
                    Brush = Hud.Render.CreateBrush(225, 160, 255, 0, 3),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 11f,
                    RadiusTransformator = new StandardPingRadiusTransformator(Hud, 333),
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 160, 255, 0, true, false, 128, 0, 0, 0, true),
                    RadiusOffset = 15f, // 16f
                    Up = true,
                }
            );
	}
		
        public void PaintWorld(WorldLayer layer)
        {
	// Draw markers on the world map if MapMarker option is enabled
	if (!MapMarker) return;

            foreach (DropSnapshot drop in MapHistory.Where(d => d.WorldId == Hud.Game.Me.WorldId && d.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) > MapMarkerTreshold))
                (drop.IsSet ? MapMarkerSet : MapMarkerLegendary).Paint(layer, null, drop.FloorCoordinate, drop.ItemName);
        }

	public void OnNewArea(bool newGame, ISnoArea area)
	{
	    if (newGame)
	    {
		// LootHistory.Clear();
		// MapHistory.Clear();
	    }
	}
		
	public void OnLootGenerated(IItem item, bool gambled)
	{
		if (item == null) return;
			
		// OnLootGenerated is called every time the item comes into TH's field of vision
		// Make sure that it is considered a new item
	
		DropSnapshot drop = null;			
		if (FilterLoot(item) && !LootHistory.Any(d => d.Id.Equals(item.SnoItem.NameLocalized + item.Seed))) //IsSameItem(d, item)
		{
			drop = new DropSnapshot(item) { Timestamp = Hud.Time.Now, IsGambled = gambled };
			LootHistory.Add(drop);
			// LootHistory.Insert(0, drop);
				
			if (LootHistory.Count > MapHistoryShown) // Make the loot memory as big as the map memory
				LootHistory.RemoveAt(0); // LootHistory.Count - 1);

			if (gambled)
				DropsGambled += 1;
			else
				DropsGenerated += 1;
		}
			
		if (item.Location == ItemLocation.Floor && FilterMap(item) && !MapHistory.Any(d => d.Id.Equals(item.SnoItem.NameLocalized + item.Seed)))
		{
			if (drop == null)
				drop = new DropSnapshot(item) { Timestamp = Hud.Time.Now, IsGambled = gambled };
			
			MapHistory.Add(drop);
			
			if (MapHistory.Count > MapHistoryShown)
				MapHistory.RemoveAt(0);
		}
	}
		
	public void OnItemPicked(IItem item)
	{
	    MapHistory.RemoveAll(d => d.Id.Equals(item.SnoItem.NameLocalized + item.Seed));
	}
		
	public void OnItemLocationChanged(IItem item, ItemLocation from, ItemLocation to)
	{
		if (to == ItemLocation.Floor && FilterMap(item) && !MapHistory.Any(d => d.Id.Equals(item.SnoItem.NameLocalized + item.Seed)))
		{
			MapHistory.Add(new DropSnapshot(item) { Timestamp = Hud.Time.Now });
		
			if (MapHistory.Count > MapHistoryShown)
				MapHistory.RemoveAt(0);
		}
	}
		
	private void DrawLootHistory(RectangleF rect) // x,y coordinates of the bottom left of the drop history window (above the other labels + space gap)
	{
		if (LootHistory == null || LootHistory.Count == 0) return;			
		
		TextLayout markerAncient = AncientFont.GetTextLayout("[A]");
		TextLayout markerPrimal = PrimalFont.GetTextLayout("[P]");

		DateTime now = Hud.Time.Now;
		IEnumerable<DropSnapshot> history = (LootHistoryTimeout > 0 ?
			LootHistory.Where(d => (now - d.Timestamp).TotalSeconds < LootHistoryTimeout) :
			LootHistory
		).OrderByDescending(d => d.Timestamp).Take(LootHistoryShown);
		List<Tuple<TextLayout, TextLayout, TextLayout, IFont, IBrush>> lines = new List<Tuple<TextLayout, TextLayout, TextLayout, IFont, IBrush>>(LootHistoryShown + 1);
			
		foreach (DropSnapshot drop in history)
		{
			// How long ago was the drop
			TimeSpan elapsed = now - drop.Timestamp;				
			string time;
			if (elapsed.TotalSeconds < 60)
				time = elapsed.TotalSeconds.ToString("F0") + "s ago";
			else if (elapsed.TotalMinutes < 10)
				time = elapsed.TotalMinutes.ToString("F0") + "m ago";
			else
				time = drop.Timestamp.ToString("hh:mm tt");

			// Name
			IFont color = (drop.IsSet ? SetFont : LegendaryFont);
				
			// Store line data
			lines.Add(new Tuple<TextLayout, TextLayout, TextLayout, IFont, IBrush>(
				TimeFont.GetTextLayout(time),
				(drop.IsPrimal ? markerPrimal : (drop.IsAncient ? markerAncient : null)),
				color.GetTextLayout(drop.ItemName),
				color,
				(elapsed.TotalSeconds <= LootHistoryHighlight ? HighlightBrush : BgBrush)
			));
		}
			
		// Compute width of the loot history display
		float widthTime = lines.Select(t => t.Item1.Metrics.Width).Max();
		float widthRank = Math.Max(markerAncient.Metrics.Width, markerPrimal.Metrics.Width);
		float widthName = lines.Select(t => t.Item3.Metrics.Width).Max();
		float width = Math.Max(rect.Width, 5 + widthTime + 10 + widthRank + 10 + widthName + 5);			
			
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
				
			//draw ancient rank
			layout = line.Item2;
			if (layout != null)
				(layout == markerPrimal ? PrimalFont : AncientFont).DrawText(line.Item2, x + 5 + widthTime + 5 + widthRank * 0.5f - layout.Metrics.Width *0.5f, y + height*0.5f - layout.Metrics.Height*0.5f);
				
			//draw item name
			layout = line.Item3;
			line.Item4.DrawText(layout, x + 5 + widthTime + 5 + widthRank + 5, y + height*0.5f - layout.Metrics.Height*0.5f);
		}
	}

    }
}