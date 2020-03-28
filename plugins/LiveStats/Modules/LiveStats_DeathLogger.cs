// TopStats Deaths Logger by HaKache

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
    public class LiveStats_DeathLogger : BasePlugin, ICustomizer, IChatLineChangedHandler //, IKeyEventHandler //, IInGameTopPainter
    {
 	public int DeadHistoryTimeout { get; set; } = 0;	// Stop showing old deaths after this many seconds (0 = never time out)
	public int DeadHistoryHighlight { get; set; } = 15;	// Highlight if the death is new for this many seconds
	public int DeadHistoryShown { get; set; } = 10;		// Max # of deaths to show for death history
	public List<DeadSnapshot> DeadHistory { get; private set; }

	public TopLabelDecorator Label { get; set; }
	public TopLabelDecorator Label0 { get; set; }
	public List<TopLabelDecorator> ExpandUpLabels { get; private set; }

	public IFont TextFont { get; set; }
	public IFont TitleFont { get; set; }
	public IFont TimeFont { get; set; }
	public IFont YellowFont { get; set; }

	public IBrush BgBrush { get; set; }
	public IBrush BgBrushAlt { get; set; }
	public IBrush BgBrushTitle { get; set; }
	public IBrush HighlightBrush { get; set; }

	public class DeadSnapshot {
		public string Id { get; set; }
		public DateTime Timestamp { get; set; }
		public string Line { get; set; }
		public bool IsGR { get; set; }
		public string Difficulty { get; set; }
	}

        public bool IsGreaterRift {
            get
            {
                return riftQuest != null &&
                       (riftQuest.QuestStepId == 13 || riftQuest.QuestStepId == 16 || riftQuest.QuestStepId == 34 ||
                        riftQuest.QuestStepId == 46);
            }
        }
        private IQuest riftQuest {
            get
            {
                return Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 337492) ?? // rift
                       Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 382695);   // gr
            }
        }

        public int Priority { get; set; } = 2;
        public int Hook { get; set; } = 1;

        public LiveStats_DeathLogger()
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

	    DeadHistory = new List<DeadSnapshot>();
		
	    TimeFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 255, 255, false, false, true);	
	    TextFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 0, 0, false, false, true);
	    TitleFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 0, 0, true, false, true);
	    YellowFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 255, 55, false, false, true);
			
	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
	    BgBrush = plugin.BgBrush;
	    BgBrushAlt = plugin.BgBrushAlt;
	    BgBrushTitle = Hud.Render.CreateBrush(175, 132, 50, 50, 0);
	    HighlightBrush = Hud.Render.CreateBrush(200, 72, 132, 84, 0);
			
	    Label = new TopLabelDecorator(Hud)
            {
                TextFont = TextFont,
                TextFunc = () => {
			double death = Hud.Tracker.Session.Death;
			return death.ToString("F0") + (death > 1 ? " deaths" : " death");
				},
                HintFunc = () => "Deaths Today",
		ExpandUpLabels = new List<TopLabelDecorator>()
                {
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Account",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => { 
				double death = Hud.Tracker.CurrentAccountTotal.Death;
				return death.ToString("F0") + (death > 1 ? " deaths" : " death");
					},
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountTotal.DeathPerHour, ValueFormat.ShortNumber) + " per hour",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Yesterday",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = TextFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => { 
				double death = Hud.Tracker.CurrentAccountYesterday.Death;
				return death.ToString("F0") + (death > 1 ? " deaths" : " death");
					},
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountYesterday.DeathPerHour, ValueFormat.ShortNumber) + " per hour",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Today",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => { 
				double death = Hud.Tracker.CurrentAccountToday.Death;
				return death.ToString("F0") + (death > 1 ? " deaths" : " death");
					},
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountToday.DeathPerHour, ValueFormat.ShortNumber) + " per hour",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Session",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = TextFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => { 
				double death = Hud.Tracker.Session.Death;
				return death.ToString("F0") + (death > 1 ? " deaths" : " death");
					},
                        HintFunc = () => ValueToString(Hud.Tracker.SessionAlwaysRunning.DeathPerHour, ValueFormat.ShortNumber) + " per hour",
                    },
		    new TopLabelDecorator(Hud) {
			TextFont = TextFont,
			TextFunc = () => {
			float w = plugin.WidthFunc();
			float h = plugin.HeightFunc();
			float x = Hook == 0 ? (plugin.PinnedLabel == Label ? plugin.ExpandLabelHook : plugin.SelectedRectangle.X) : (plugin.PinnedTopLabel == Label ? plugin.ExpandLabelTopHook : plugin.SelectedTopRectangle.X);
			float y = Hook == 0 ? (plugin.SelectedRectangle.Y - h * (ExpandUpLabels.Count - 0.5f)) : (plugin.SelectedTopRectangle.Y + h * (ExpandUpLabels.Count - 0.5f));
						
			DrawDeathLog(new RectangleF(x, y, w, h));
							
			return " ";
			    },
			},
		    },
		};

	    Label0 = new TopLabelDecorator(Hud)
            {
                TextFont = TextFont,
                TextFunc = () => {
			double death = Hud.Tracker.Session.Death;
			return death.ToString("F0") + (death > 1 ? " deaths" : " death");
				},
                HintFunc = () => "Deaths Today",
		ExpandUpLabels = new List<TopLabelDecorator>()
                {
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = TextFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => { 
				double death = Hud.Tracker.Session.Death;
				return death.ToString("F0") + (death > 1 ? " deaths" : " death");
					},
                        HintFunc = () => ValueToString(Hud.Tracker.SessionAlwaysRunning.DeathPerHour, ValueFormat.ShortNumber) + " per hour",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Session",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => { 
				double death = Hud.Tracker.CurrentAccountToday.Death;
				return death.ToString("F0") + (death > 1 ? " deaths" : " death");
					},
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountToday.DeathPerHour, ValueFormat.ShortNumber) + " per hour",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Today",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont = TextFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => { 
				double death = Hud.Tracker.CurrentAccountYesterday.Death;
				return death.ToString("F0") + (death > 1 ? " deaths" : " death");
					},
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountYesterday.DeathPerHour, ValueFormat.ShortNumber) + " per hour",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => "Yesterday",
                        HintFunc = () => " ",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TextFont,
			BackgroundBrush = BgBrush,
                        ExpandedHintFont = TextFont, //expandedHintFont,
			ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => { 
				double death = Hud.Tracker.CurrentAccountTotal.Death;
				return death.ToString("F0") + (death > 1 ? " deaths" : " death");
					},
                        HintFunc = () => ValueToString(Hud.Tracker.CurrentAccountTotal.DeathPerHour, ValueFormat.ShortNumber) + " per hour",
                    },
                    new TopLabelDecorator(Hud)
                    {
                        TextFont = TitleFont,
			BackgroundBrush = BgBrushTitle,
                        ExpandedHintFont = TitleFont, //expandedHintFont,
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
						
			DrawDeathLog(new RectangleF(x, y, w, h));
							
			return " ";
			    },
			},
		    },
		};
			
		if (Hook == 0) ExpandUpLabels = Label0.ExpandUpLabels;
			else ExpandUpLabels = Label.ExpandUpLabels;
	}

	public void OnChatLineChanged(string currentLine, string previousLine)
	{
	if (string.IsNullOrEmpty(currentLine)) return;

	    if (currentLine.Contains("was slain"))
		{
		currentLine = currentLine.Replace("!", "");
		currentLine = currentLine.Replace("{c_gold}", "");

		if (IsGreaterRift) {
		var death = new DeadSnapshot() { Line = currentLine, Timestamp = Hud.Time.Now, IsGR = true, Difficulty = Hud.Game.Me.InGreaterRiftRank.ToString("") };
		DeadHistory.Add(death); 
			} else {
		var death = new DeadSnapshot() { Line = currentLine, Timestamp = Hud.Time.Now, IsGR = false, Difficulty = Hud.Game.GameDifficulty.ToString("") };
		DeadHistory.Add(death); 
			}
		}
	}

	private void DrawDeathLog(RectangleF rect) //x,y coordinates of the bottom left of the rift history window (above the other labels + space gap)
	{
		if (DeadHistory == null || DeadHistory.Count == 0) return;

		DateTime now = Hud.Time.Now;
		IEnumerable<DeadSnapshot> history = (DeadHistoryTimeout > 0 ?
			DeadHistory.Where(d => (now - d.Timestamp).TotalSeconds < DeadHistoryTimeout) :
			DeadHistory
		).OrderByDescending(d => d.Timestamp).Take(DeadHistoryShown);
		List<Tuple<TextLayout, TextLayout, IBrush>> lines = new List<Tuple<TextLayout, TextLayout, IBrush>>(DeadHistoryShown + 1);
			
		foreach (DeadSnapshot death in history)
		{
			// How long ago was the drop
			TimeSpan elapsed = now - death.Timestamp;				
			string time;
			if (elapsed.TotalSeconds < 60)
				time = elapsed.TotalSeconds.ToString("F0") + "s ago";
			else if (elapsed.TotalMinutes < 10)
				time = elapsed.TotalMinutes.ToString("F0") + "m ago";
			else
				time = death.Timestamp.ToString("hh:mm tt");

			// IS GR ?
			string textrift;
			if (death.IsGR) textrift = " in GR" + death.Difficulty;
			else textrift = " in " + death.Difficulty.ToUpper();

			// Name
			IFont color = YellowFont;

			// Store line data
			lines.Add(new Tuple<TextLayout, TextLayout, IBrush>(
				TextFont.GetTextLayout(time),
				TextFont.GetTextLayout(death.Line + textrift),
				(elapsed.TotalSeconds <= DeadHistoryHighlight ? HighlightBrush : BgBrush)
			));
		}
			
		// Compute width of the loot history display
		float widthTime = lines.Select(t => t.Item1.Metrics.Width).Max();
		float widthLine = lines.Select(t => t.Item2.Metrics.Width).Max();
		float width = Math.Max(rect.Width, 5 + widthTime + 10 + widthLine + 5);			
			
		//x += width;
		float x = rect.X;
		if (x + width > Hud.Window.Size.Width)
			x = Hud.Window.Size.Width - width;
		float y = rect.Y;
			
		foreach (Tuple<TextLayout, TextLayout, IBrush> line in lines)
		{
			TextLayout layout = line.Item1;
			float height = Math.Max(layout.Metrics.Height, rect.Height);
			if (Hook == 0) y -= height;
				else y += height;
				
			//draw background
			line.Item3.DrawRectangle(x, y, width, height); 
				
			//draw timestamp
			TimeFont.DrawText(layout, x + 5 + widthTime - layout.Metrics.Width, y + height*0.5f - layout.Metrics.Height*0.5f); 
				
			//draw death infos
			layout = line.Item2;
			if (layout != null)
				YellowFont.DrawText(line.Item2, x + 5 + widthTime + 5, y + height*0.5f - layout.Metrics.Height*0.5f);
		}
	}

    }
}