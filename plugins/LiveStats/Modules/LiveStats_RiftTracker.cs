// TopStats Rifts Tracker by HaKache

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
    public class LiveStats_RiftTracker : BasePlugin, ICustomizer, IMonsterKilledHandler, IChatLineChangedHandler
    {
 	public int RiftHistoryTimeout { get; set; } = 0;	// Stop showing old rift clears after this many seconds (0 = never time out)
	public int RiftHistoryHighlight { get; set; } = 20;	// Highlight if the drop is new for this many seconds
	public int RiftHistoryShown { get; set; } = 12;		// Max # of clears to show for rift history
	public List<RiftSnapshot> RiftHistory { get; private set; }
		
	public TopLabelDecorator Label { get; set; }
	public List<TopLabelDecorator> ExpandUpLabels { get; private set; }
	
	public IFont RiftFont { get; set; }
	public IFont BossFont { get; set; }
	public IFont TimeFont { get; set; }

	public IBrush BgBrush { get; set; }
	public IBrush BgBrushAlt { get; set; }
	public IBrush HighlightBrush { get; set; }

	public int TotalRifts { get; private set; } = 0;
	public int TotalGRs { get; private set; } = 0;
	public int TotalNephalems { get; private set; } = 0;

        private List<uint> RiftMap { get; set; }

	public class RiftSnapshot {
		public string Id { get; set; }
		public DateTime Timestamp { get; set; }
		public string BossName { get; set; }
		public bool IsGR { get; set; }
		public string Difficulty { get; set; }
		public string Timer { get; set; }
	    }

        public bool IsHamelinDead {
            get
            {
                if (Hud.Game.Monsters.Any(m => m.Rarity == ActorRarity.Boss && m.SnoActor.Sno == ActorSnoEnum._x1_lr_boss_ratking_a && !m.IsAlive))
                    return true;

                return riftQuest != null && (riftQuest.QuestStepId == 5 || riftQuest.QuestStepId == 10 || riftQuest.QuestStepId == 34 || riftQuest.QuestStepId == 46);
            }
        }

        public bool IsNephalemRift {
            get
            {
                return riftQuest != null && (riftQuest.QuestStepId == 1 || riftQuest.QuestStepId == 3 || riftQuest.QuestStepId == 10);
            }
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

        public int Priority { get; set; } = 8;
        public int Hook { get; set; } = 1;
		
        public LiveStats_RiftTracker()
	{
	    Enabled = true;
	}
		
	public void Customize()
	{
	    // Add this display to the LiveStats readout with a specified positional order priority of 8
	    Hud.RunOnPlugin<LiveStatsPlugin>(plugin => { 
			plugin.Add(this.Label, this.Priority, this.Hook); 
		}); 
	}

        public override void Load(IController hud)
        {
            base.Load(hud);

	    RiftHistory = new List<RiftSnapshot>();

            RiftMap = new List<uint> { 288482, 288684, 288686, 288797, 288799, 288801, 288803, 288809, 288812, 288813 }; // Rift Level 1-10

	    TimeFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 255, 255, false, false, true);
	    RiftFont = Hud.Render.CreateFont("tahoma", 7, 255, 198, 86, 255, false, false, true); //205, 102, 255
	    BossFont = Hud.Render.CreateFont("tahoma", 7, 190, 255, 0, 0, false, false, true);
			
	    var plugin = Hud.GetPlugin<LiveStatsPlugin>();
	    BgBrush = plugin.BgBrush;
	    BgBrushAlt = plugin.BgBrushAlt;
	    HighlightBrush = Hud.Render.CreateBrush(200, 72, 132, 84, 0); // 208, 156, 255 Light Purple
			
            Label = new TopLabelDecorator(Hud)
            {
                TextFont = RiftFont,
		TextFunc = () => { return TotalRifts.ToString() + (TotalRifts > 1 ? " rifts" : " rift"); },
                HintFunc = () => "Rifts Done",
                ExpandUpLabels = new List<TopLabelDecorator>() {
                    new TopLabelDecorator(Hud) {
                        //Enabled = false,
                        TextFont = RiftFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  RiftFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => TotalRifts.ToString(),
                        HintFunc = () => "Total Rifts",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  RiftFont,
                        BackgroundBrush = BgBrushAlt,
                        ExpandedHintFont =  RiftFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => TotalNephalems.ToString() + " (" + (TotalNephalems / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F1", CultureInfo.InvariantCulture) + "/h)",
                        HintFunc = () => "Nephalem Rifts",
                    },
                    new TopLabelDecorator(Hud) {
                        TextFont =  RiftFont,
                        BackgroundBrush = BgBrush,
                        ExpandedHintFont =  RiftFont,
                        ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 1.0),
                        TextFunc = () => TotalGRs.ToString() + " (" + (TotalGRs / ((((double)Hud.Tracker.Session.ElapsedMilliseconds/1000d)/60d)/60d)).ToString("F1", CultureInfo.InvariantCulture) + "/h)",
                        HintFunc = () => "Greater Rifts",
                    },
		    new TopLabelDecorator(Hud) {
			TextFont = RiftFont,
			TextFunc = () => {
			float w = plugin.WidthFunc();
			float h = plugin.HeightFunc();
			float x = Hook == 0 ? (plugin.PinnedLabel == Label ? plugin.ExpandLabelHook : plugin.SelectedRectangle.X) : (plugin.PinnedTopLabel == Label ? plugin.ExpandLabelTopHook : plugin.SelectedTopRectangle.X);
			float y = Hook == 0 ? (plugin.SelectedRectangle.Y - h * (ExpandUpLabels.Count - 0.5f)) : (plugin.SelectedTopRectangle.Y + h * (ExpandUpLabels.Count - 0.5f));
						
			DrawRiftHistory(new RectangleF(x, y, w, h));
							
			return " ";
			    },
			},
		    },
		};
			
		ExpandUpLabels = Label.ExpandUpLabels;
	}

	public void OnChatLineChanged(string currentLine, string previousLine)
	{
	if (string.IsNullOrEmpty(currentLine)) return;
	bool BossAround = (Hud.Game.Monsters.Count(m => m.Rarity == ActorRarity.Boss) > 0);

	    if (currentLine.Contains("has killed Agnidox") || currentLine.Contains("has killed Blighter") || currentLine.Contains("has killed Bloodmaw") ||
		currentLine.Contains("has killed Bone Warlock") || currentLine.Contains("has killed Cold Snap") || currentLine.Contains("has killed Crusader King") ||
		currentLine.Contains("has killed Ember") || currentLine.Contains("has killed Erethon") || currentLine.Contains("has killed Eskandiel") ||
		currentLine.Contains("has killed Infernal Maiden") || currentLine.Contains("has killed Man Carver") || currentLine.Contains("has killed Orlash") ||
		currentLine.Contains("has killed Perdition") || currentLine.Contains("has killed Perendi") || currentLine.Contains("has killed Raiziel") ||
		currentLine.Contains("has killed Rime") || currentLine.Contains("has killed Sand Shaper") || currentLine.Contains("has killed Saxtris") ||
		currentLine.Contains("has killed Stonesinger") || currentLine.Contains("has killed Tethrys") || currentLine.Contains("has killed The Binder") ||
		currentLine.Contains("has killed The Choker") || currentLine.Contains("has killed Vesalius") || currentLine.Contains("has killed Voracity") ||
		currentLine.Contains("has killed Josh Mosqueira") || currentLine.Contains("has killed Lord of Bells"))
		{
		if (IsNephalemRift) { 
		    TotalNephalems += 1; TotalRifts += 1;
			if (!BossAround) { 
		currentLine = currentLine.Replace("!", "");
		string output = currentLine.Substring(currentLine.IndexOf("killed ") + 7);

		var rift = new RiftSnapshot() { BossName = output, Timestamp = Hud.Time.Now, IsGR = false, Difficulty = Hud.Game.GameDifficulty.ToString("") };
		RiftHistory.Add(rift); 
			}
		    }
		if (IsGreaterRift) { 
		    TotalGRs += 1; TotalRifts += 1;
			if (!BossAround) { 
		currentLine = currentLine.Replace("!", "");
		string output = currentLine.Substring(currentLine.IndexOf("killed ") + 7);

		var rift = new RiftSnapshot() { BossName = output, Timestamp = Hud.Time.Now, IsGR = true, Timer = ValueToString((long)(Hud.Game.CurrentGameTick - Hud.Game.CurrentTimedEventStartTick) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime) };
		RiftHistory.Add(rift); 
			}
		    }
		}

	    if (currentLine.Contains("has engaged: Hamelin") && IsHamelinDead) // Handling Hamelin (No OnKill Message)
		{
		if (IsNephalemRift) { 
		    TotalNephalems += 1; TotalRifts += 1;
			if (!BossAround) { 
		var rift = new RiftSnapshot() { BossName = "Hamelin", Timestamp = Hud.Time.Now, IsGR = false, Difficulty = Hud.Game.GameDifficulty.ToString("") };
		RiftHistory.Add(rift); 
			}
		    }
		if (IsGreaterRift) { 
		    TotalGRs += 1; TotalRifts += 1;
			if (!BossAround) { 
		var rift = new RiftSnapshot() { BossName = "Hamelin", Timestamp = Hud.Time.Now, IsGR = true, Timer = ValueToString((long)(Hud.Game.CurrentGameTick - Hud.Game.CurrentTimedEventStartTick) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime) };
		RiftHistory.Add(rift); 
			}
		    }
		}

	}

	public void OnMonsterKilled(IMonster monster)
	{
	if (monster.Rarity != ActorRarity.Boss) return;
	if (monster.SnoActor.Sno == ActorSnoEnum._x1_lr_boss_terrordemon_a_breathminion) return; // Echo of Orlash
	if (!RiftMap.Contains(Hud.Game.Me.SnoArea.Sno)) return; // Return if you're not in a Rift Zone

	    if (IsGreaterRift)
	    {
		//if (monster.SnoActor.Sno == ActorSnoEnum._x1_lr_boss_ratking_a) { TotalGRs += 1; TotalRifts += 1; } // Handling Hamelin

		// Add Boss Killed & Rift Infos into Rift History
		var rift = new RiftSnapshot() { BossName = monster.SnoMonster.NameLocalized, Timestamp = Hud.Time.Now, IsGR = true, Difficulty = Hud.Game.Me.InGreaterRiftRank.ToString(""), Timer = ValueToString((long)(Hud.Game.CurrentGameTick - Hud.Game.CurrentTimedEventStartTick) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime) };
		RiftHistory.Add(rift);
	    }

	    if (IsNephalemRift) 
	    { 
		//if (monster.SnoActor.Sno == ActorSnoEnum._x1_lr_boss_ratking_a) { TotalNephalems += 1; TotalRifts += 1; } // Handling Hamelin

		// Add Boss Killed & Rift Infos into Rift History
		var rift = new RiftSnapshot() { BossName = monster.SnoMonster.NameLocalized, Timestamp = Hud.Time.Now, IsGR = false, Difficulty = Hud.Game.GameDifficulty.ToString("") };
		RiftHistory.Add(rift);
	    }

	}

		
	private void DrawRiftHistory(RectangleF rect) //x,y coordinates of the bottom left of the rift history window (above the other labels + space gap)
	{
		if (RiftHistory == null || RiftHistory.Count == 0) return;

		DateTime now = Hud.Time.Now;
		IEnumerable<RiftSnapshot> history = (RiftHistoryTimeout > 0 ?
			RiftHistory.Where(d => (now - d.Timestamp).TotalSeconds < RiftHistoryTimeout) :
			RiftHistory
		).OrderByDescending(d => d.Timestamp).Take(RiftHistoryShown);
		List<Tuple<TextLayout, TextLayout, TextLayout, IFont, IBrush>> lines = new List<Tuple<TextLayout, TextLayout, TextLayout, IFont, IBrush>>(RiftHistoryShown + 1);
			
		foreach (RiftSnapshot rift in history)
		{
			// How long ago was the drop
			TimeSpan elapsed = now - rift.Timestamp;				
			string time;
			if (elapsed.TotalSeconds < 60)
				time = elapsed.TotalSeconds.ToString("F0") + "s ago";
			else if (elapsed.TotalMinutes < 10)
				time = elapsed.TotalMinutes.ToString("F0") + "m ago";
			else
				time = rift.Timestamp.ToString("hh:mm tt");

			// IS GR ?
			string textrift;
			if (rift.IsGR) textrift = "killed in GR" + rift.Difficulty + " [" + rift.Timer + "]";
			else textrift = "killed in " + rift.Difficulty.ToUpper() + " Rift";

			// Name
			IFont color = RiftFont;

			// Store line data
			lines.Add(new Tuple<TextLayout, TextLayout, TextLayout, IFont, IBrush>(
				RiftFont.GetTextLayout(time),
				BossFont.GetTextLayout(rift.BossName),
				color.GetTextLayout(textrift),
				color,
				(elapsed.TotalSeconds <= RiftHistoryHighlight ? HighlightBrush : BgBrush)
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