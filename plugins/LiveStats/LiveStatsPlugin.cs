// LiveStats Plugin by HaKache
// Based on RunStats Plugin by Razorfish
// Menu Labels auto-resize inspired by Resu

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using SharpDX.DirectInput;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.LiveStats
{
    public class LiveStatsPlugin : BasePlugin, IInGameTopPainter, IKeyEventHandler
    {
	public IBrush BgBrush { get; set; }
	public IBrush BgBrushAlt { get; set; }
	public IBrush BgBrushTitle { get; set; }
	public IBrush BarBgBrush { get; set; }
	public IBrush SelectedBgBrush { get; set; }
	public IBrush BorderBrush { get; set; }

	public float ExpandLabelHook { get; private set; }
	public float ExpandLabelTopHook { get; private set; }
		
	// Compute placement and dimensions dynamically, similar to AbstractTopLabelList
        public Func<float> WidthFunc { get; set; } // Width of a label
        public Func<float> HeightFunc { get; set; } // Height of a label
		
	// Session clock time display
	public TopLabelDecorator SessionLabel { get; set; }
	public TopLabelDecorator UptimeLabel { get; set; }
	public TopLabelDecorator Pin { get; set; }

	public IFont PinFont { get; set; }
	public IFont PinnedFont { get; set; }
	public int HoverPinDelay { get; set; } = 700; // Milliseconds
	public int RepeatPinDelay { get; set; } = 2000; // Milliseconds - Keep hovering over a pin to re/un-pin (0 = don't repeat until mouse is moved away from pin first)

	// Hover state
	public TopLabelDecorator SelectedLabel { get; private set; }
	public TopLabelDecorator SelectedTopLabel { get; private set; }
	public TopLabelDecorator PinnedLabel { get; private set; }
	public TopLabelDecorator PinnedTopLabel { get; private set; }
	public RectangleF SelectedRectangle { get; private set; }
	public RectangleF SelectedTopRectangle { get; private set; }
	//public RectangleF PinnedRectangle { get; private set; }
	public float PinnedX { get; private set; }
	public float PinnedTopX { get; private set; }
		
	// Internal bookkeeping
	private List<Tuple<TopLabelDecorator, int, int>> Labels = new List<Tuple<TopLabelDecorator, int, int>>(); // Label - Priority - Hook
	private Dictionary<TopLabelDecorator, List<TopLabelDecorator>> ExpandUpLabels = new Dictionary<TopLabelDecorator, List<TopLabelDecorator>>();
	private IUiElement Anchor;
	private IWatch Hover; // Handled by OnMouseOver and OnMouseOut
	private bool HoverRepeat = false; // Handled by OnMouseOver and OnMouseOut
        //private HorizontalTopLabelList LabelList;

	// Bag Space Widget
        public bool BagWidget { get; set; }
        public TopLabelDecorator RedDecorator { get; set; }
        public TopLabelDecorator YellowDecorator { get; set; }
        public TopLabelDecorator GreenDecorator { get; set; }

	// Key Toggle
        public bool Show { get; set; }
        public IKeyEvent ToggleKeyEvent { get; set; }
        public Key HotKey
        {
            get { return ToggleKeyEvent.Key; }
            set { ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, value, false, false, false); }
        }

	// Bool Options
        public bool SessionHelper { get; set; } = true;	// Show the Session Module in the bar
        public bool TimeTracker { get; set; } = true;	// Show the Time Tracker Module in the bar
        public bool LockPinInCombat { get; set; } = false;

        public LiveStatsPlugin() : base()
        {
            Enabled = true;
	    Order = -10000;
        }
		
        public override void Load(IController hud)
        {
            base.Load(hud);

            HotKey = Key.F7;
            Show = false; // True = LiveStats is shown by Default - False = LiveStats is hidden by Default
            BagWidget = true; // True = Enable a Bag Space Tool at the edge of RunStats Bar
	
	    BarBgBrush = Hud.Render.CreateBrush(100, 0, 0, 0, 0);
	    BgBrush = Hud.Render.CreateBrush(200, 0, 0, 0, 0);
	    BgBrushAlt = Hud.Render.CreateBrush(200, 15, 15, 15, 0);
	    BgBrushTitle = Hud.Render.CreateBrush(175, 40, 84, 92, 0); // 50, 84, 132 // 205, 184, 87
	    BorderBrush = Hud.Render.CreateBrush(35, 190, 190, 190, 1);
	    SelectedBgBrush = Hud.Render.CreateBrush(175, 0, 0, 0, 0);

	    Anchor = Hud.Render.GetPlayerSkillUiElement(ActionKey.Heal);
	    float x = Anchor.Rectangle.Right + 5;
	    float y = Anchor.Rectangle.Bottom;
			
	    // LeftFunc = () => Hud.Window.Size.Width - LabelList.LabelDecorators.Count * (LabelList.WidthFunc() - 1); // Anchor.Rectangle.Right;
	    // TopFunc = () => Anchor.Rectangle.Bottom + 1;
	    WidthFunc = () => Hud.Window.Size.Height * 0.1034f; // 0.105f Default // Hud.Window.Size.Width - Anchor.Rectangle.Right;
	    HeightFunc = () => (float)Math.Ceiling(Hud.Window.Size.Height - Anchor.Rectangle.Bottom) - 1f;

	    ExpandLabelHook = Hud.Window.Size.Width;
			
	    IFont timeFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 234, 137, false, false, true);
	    IFont TitleFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 234, 137, true, false, true);

            RedDecorator = new TopLabelDecorator(Hud)
		{
		    TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 100, 100, true, false, 255, 0, 0, 0, true),
		    BackgroundBrush = BarBgBrush,
		    // BorderBrush = BorderBrush,
		    TextFunc = () => (Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed).ToString("D", CultureInfo.InvariantCulture),
		    HintFunc = () => "Free Space in Inventory",
		};
            YellowDecorator = new TopLabelDecorator(Hud)
		{
		    TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 200, 205, 50, true, false, 255, 0, 0, 0, true),
		    BackgroundBrush = BarBgBrush,
		    // BorderBrush = BorderBrush,
		    TextFunc = () => (Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed).ToString("D", CultureInfo.InvariantCulture),
		    HintFunc = () => "Free Space in Inventory",
		};
            GreenDecorator = new TopLabelDecorator(Hud)
		 {
		    TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 100, 130, 100, true, false, 255, 0, 0, 0, true),
		    BackgroundBrush = BarBgBrush,
		    // BorderBrush = BorderBrush,
		    TextFunc = () => (Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed).ToString("D", CultureInfo.InvariantCulture),
		    HintFunc = () => "Free Space in Inventory",
		};

            SessionLabel = new TopLabelDecorator(Hud)
            {
                TextFont = timeFont,
                TextFunc = () => ValueToString(Hud.Tracker.SessionAlwaysRunning.ElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime), //ValueToString(Hud.Game.Me.Defense.EhpCur, ValueFormat.ShortNumber),
                HintFunc = () => "Hud has been active for this amount of time",
				ExpandUpLabels = new List<TopLabelDecorator>()
                {
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = timeFont,
                        ExpandedHintFont = timeFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			BackgroundBrush = BgBrush,
                        TextFunc = () => ValueToString(Hud.Tracker.Session.TownElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime),
                        HintFunc = () => "Town Time",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = timeFont,
                        ExpandedHintFont = timeFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			BackgroundBrush = BgBrushAlt,
                        TextFunc = () => ValueToString(Hud.Tracker.Session.PlayElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime),
                        HintFunc = () => "Play Time",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = timeFont,
                        ExpandedHintFont = timeFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
			BackgroundBrush = BgBrush,
                        TextFunc = () => ValueToString((Hud.Tracker.SessionAlwaysRunning.ElapsedMilliseconds - Hud.Tracker.Session.ElapsedMilliseconds) * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime),
                        HintFunc = () => "Loading & Menus",
                    },
                },
            };
			
			//if (SessionHelper) Add(SessionLabel, -1, 0); // Show the Session Module in the RunStats bar

            UptimeLabel = new TopLabelDecorator(Hud)
            {
                TextFont = timeFont,
                TextFunc = () => ValueToString(Hud.Tracker.SessionAlwaysRunning.ElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime), //ValueToString(Hud.Game.Me.Defense.EhpCur, ValueFormat.ShortNumber),
                HintFunc = () => "Session (Abs)",
				ExpandUpLabels = new List<TopLabelDecorator>()
                {
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = timeFont,
                        ExpandedHintFont = timeFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = 1f,
			BackgroundBrush = BgBrush,
                        TextFunc = () => "Account Time",
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountTotal.ElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime),
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
                        ExpandedHintFont = TitleFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = 1f,
			BackgroundBrush = BgBrushTitle,
                        TextFunc = () => "Town Time",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = timeFont,
                        ExpandedHintFont = timeFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = 1f,
			BackgroundBrush = BgBrush,
                        TextFunc = () => "Account",
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountTotal.TownElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime),
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = timeFont,
                        ExpandedHintFont = timeFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = 1f,
			BackgroundBrush = BgBrushAlt,
                        TextFunc = () => "Yesterday",
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountYesterday.TownElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime),
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = timeFont,
                        ExpandedHintFont = timeFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = 1f,
			BackgroundBrush = BgBrush,
                        TextFunc = () => "Today",
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountToday.TownElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime),
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = timeFont,
                        ExpandedHintFont = timeFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = 1f,
			BackgroundBrush = BgBrushAlt,
                        TextFunc = () => "Session",
                        HintFunc = () => ValueToString(Hud.Tracker.Session.TownElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime),
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
                        ExpandedHintFont = TitleFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = 1f,
			BackgroundBrush = BgBrushTitle,
                        TextFunc = () => "Play Time",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = timeFont,
                        ExpandedHintFont = timeFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = 1f,
			BackgroundBrush = BgBrush,
                        TextFunc = () => "Account",
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountTotal.PlayElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime),
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = timeFont,
                        ExpandedHintFont = timeFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = 1f,
			BackgroundBrush = BgBrushAlt,
                        TextFunc = () => "Yesterday",
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountYesterday.PlayElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime),
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = timeFont,
                        ExpandedHintFont = timeFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = 1f,
			BackgroundBrush = BgBrush,
                        TextFunc = () => "Today",
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountToday.PlayElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime),
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = timeFont,
                        ExpandedHintFont = timeFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = 1f,
			BackgroundBrush = BgBrushAlt,
                        TextFunc = () => "Session",
                        HintFunc = () => ValueToString(Hud.Tracker.Session.PlayElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime),
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
                        ExpandedHintFont = timeFont,
                        //ExpandedHintWidthMultiplier = 0.75f,
                        ExpandedHintWidthMultiplier = 1f,
			BackgroundBrush = BgBrushTitle,
                        TextFunc = () => "Session ABS",
                        HintFunc = () => ValueToString(Hud.Tracker.SessionAlwaysRunning.PlayElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime),
                    },
                },
            };
			
			//if (TimeTracker) Add(UptimeLabel, -1, 1); // Show the TimeTracker Module in the TopStats bar
			
	    PinFont = Hud.Render.CreateFont("tahoma", 6, 255, 185, 185, 185, false, false, true);
	    PinnedFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 55, 55, false, false, true);

            Pin = new TopLabelDecorator(Hud)
            {
		//Enabled = false, //test
                TextFont = PinFont,
                TextFunc = () => "📌", //🖈
	    };
			
	    Hover = Hud.Time.CreateWatch();
        }
		
        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (keyEvent.IsPressed && ToggleKeyEvent.Matches(keyEvent))
            {
                Show = !Show;
            }
        }

        public void PaintTopInGame(ClipState clipState)
        {
	    if (!Show) return;
            if (Hud.Render.UiHidden) return;

            if (((PinnedLabel != null || PinnedTopLabel != null) && clipState != ClipState.BeforeClip) || // If there is a pinned label, allow it to be clipped
				((PinnedLabel == null && PinnedTopLabel == null) && clipState != ClipState.AfterClip)) // No pinned labels, don't clip it
                return;

		float w = WidthFunc();
		float w2 = WidthFunc();
		float h = HeightFunc();

		// RunStats
		IEnumerable<TopLabelDecorator> activeLabels = Labels.Where(p => p.Item1.Enabled && p.Item3 == 0).OrderBy(p => p.Item2).Select(p => p.Item1);
		int count = activeLabels.Count();
		float x = Hud.Window.Size.Width - count * w;
		float y = Hud.Window.Size.Height - h;

		// TopStats
		IEnumerable<TopLabelDecorator> activeTopLabels = Labels.Where(p => p.Item1.Enabled && p.Item3 >= 1).OrderBy(p => p.Item2).Select(p => p.Item1);
		int topcount = activeTopLabels.Count();
		float x2 = Hud.Window.Size.Width * 0.5f - (topcount * w) / 2;
		float y2 = Hud.Window.Size.Height * 0.0f;

		// Define the Top Pinned Label Hook
	        ExpandLabelTopHook = Hud.Window.Size.Width * 0.5f - (topcount * w) / 2; // Hud.Window.Size.Width * 0.355f

		TopLabelDecorator selectedLabel = null;
		TopLabelDecorator selectedTopLabel = null;
		float selectedX = 0;
		float selectedtopX = 0;
		
		// Define variables for the Bag Widget
		var BagWidth = Hud.Render.InGameBottomHudUiElement.Rectangle.Width * 0.03f;
		var FreeSpace = Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed;
		var BagDecorator = FreeSpace < 6 ? RedDecorator : FreeSpace < 30 ? YellowDecorator : GreenDecorator;

		// Manage the Bag Widget display and auto label resize at the same time
		if (BagWidget) // Draw mini label at the edge of RunStats for Inventory Space
		{
		    if (x - BagWidth < Anchor.Rectangle.Right)
		    {
			x = Anchor.Rectangle.Right + BagWidth;
			w2 = (Hud.Window.Size.Width - Anchor.Rectangle.Right - BagWidth) / count;
		    }
		    BagDecorator.Paint(x - BagWidth, y, BagWidth, h, HorizontalAlign.Center);
		    BorderBrush.DrawLine(x - BagWidth, y, x, y);
		}
		else
		{
		    if (x < Anchor.Rectangle.Right)
		    {
			x = Anchor.Rectangle.Right;
			w2 = (Hud.Window.Size.Width - Anchor.Rectangle.Right) / count;
		    }
		}

		// RunStats
		BorderBrush.DrawLine(x, y, x + w2*count, y);
		BarBgBrush.DrawRectangle(x, y, w2*count, h);

		// TopStats
		BorderBrush.DrawLine(x2, y2 + h, x2 + (w*topcount), y2 + h);
		BarBgBrush.DrawRectangle(x2, y2, w*topcount, h);
	
		// RunStats
		foreach (TopLabelDecorator label in activeLabels) 
		{
			IBrush prev = null;
			IBrush prevBorder = null;
			if (Hud.Window.CursorInsideRect(x, y, w2, h))
			{
				selectedLabel = label;
				selectedX = x;
				SelectedRectangle = new RectangleF(x, y, w2, h);
				
				prev = label.BackgroundBrush;
				prevBorder = label.BorderBrush;
				label.BackgroundBrush = SelectedBgBrush;
				label.BorderBrush = BorderBrush;
			}
			else if (label == PinnedLabel)
			{
				prev = label.BackgroundBrush;
				prevBorder = label.BorderBrush;
				label.BackgroundBrush = SelectedBgBrush;
				label.BorderBrush = BorderBrush;
			}

			label.Paint(x, y, w2, h, HorizontalAlign.Center);				
			label.BackgroundBrush = prev; // Revert draw settings to default state
			label.BorderBrush = prevBorder; // Revert draw settings to default state
				
			x += w2;
		}

		// TopStats
		foreach (TopLabelDecorator label2 in activeTopLabels) 
		{
			IBrush prev = null;
			IBrush prevBorder = null;
			if (Hud.Window.CursorInsideRect(x2, y2, w, h))
			{
				selectedTopLabel = label2;
				selectedtopX = x2;
				SelectedTopRectangle = new RectangleF(x2, y2, w, h);
					
				prev = label2.BackgroundBrush;
				prevBorder = label2.BorderBrush;
				label2.BackgroundBrush = SelectedBgBrush;
				label2.BorderBrush = BorderBrush;
			}
			else if (label2 == PinnedTopLabel)
			{
				prev = label2.BackgroundBrush;
				prevBorder = label2.BorderBrush;
				label2.BackgroundBrush = SelectedBgBrush;
				label2.BorderBrush = BorderBrush;
			}

			label2.Paint(x2, y2, w, h, HorizontalAlign.Center);				
			label2.BackgroundBrush = prev; // Revert draw settings to default state
			label2.BorderBrush = prevBorder; // Revert draw settings to default state
				
			x2 += w;
		}
			
		// Draw RunStats pinned label extras
		if (PinnedLabel != null)
		{
			// Expand up labels always open for pinned label, anchor it to the right side of the screen
			PaintExpandUpLabels(PinnedLabel, Hud.Window.Size.Width, y, w2, h);
			
			// Draw pinned pin icon
			Pin.TextFont = PinnedFont;
			Pin.Paint(PinnedX + w2*0.85f, y, w2*0.15f, h, HorizontalAlign.Left);
		}

		// Draw TopStats pinned label extras
		if (PinnedTopLabel != null)
		{
			// Expand up labels always open for pinned label, anchor it to the right side of the screen
			PaintExpandDownLabels(PinnedTopLabel, Hud.Window.Size.Width * 0.5f - (topcount * w) / 2, y2, w, h);
				
			// Draw pinned pin icon
			Pin.TextFont = PinnedFont;
			Pin.Paint(PinnedTopX + w*0.85f, y2, w*0.15f, h, HorizontalAlign.Left);
		}
			
		// Draw the selected label's expanduplabels last so that it appears on top
		if (selectedLabel != null)
		{
			// Show pop up menus if they're not already pinned open
			if (selectedLabel != PinnedLabel)
			{
				PaintExpandUpLabels(selectedLabel, selectedX, y, w2, h);

				if (LockPinInCombat && Hud.Game.Me.InCombat) return;

				// Draw an unpinned pin icon
				Pin.TextFont = PinFont;
				Pin.Paint(selectedX + w2*0.85f, y, w2*0.15f, h, HorizontalAlign.Left);
			}

			if (Hud.Window.CursorInsideRect(selectedX + w2*0.85f, y, w2*0.15f, h))
			{
				if (LockPinInCombat && Hud.Game.Me.InCombat) return;

				if (OnMouseOver(HoverPinDelay, RepeatPinDelay)) // Has the mouse been hovering over the pin for at least HoverPinDelay milliseconds?
				{
					if (PinnedLabel == selectedLabel) // Unpin
					{
						PinnedLabel = null;
						PinnedX = 0;
					}
					else // Pin
					{
						PinnedLabel = selectedLabel;
						PinnedX = selectedX;
					}
				}
			}
			else 
				OnMouseOut();
		} else 
			if (selectedTopLabel == null) OnMouseOut();

		SelectedLabel = selectedLabel;

		// Draw the selected label's expanddownlabels last so that it appears on bottom
		if (selectedTopLabel != null && selectedLabel == null)
		{
			// Show pop up menus if they're not already pinned open
			if (selectedTopLabel != PinnedTopLabel)
			{
				PaintExpandDownLabels(selectedTopLabel, selectedtopX, y2, w, h);

				if (LockPinInCombat && Hud.Game.Me.InCombat) return;

				// Draw an unpinned pin icon
				Pin.TextFont = PinFont;
				Pin.Paint(selectedtopX + w*0.85f, y2, w*0.15f, h, HorizontalAlign.Left);
			}

			if (Hud.Window.CursorInsideRect(selectedtopX + w*0.85f, y2, w*0.15f, h))
			{
				if (LockPinInCombat && Hud.Game.Me.InCombat) return;

				if (OnMouseOver(HoverPinDelay, RepeatPinDelay)) // Has the mouse been hovering over the pin for at least HoverPinDelay milliseconds?
				{
					if (PinnedTopLabel == selectedTopLabel) // Unpin
					{
						PinnedTopLabel = null;
						PinnedTopX = 0;
					}
					else // Pin
					{
						PinnedTopLabel = selectedTopLabel;
						PinnedTopX = selectedtopX;
					}
				}
			}
			else 
				OnMouseOut();
		} else 
			//OnMouseOut();

		SelectedTopLabel = selectedTopLabel;
        }
		
	public void Add(TopLabelDecorator label, int priority = 1, int hook = 0)
	{
		if (!Labels.Any(l => l.Item1 == label))
		{
			Labels.Add(new Tuple<TopLabelDecorator, int, int>(label, priority, hook));
			
			// Handle hover behavior in this plugin instead of delegating it to TopLabelDecorator
			if (label.ExpandUpLabels != null)
			{
				ExpandUpLabels[label] = label.ExpandUpLabels;
				label.ExpandUpLabels = null;
				label.HintFunc = null;
			}
		}
	}
		
	public void Remove(TopLabelDecorator label)
	{
	    Labels.RemoveAll(l => l.Item1 == label);
	    ExpandUpLabels.Remove(label);
	}
		
	private void PaintExpandUpLabels(TopLabelDecorator label, float x, float y, float w, float h)
	{
		List<TopLabelDecorator> labels;
		if (ExpandUpLabels.TryGetValue(label, out labels))
		{
			foreach (TopLabelDecorator tld in labels.Where(l => l.Enabled))
			{
				float width = w * (tld.ExpandedHintWidthMultiplier + 1);
				y -= h;
				
				if (x + width > Hud.Window.Size.Width)
					x = Hud.Window.Size.Width - width;

				tld.Paint(x, y, w, h, HorizontalAlign.Center);
				tld.PaintExpandedHint(x + w, y, w * tld.ExpandedHintWidthMultiplier, h, HorizontalAlign.Left);
			}
		}
	}

	private void PaintExpandDownLabels(TopLabelDecorator label, float x, float y, float w, float h)
	{
		List<TopLabelDecorator> labels;
		if (ExpandUpLabels.TryGetValue(label, out labels))
		{
			foreach (TopLabelDecorator tld in labels.Where(l => l.Enabled))
			{
				float width = w * (tld.ExpandedHintWidthMultiplier + 1);
				y += h;
					
				if (x + width > Hud.Window.Size.Width)
					x = Hud.Window.Size.Width - width;

				tld.Paint(x, y, w, h, HorizontalAlign.Center);
				tld.PaintExpandedHint(x + w, y, w * tld.ExpandedHintWidthMultiplier, h, HorizontalAlign.Left);
			}
		}
	}
		
	// Returns whether or not mouse has been hovering for at least 'delay' amount of time
	private bool OnMouseOver(int delay, int repeat = 0)
	{
		if (!Hover.IsRunning)
		{
			Hover.Start();
			HoverRepeat = false;
		} 
		else if ((!HoverRepeat && Hover.TimerTest(delay)) || (repeat > 0 && HoverRepeat && Hover.TimerTest(repeat)))
		{
			Hover.Restart();
			HoverRepeat = true;
			return true;
		}	
		return false;
	}
		
	private void OnMouseOut()
	{
	    Hover.Stop();
	    Hover.Reset();
	    HoverRepeat = false;
	}

    }
}