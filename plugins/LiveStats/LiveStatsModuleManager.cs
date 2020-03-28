// LiveStats Module Manager by HaKache
// Allows you to configure LiveStats modules priority order and hook bar.

using SharpDX.DirectInput;
using System.Globalization;
using System.Collections.Generic;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.LiveStats
{
    public class LiveStatsModuleManager : BasePlugin, ICustomizer 
    {

        public LiveStatsModuleManager() : base()
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

	    // General Module Priority Order (To change the position of the modules in the bars) - Lower number means the module will appear first

	    // RunStats Modules
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_BagSpaceHelper>().Priority = 0;		// Bag Space Helper
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LootHelper>().Priority = 1;		// Loot Helper
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_BloodShardHelper>().Priority = 2;		// Blood Shard Helper
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_KeystoneHelper>().Priority = 3;		// Keystone Helper
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_DeathBreathHelper>().Priority = 4;	// Death Breath Meter
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_BountiesHelper>().Priority = 5;		// Bounties Helper
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_XPHelper>().Priority = 7;			// XP Helper
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_UptimeHelper>().Priority = 8;		// Uptime Helper
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_DamageMeter>().Priority = 20;		// Damage Meter
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_HealthMonitor>().Priority = 21;		// Health Meter
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LatencyMeter>().Priority = 25;		// Latency Meter

	    // TopStats Modules
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_XPTracker>().Priority = 1;	// XP Tracker
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_DeathLogger>().Priority = 2;	// Death Logger
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_MaterialTracker>().Priority = 3;	// Materials Tracker
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_MonsterTracker>().Priority = 4;	// Monsters Tracker
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_RiftTracker>().Priority = 8;	// Rift Tracker
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_UbersHelper>().Priority = 100;	// Ubers Helper


	    // General Module Hook Point (To change in which bar goes each module) - 0 = RunStats / 1+ = TopStats

	    // RunStats Modules
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_BagSpaceHelper>().Hook = 0;	// Bag Space Helper
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LootHelper>().Hook = 0;		// Loot Helper
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_BloodShardHelper>().Hook = 0;	// Blood Shard Helper
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_KeystoneHelper>().Hook = 0;	// Keystone Helper
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_DeathBreathHelper>().Hook = 0;	// Death Breath Meter
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_BountiesHelper>().Hook = 0;	// Bounties Helper
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_XPHelper>().Hook = 0;		// XP Helper
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_UptimeHelper>().Hook = 1;		// Uptime Helper
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_DamageMeter>().Hook = 0;		// Damage Meter
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_HealthMonitor>().Hook = 0;	// Health Meter
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_LatencyMeter>().Hook = 0;		// Latency Meter

	    // TopStats Modules
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_XPTracker>().Hook = 1;		// XP Tracker
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_DeathLogger>().Hook = 1;		// Death Logger
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_MaterialTracker>().Hook = 1;	// Materials Tracker
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_MonsterTracker>().Hook = 1;	// Monsters Tracker
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_RiftTracker>().Hook = 1;		// Rift Tracker
	    Hud.GetPlugin<LiveStats.Modules.LiveStats_UbersHelper>().Hook = 1;		// Ubers Helper

        }

    }
}