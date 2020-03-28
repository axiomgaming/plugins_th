// RunStats Buffs Uptime Helper by Razor
// Added Flying Dragon Uptime by S4000
// Added F&R Uptime, Squirt 5+ stacks Uptime & Archon Uptime by HaKache

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
    public class LiveStats_UptimeHelper : BasePlugin, IAfterCollectHandler, ICustomizer //, IKeyEventHandler //, IInGameTopPainter
    {
        public string TextUptime { get; set; } = "Uptime";
        public int OculusValidDistanceFromTarget { get; set; } = 40; // Skeletal Mage reach is about 40 yards, oculus radius is about 10 yards

        public IFont TextFont { get; set; }
        public IBrush BgBrush { get; set; }
        public IBrush BgBrushAlt { get; set; }

        public class UptimeRule
        {
            // Rules
            public string Name { get; set; }		// Name of the rule (For later use ?)
            public bool IsEnabled { get; set; }	= true;	// Is this rule enabled?
            public Func<bool> IsRelevant { get; set; }	// Does this tracked stat apply to the current hero?
            public Func<bool> IsUp { get; set; }	// Is this stat up?
            public Func<bool> IsWatching { get; set; }	// What is the context for the uptime? (e.g. in a rift + in combat)
            public string Description { get; set; }

            // Track and display
            public IWatch Uptime { get; set; } // Duration for which IsUp returns true
            public IWatch TotalTime { get; set; } // Duration for which IsWatching returns true
            public TopLabelDecorator Label { get; set; } // Display Uptime / TotalTime
            public IBrush BgBrush { get; set; } // Optional
            public IFont Font { get; set; } // Optional
            
            public UptimeRule() {}
            
            public decimal Percent() {
                return (TotalTime.ElapsedMilliseconds > 0 ?
                            ((decimal)Uptime.ElapsedMilliseconds/(decimal)TotalTime.ElapsedMilliseconds) : 0);
            }
        }
        public List<UptimeRule> Watching { get; set; }
        
        public TopLabelDecorator Label { get; set; }
        public List<TopLabelDecorator> ExpandUpLabels { get; set; }

        public int Priority { get; set; } = 8;
        public int Hook { get; set; } = 0;
        
        public LiveStats_UptimeHelper()
        {
            Enabled = true;
        }
        
        public void Customize()
        {
            // Add this display to the LiveStats readout with a(n optional) specified positional order priority of 2
            Hud.RunOnPlugin<LiveStatsPlugin>(plugin => {
                plugin.Add(this.Label, this.Priority, this.Hook);
            });
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            
            TextFont = Hud.Render.CreateFont("tahoma", 7, 200, 255, 255, 255, false, false, true);
            
            var plugin = Hud.GetPlugin<LiveStatsPlugin>();
            BgBrush = plugin.BgBrush;
            BgBrushAlt = plugin.BgBrushAlt;
            
            Watching = new List<UptimeRule>();

	    // You can sort these rules by priority. The first relevant rule for your character will be the one shown in the non-expended menu label.

	    // Track Flying Dragon uptime
	    Watching.Add(new UptimeRule()
	    {
		Name = "FD",
		IsEnabled = true,
		IsRelevant = () => (Hud.Game.Me.CubeSnoItem1?.Sno == Hud.Sno.SnoItems.Unique_CombatStaff_2H_009_x1.Sno), // FD is cubed
		IsUp = () => IsInRift() && Hud.Game.Me.InCombat && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.FlyingDragon.Sno, 1),
		IsWatching = () => IsInRift() && Hud.Game.Me.InCombat,
		Description = "Flying Dragon " + TextUptime,
		Uptime = Hud.Time.CreateWatch(),
		TotalTime = Hud.Time.CreateWatch(),
		//BgBrush = Hud.Render.CreateBrush(200, 81, 78, 72, 0),
		Font = Hud.Render.CreateFont("tahoma", 7, 255, 215, 190, 100, false, false, true),
	    });
            
            // Track Simulacrum uptime
            Watching.Add(new UptimeRule()
            {
		Name = "Simulacrum",
		IsEnabled = false,
                IsRelevant = () => Hud.Game.Me.Powers.UsedSkills.Any(s => s.SnoPower.Sno == Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno), // Simulacrum Skill is on the action bar
                IsUp = () => IsInRift() && Hud.Game.Me.InCombat && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno, 1),
                IsWatching = () => IsInRift() && Hud.Game.Me.InCombat,
                Description = Hud.Sno.SnoPowers.Necromancer_Simulacrum.NameLocalized + " " + TextUptime,
                Uptime = Hud.Time.CreateWatch(),
                TotalTime = Hud.Time.CreateWatch(),
                //BgBrush = Hud.Render.CreateBrush(200, 107, 3, 3, 0),
                Font = Hud.Render.CreateFont("tahoma", 7, 255, 255, 60, 60, false, false, true),
            });

            // Track Gruesome Feast 4+ Stacks uptime
            Watching.Add(new UptimeRule()
            {
		Name = "Gruesome",
		IsEnabled = false,
                IsRelevant = () => Hud.Game.Me.Powers.UsedPassives.Any(s => s.Sno == Hud.Sno.SnoPowers.WitchDoctor_Passive_GruesomeFeast.Sno), // Gruesome Feast is an active passive (Doesnt account for Hellfire...)
                IsUp = () => IsInRift() && Hud.Game.Me.InCombat && Hud.Game.Me.GetAttributeValue(Hud.Sno.Attributes.Buff_Icon_Count1, 208594) == 4, // At least 4 stacks of Gruesome Feast
                IsWatching = () => IsInRift() && Hud.Game.Me.InCombat,
                Description = Hud.Sno.SnoPowers.WitchDoctor_Passive_GruesomeFeast.NameLocalized + " " + TextUptime,
                Uptime = Hud.Time.CreateWatch(),
                TotalTime = Hud.Time.CreateWatch(),
                Font = Hud.Render.CreateFont("tahoma", 7, 255, 255, 0, 50, false, false, true),
            });

            // Track Focus + Restraint uptime
            // Note: IsRelevant => Checking for buffs is inconsistent - they sometimes drop off for a moment and then are reapplied (might be due to latency or packet loss), this function has to be consistent because it resets timers when it returns false
            Watching.Add(new UptimeRule()
            {
		Name = "FnR",
		IsEnabled = false,
                IsRelevant = () => Hud.Game.Items.Any(x => (x.Location == ItemLocation.LeftRing || x.Location == ItemLocation.RightRing) && x.SnoItem.Sno == Hud.Sno.SnoItems.Unique_Ring_Set_001_x1.Sno) && Hud.Game.Items.Any(x => (x.Location == ItemLocation.LeftRing || x.Location == ItemLocation.RightRing) && x.SnoItem.Sno == Hud.Sno.SnoItems.Unique_Ring_Set_002_x1.Sno), // Unique_Ring_Set_001_x1 (Focus) - Unique_Ring_Set_002_x1 (Restraint)
                IsUp = () => IsInRift() && Hud.Game.Me.InCombat && Hud.Game.Me.Powers.BuffIsActive(359583, 2) && Hud.Game.Me.Powers.BuffIsActive(359583, 1),
                IsWatching = () => IsInRift() && Hud.Game.Me.InCombat,
                Description = "Focus & Restraint " + TextUptime,
                Uptime = Hud.Time.CreateWatch(),
                TotalTime = Hud.Time.CreateWatch(),
                Font = Hud.Render.CreateFont("tahoma", 7, 255, 255, 120, 20, false, false, true),
            });

            // Track Squirt >= 5 stacks uptime
            Watching.Add(new UptimeRule()
            {
		Name = "Squirt",
		IsEnabled = true,
                IsRelevant = () => (Hud.Game.Me.CubeSnoItem3?.Sno == 1187653737 || Hud.Game.Items.Any(x => x.Location == ItemLocation.Neck && x.SnoItem.Sno == 1187653737)), // Hud.Sno.SnoItems.Unique_Amulet_010_x1.Sno
                IsUp = () => IsInRift() && Hud.Game.Me.InCombat && Hud.Game.Me.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_5_Visual_Effect_None, Hud.Sno.SnoPowers.SquirtsNecklace.Sno) == 1, // 5 stacks of Squirt - Powers.GetBuff(Hud.Sno.SnoPowers.SquirtsNecklace.Sno).IconCounts[5]
                IsWatching = () => IsInRift() && Hud.Game.Me.InCombat,
                Description = "5+ Stacks Squirt " + TextUptime,
                Uptime = Hud.Time.CreateWatch(),
                TotalTime = Hud.Time.CreateWatch(),
                Font = Hud.Render.CreateFont("tahoma", 7, 255, 255, 200, 30, false, false, true),
            });

            // Track Archon uptime
            Watching.Add(new UptimeRule()
            {
		Name = "Archon",
		IsEnabled = false,
                IsRelevant = () => Hud.Game.Me.Powers.UsedSkills.Any(s => s.SnoPower.Sno == Hud.Sno.SnoPowers.Wizard_Archon.Sno), // Archon Skill is on the action bar
                IsUp = () => IsInRift() && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Wizard_Archon.Sno, 2), // We watch uptime even if not in combat
                IsWatching = () => IsInRift(),
                Description = Hud.Sno.SnoPowers.Wizard_Archon.NameLocalized + " " + TextUptime,
                Uptime = Hud.Time.CreateWatch(),
                TotalTime = Hud.Time.CreateWatch(),
                Font = Hud.Render.CreateFont("tahoma", 7, 255, 180, 76, 216, false, false, true),
            });

            // Track Hexing Pants uptime
            // Note: IsRelevant => Checking for buffs is inconsistent - they sometimes drop off for a moment and then are reapplied (might be due to latency or packet loss), this function has to be consistent because it resets timers when it returns false
            Watching.Add(new UptimeRule()
            {
		Name = "Hexing",
		IsEnabled = false,
                IsRelevant = () => (Hud.Game.Me.CubeSnoItem2?.Sno == Hud.Sno.SnoItems.Unique_Pants_101_x1.Sno) || Hud.Game.Items.Any(x => x.Location == ItemLocation.Legs && x.SnoItem.Sno == Hud.Sno.SnoItems.Unique_Pants_101_x1.Sno), // Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.HexingPantsOfMrYan.Sno), // Hexing pants is equipped/cubed
                IsUp = () => IsInRift() && Hud.Game.Me.InCombat && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.HexingPantsOfMrYan.Sno, 2),
                IsWatching = () => IsInRift() && Hud.Game.Me.InCombat,
                Description = "Hexing Pants " + TextUptime,
                Uptime = Hud.Time.CreateWatch(),
                TotalTime = Hud.Time.CreateWatch(),
                Font = Hud.Render.CreateFont("tahoma", 7, 255, 135, 135, 135, false, false, true),
            });
            
            // Track Dayntee's Buff uptime
            // Note: IsRelevant => Checking for buffs is inconsistent - they sometimes drop off for a moment and then are reapplied (might be due to latency or packet loss), this function has to be consistent because it resets timers when it returns false
            Watching.Add(new UptimeRule()
            {
		Name = "Dayntee",
		IsEnabled = false,
                IsRelevant = () => (Hud.Game.Me.CubeSnoItem2?.Sno == Hud.Sno.SnoItems.P61_Unique_Belt_01.Sno) || Hud.Game.Items.Any(x => x.Location == ItemLocation.Waist && x.SnoItem.Sno == Hud.Sno.SnoItems.P61_Unique_Belt_01.Sno), // Dayntee's is cubed or equipped
                IsUp = () => IsInRift() && Hud.Game.Me.InCombat && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.DaynteesBinding.Sno, 1),
                IsWatching = () => IsInRift() && Hud.Game.Me.InCombat,
                Description = "Dayntee's " + TextUptime,
                Uptime = Hud.Time.CreateWatch(),
                TotalTime = Hud.Time.CreateWatch(),
                //BgBrush = Hud.Render.CreateBrush(200, 81, 78, 72, 0),
                Font = Hud.Render.CreateFont("tahoma", 7, 255, 255, 251, 209, false, false, true),
            });
            
            // Track IP uptime
            Watching.Add(new UptimeRule()
            {
		Name = "IP",
		IsEnabled = true,
                IsRelevant = () => Hud.Game.Players.Any(p => p.HeroClassDefinition.HeroClass == HeroClass.Barbarian && p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.Barbarian_IgnorePain.Sno)), //someone in the party has IP equipped
                IsUp = () => 
                    IsInRift() && 
                    Hud.Game.Me.InCombat && 
                    Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Barbarian_IgnorePain.Sno) && 
                    !Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorInvulBuff.Sno) && //not in stasis
                    !Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInvulnerable.Sno), //no Shield Pylon
                IsWatching = () => 
                    IsInRift() && 
                    Hud.Game.Me.InCombat && 
                    !Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorInvulBuff.Sno) &&  //not in stasis
                    !Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInvulnerable.Sno), //no Shield Pylon
                Description = Hud.Sno.SnoPowers.Barbarian_IgnorePain.NameLocalized + " " + TextUptime,
                Uptime = Hud.Time.CreateWatch(),
                TotalTime = Hud.Time.CreateWatch(),
                //BgBrush = Hud.Render.CreateBrush(200, 9, 68, 34, 0),
                Font = Hud.Render.CreateFont("tahoma", 7, 255, 18, 234, 110, false, false, true),
            });

            // Track Oculus Buff uptime
            Watching.Add(new UptimeRule()
            {
		Name = "Oculus",
		IsEnabled = true,
                IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.BuffIsActive(Hud.Sno.SnoPowers.OculusRing.Sno)), // Someone in the party (or active follower) has oculus ring equipped/cubed
                IsUp = () => 
                    IsInRift() && 
                    Hud.Game.Me.InCombat && 
                    Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.OculusRing.Sno, 2) &&
                    Hud.Game.AliveMonsters.Any(m => m.IsElite && m.Rarity != ActorRarity.RareMinion && m.CentralXyDistanceToMe < OculusValidDistanceFromTarget), //player.FloorCoordinate.XYZDistanceTo(m.FloorCoordinate)
                IsWatching = () => {
                    if (!IsInRift()) return false;
                    if (!Hud.Game.Me.InCombat) return false;					
                    var circles = Hud.Game.Actors.Where(a => a.SnoActor.Sno == ActorSnoEnum._generic_proxy && a.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, Hud.Sno.SnoPowers.OculusRing.Sno) == 1);
                    return circles.Any(c => Hud.Game.AliveMonsters.Any(m => m.IsElite && m.Rarity != ActorRarity.RareMinion && c.FloorCoordinate.XYZDistanceTo(m.FloorCoordinate) < OculusValidDistanceFromTarget+10));
                },
                Description = "Oculus " + TextUptime,
                Uptime = Hud.Time.CreateWatch(),
                TotalTime = Hud.Time.CreateWatch(),
                //BgBrush = Hud.Render.CreateBrush(200, 76, 79, 7, 0),
                Font = Hud.Render.CreateFont("tahoma", 7, 255, 158, 255, 100, false, false, true), //255, 253, 229
            });
            
            // Create all the uptime hover labels based on what is being watched
            ExpandUpLabels = new List<TopLabelDecorator>();
            foreach (UptimeRule rule in Watching.Where(r => r.IsEnabled)) // IsEnabled ?
            {
                TopLabelDecorator label = new TopLabelDecorator(Hud) {
                    TextFont = (rule.Font == null ? TextFont : rule.Font),
                    BackgroundBrush = (rule.BgBrush == null ? BgBrush : rule.BgBrush),
                    ExpandedHintFont = TextFont,
                    ExpandedHintWidthMultiplier = (float)((Hud.Window.Size.Width / 1000)* 2.0), // 1.5, 1.75 ?
                    TextFunc = () => string.Format("{0}/{1}", 
                        ValueToString(rule.Uptime.ElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime),
                        ValueToString(rule.TotalTime.ElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime)
                    ),
                    HintFunc = () =>
                    {
                        var pct = rule.Percent() * 100;
                        return string.Format("({0}%) {1}", pct.ToString((pct == 0 ? "F0" : "F1"), CultureInfo.InvariantCulture), rule.Description);
                    },
                };
                
                rule.Label = label;
                ExpandUpLabels.Add(label);
            }
            
            // Create the main menu label based on what is being watched
            Label = new TopLabelDecorator(Hud)
            {
                TextFont = TextFont,
                //BackgroundBrush = BgBrush,
                TextFunc = () => {
                    var firstActiveRule = Watching.FirstOrDefault(r => r.IsRelevant());
                    if (firstActiveRule != null)
                    {
                        var pct = firstActiveRule.Percent() * 100;
                        return string.Format("{0}% {1}", pct.ToString((pct == 0 ? "F0" : "F1"), CultureInfo.InvariantCulture), TextUptime.ToLower());
                    }
                    
                    return string.Empty;
                }, //ValueToString(Hud.Game.Me.Defense.EhpCur, ValueFormat.ShortNumber),
                HintFunc = () => "Uptime",
                ExpandUpLabels = this.ExpandUpLabels,
            };
        }
        
        public void AfterCollect()
        {
            if (!Hud.Game.IsInGame) return;
            
            // Calculate uptimes
            bool isAnyRelevant = false;
            int alternate = 0;
            foreach (UptimeRule rule in Watching.Where(r => r.IsEnabled)) // IsEnabled ?
            {
                // Show or hide labels based on whether or not it is relevant to the current hero
                if (!rule.IsRelevant())
                {
                    rule.Label.Enabled = false;
                    rule.Uptime.Stop();
                    rule.Uptime.Reset();
                    rule.TotalTime.Stop();
                    rule.TotalTime.Reset();
                    continue;
                }
                rule.Label.Enabled = true;
                rule.Label.BackgroundBrush = (alternate++ % 2 == 0 ? BgBrush : BgBrushAlt);
                isAnyRelevant = true;
                
                // Update uptime
                if (rule.IsUp() && !Hud.Game.IsPaused) // We check if game is not paused
                {
                    if (!rule.Uptime.IsRunning) 
                        rule.Uptime.Start();
                }
                else
                    rule.Uptime.Stop();

                // Update total time
                if (rule.IsWatching() && !Hud.Game.IsPaused) // We check if game is not paused
                {
                    if (!rule.TotalTime.IsRunning) 
                        rule.TotalTime.Start();
                }
                else
                    rule.TotalTime.Stop();
            }
            
            Label.Enabled = isAnyRelevant;
        }
        
        public bool IsInRift()
        {
            return (Hud.Game.SpecialArea == SpecialArea.Rift) || (Hud.Game.SpecialArea == SpecialArea.GreaterRift);
        }
    }
}