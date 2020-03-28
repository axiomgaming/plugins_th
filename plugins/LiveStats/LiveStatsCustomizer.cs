// LiveStats Customizer by HaKache
// Allows you to fully configure and customize LiveStats and its modules.

using SharpDX.DirectInput;
using System.Globalization;
using System.Collections.Generic;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.LiveStats
{
    public class LiveStatsCustomizer : BasePlugin, ICustomizer 
    {

	// Loot Filter : Add item names here if you want to see some non-ancient legendaries with the Map Marker
        public readonly HashSet<string> LootFilter = new HashSet<string>() { "Bovine Bardiche", "Oculus Ring", "The Flavor of Time", "Squirt's Necklace", "Little Rogue", "The Slanderer", "Band of Might", "Leoric's Crown", "The Horadric Hamburger", "Lamentation", "Swamp Land Waders" };

        public LiveStatsCustomizer() : base()
        {
            Enabled = true;
	    Order = -10001; // Load Order - Just before LiveStats Plugins
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }

        public void Customize()
	{

	    // Latency Label Toggler
	    Hud.GetPlugin<LiveStats.Togglers.LatencyPlugin>().IsEnabled = true; // Enable or disable the latency numbers on the game UI latency bar

	    // TopExperienceStats Label Toggler
	    Hud.GetPlugin<LiveStats.Togglers.NextParagonToggler>().IsEnabled = true; // Enable or disable the "ToNextParagon" default label

	    // Inventory Labels Toggler (Bag Free Space & Blood Shards Count)
	    Hud.GetPlugin<LiveStats.Togglers.InventoryToggler>().IsEnabled = true;

	    /////////////////////
	    ///   LiveStats   ///
	    /////////////////////

            Hud.TogglePlugin<LiveStats.LiveStatsPlugin>(true); // Completely disable LiveStats if false

	    // Main Plugin
	    Hud.GetPlugin<LiveStats.LiveStatsPlugin>().HotKey = Key.F7;		// Press this key to show/hide LiveStats 
            Hud.GetPlugin<LiveStats.LiveStatsPlugin>().Show = true;		// True = LiveStats is shown by Default - False = LiveStats is hidden by Default
            Hud.GetPlugin<LiveStats.LiveStatsPlugin>().BagWidget = true;	// True = Enable a Bag Space Widget at the edge of the RunStats bar
            Hud.GetPlugin<LiveStats.LiveStatsPlugin>().LockPinInCombat = false; // True = Disable the ability to pin and unpin a module when you're in combat

	    // Session Module (Built-in Main Plugin)
	    Hud.GetPlugin<LiveStats.LiveStatsPlugin>().SessionHelper = false; // To enable/disable the Session Uptime Module
	    Hud.RunOnPlugin<LiveStats.LiveStatsPlugin>(plugin => { if (plugin.SessionHelper) plugin.Add(plugin.SessionLabel, -1, 0); }); // Label, Priority, Hook

	    // Time Tracker Module (Built-in Main Plugin)
	    Hud.GetPlugin<LiveStats.LiveStatsPlugin>().TimeTracker = true; // Time Stats Module from the main plugin
	    Hud.RunOnPlugin<LiveStats.LiveStatsPlugin>(plugin => { if (plugin.TimeTracker) plugin.Add(plugin.UptimeLabel, -1, 1); });	// Label, Priority, Hook


	    // RunStats can contains up to 7 modules (+ the bag widget) for optimal display (1920x1080 resolution).
	    // Nonetheless, the whole label width and positioning will auto-adapt if you have more than 7 modules.
	    // TopStats can contains 8 to 10 modules for optimal display (1920x1080 resolution).

	    ///////////////////
	    ///   Modules   ///
	    ///////////////////

	    // Damage Meter
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_DamageMeter>(true);
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_DamageMeter>().HideGraphInCombat = false;		// Hide the full Graph when you're in combat
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_DamageMeter>().HideTooltipInCombat = false;	// Hide only the Graph Tooltips when you're in combat
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_DamageMeter>().GraphDuration = 30; 		// In Seconds

	    // Health Meter
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_HealthMonitor>(false);
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_HealthMonitor>().HideGraphInCombat = false;	// Hide the full Graph when you're in combat
	   Hud.GetPlugin<LiveStats.Modules.LiveStats_HealthMonitor>().HideTooltipInCombat = false;	// Hide only the Graph Tooltips when you're in combat
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_HealthMonitor>().GraphDuration = 60; 		// In Seconds
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_HealthMonitor>().EnableProcTracker = false; 	// Enable or disable the ProcTracker Label Color Change function

	    // Latency Meter
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_LatencyMeter>(true);
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LatencyMeter>().HideGraphInCombat = false;	// Hide the full Graph when you're in combat
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LatencyMeter>().HideTooltipInCombat = false;	// Hide only the Graph Tooltips when you're in combat
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LatencyMeter>().GraphDuration = 120;		// In Seconds
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LatencyMeter>().MediumLimit = 80;	// Menu Font changes to Orange if Latency is equal to this value or higher
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LatencyMeter>().HighLimit = 150;	// Menu Font changes to Red if Latency is equal to this value or higher

	    // Blood Shards Helper
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_BloodShardHelper>(false);
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_BloodShardHelper>().UsePercent = false;		// Use % filled cap instead of remaining shards before cap to show Warning
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_BloodShardHelper>().RemainingTreshold = 300;	// Remaining Blood Shards before cap to show Warning
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_BloodShardHelper>().PercentTreshold = 67;		// % Filled Blood Shards cap to show Warning

	    // Keystones & DBs Helper
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_KeystoneHelper>(false);
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_DeathBreathHelper>(false);

	    // Buffs Uptime Helper
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_UptimeHelper>(true);
	    // To disable uptime rules you dont want, go to Modules/LiveStats_UptimeHelper.cs and change the IsEnabled value of the rules you dont want to False.

	    // Loot Helper 
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_LootHelper>(true);
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LootHelper>().MapMarker = true;		// Enable or disable the Map Marker function
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LootHelper>().MapMarkerTreshold = 130;	// Range at which the Map Marker is enabled (130 = ~HUD detection range, 0 = Always active)
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LootHelper>().LootHistoryTimeout = 0;	// Stop showing old drops after this many seconds (0 = Never time out)
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LootHelper>().LootHistoryHighlight = 20;	// Highlight if the drop is new for this many seconds
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LootHelper>().LootHistoryShown = 12; 	// Max # of items to show for loot history (0 = No history)

	    // Loot Helper LootFilter Customization
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LootHelper>().FilterLoot = (item) => item.Quality == ItemQuality.Legendary; // The history will show all legendaries 
	    // The map marker will only show ancients/primals legendaries or any legendary whose name is in the list at top of this file
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LootHelper>().FilterMap = (item) => item.Quality == ItemQuality.Legendary && (item.AncientRank > 0 || LootFilter.Contains(item.SnoItem.NameLocalized));

	    // Bounties Helper
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_BountiesHelper>(false);
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_BountiesHelper>().BountyHistoryTimeout = 0; 	// Stop showing old events after this many seconds (0 = Never time out)
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_BountiesHelper>().BountyHistoryHighlight = 20;	// Highlight if the event is new for this many seconds
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_BountiesHelper>().BountyHistoryShown = 6;		// Max # of events to show for event log (0 = No history)

	    // Bag Space Helper
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_BagSpaceHelper>(false);

	    // XP Stats Helper
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_XPHelper>(false);

	    // XP Stats Helper Expand Labels Customization Example
            Hud.RunOnPlugin<LiveStats.Modules.LiveStats_XPHelper>(plugin =>
            {
                // Remove the 3 first lines :
                // plugin.Label.ExpandUpLabels.RemoveAt(0); plugin.Label.ExpandUpLabels.RemoveAt(0); plugin.Label.ExpandUpLabels.RemoveAt(0);

                // Select Bonus Pool line & Change line content to show Pools % and value.
                var bonusPools = plugin.Label.ExpandUpLabels[plugin.Label.ExpandUpLabels.Count - 1];
                bonusPools.TextFunc = () => Hud.Game.Me.ParagonExpToNextLevel > 0 ?
                    string.Format(CultureInfo.InvariantCulture, "{0:0.0}% ({1})", Hud.Game.Me.BonusPoolRemaining / (double)Hud.Game.Me.ParagonExpToNextLevel * 100d, ValueToString(Hud.Game.Me.BonusPoolRemaining, ValueFormat.ShortNumber)) 
                    : "N/A";
                bonusPools.HintFunc = () => "Bonus Pools";
            });

	    // Materials & Killed Monsters Trackers
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_MaterialTracker>(false);
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_MonsterTracker>(false);

	    // Deaths Logger
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_DeathLogger>(true);
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_DeathLogger>().DeadHistoryTimeout = 0;	// Stop showing old deaths after this many seconds (0 = Never time out)
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_DeathLogger>().DeadHistoryHighlight = 15;	// Highlight if the death is new for this many seconds
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_DeathLogger>().DeadHistoryShown = 10;	// Max # of deaths to show for death history (0 = No history)

	    // Cleared Rifts Tracker
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_RiftTracker>(true);
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_RiftTracker>().RiftHistoryTimeout = 0;	// Stop showing old rift clears after this many seconds (0 = Never time out)
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_RiftTracker>().RiftHistoryHighlight = 20;	// Highlight if the cleared rift is new for this many seconds
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_RiftTracker>().RiftHistoryShown = 12;	// Max # of cleared rifts to show for rift history (0 = No history)

	    // XP Tracker
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_XPTracker>(true);
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_XPTracker>().MapMarker = true;		// Enable or disable the Pool MapMarker function
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_XPTracker>().EnablePoolTracker = true;	// Enable or disable the PoolTracker List function

	    // Ubers Helper
            Hud.TogglePlugin<LiveStats.Modules.LiveStats_UbersHelper>(false);
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_UbersHelper>().UberHistoryTimeout = 120;	// Stop showing old uber kills after this many seconds (0 = never time out)
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_UbersHelper>().UberHistoryHighlight = 10;	// Highlight if the uber kill is new for this many seconds
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_UbersHelper>().UberHistoryShown = 10;	// Max # of uber kills to show for uber history (0 = No history)

        }

    }
}