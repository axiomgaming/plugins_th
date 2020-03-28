// PluginEnablerOrDisablerPlugin.cs "$Revision: 1460 $" "$Date: 2019-03-21 15:42:50 +0200 (to, 21 maalis 2019) $"
using SharpDX;
using SharpDX.Direct2D1;
using System.Collections.Generic;
using System.Linq;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.User
{
    // See: https://www.ownedcore.com/forums/diablo-3/turbohud/turbohud-community-plugins/782208-v9-minimal-plugin-theme.html
    //      https://pastebin.com/nggKPP1w

    public class PluginEnablerOrDisablerPlugin : BasePlugin, ICustomizer
    {
        const bool YES = true;
        const bool NOO = false;

        public PluginEnablerOrDisablerPlugin() { Enabled = true; Order = 10; }  // Notice plugin order, place your own plugin customizations using higder order number!

        public void Customize()
        {
            SetupPlugins();                                             // Choose which default plugins to use.
            CustomizeDefault();                                         // Customize default plugins.
            CustomizeSpecial();                                         // Customize any third party plugins you have.
        }

        void SetupPlugins()
        {
            Hud.TogglePlugin<BountyTablePlugin>(YES);                   // F6 for bountry summary table
            Hud.TogglePlugin<DamageBonusPlugin>(NOO);
            Hud.TogglePlugin<DebugPlugin>(NOO);
            Hud.TogglePlugin<ExperienceOverBarPlugin>(NOO);             // Skillbar experience gained and pool bonus
            Hud.TogglePlugin<GameInfoPlugin>(YES);
            Hud.TogglePlugin<NetworkLatencyPlugin>(YES);
            Hud.TogglePlugin<NotifyAtRiftPercentagePlugin>(NOO);
            Hud.TogglePlugin<ParagonCapturePlugin>(NOO);
            Hud.TogglePlugin<PortraitBottomStatsPlugin>(NOO);
            Hud.TogglePlugin<ResourceOverGlobePlugin>(YES);             // Skillbar health and player class specifc resource numbers
            Hud.TogglePlugin<RiftPlugin>(YES);
            Hud.TogglePlugin<WaypointQuestsPlugin>(YES);                // Act Map normal/special bounty names and completion status
            // Actors
            Hud.TogglePlugin<ChestPlugin>(YES);                         // Minimap chest marker
            Hud.TogglePlugin<ClickableChestGizmoPlugin>(NOO);           // 
            Hud.TogglePlugin<CursedEventPlugin>(YES);                   // Minimap cursed event chest marfker
            Hud.TogglePlugin<DeadBodyPlugin>(NOO);                      // 
            Hud.TogglePlugin<GlobePlugin>(YES);                         // Minimap rift progression orbs and power globes
            Hud.TogglePlugin<OculusPlugin>(NOO);                        // 
            Hud.TogglePlugin<PortalPlugin>(YES);                        // Minimap portals to other levels
            Hud.TogglePlugin<RackPlugin>(YES);                          // 
            Hud.TogglePlugin<ShrinePlugin>(YES);                        // Minimap shrine and well markers
            // BuffLists
            Hud.TogglePlugin<CheatDeathBuffFeederPlugin>(NOO);          // Minimap shows red transpared overlay when your passive "cheat death" skill
            Hud.TogglePlugin<ConventionOfElementsBuffListPlugin>(YES);  // Player portrait shows Convention Of Element cycle/state for all players
            Hud.TogglePlugin<MiniMapLeftBuffListPlugin>(YES);           // Minimap left side shows rift pylon effects (vertically)
            Hud.TogglePlugin<MiniMapRightBuffListPlugin>(NOO);          // Minimap right side passive "cheat death" skill
            Hud.TogglePlugin<PlayerBottomBuffListPlugin>(YES);          // Player bottom: Taeguk, Focus and Restraint
            Hud.TogglePlugin<PlayerLeftBuffListPlugin>(NOO);            // Player left side: customizable by you
            Hud.TogglePlugin<PlayerRightBuffListPlugin>(NOO);           // Player right side: customizable by you
            Hud.TogglePlugin<PlayerTopBuffListPlugin>(YES);             // Player top: customizable by you
            Hud.TogglePlugin<TopLeftBuffListPlugin>(NOO);               // Screen top left: customizable by you
            Hud.TogglePlugin<TopRightBuffListPlugin>(NOO);              // Screen top right: customizable by you
            // CooldownSoundPlayer
            Hud.TogglePlugin<CooldownSoundPlayerPlugin>(YES);
            // Decorators
            Hud.TogglePlugin<GroundLabelDecoratorPainterPlugin>(true);  // Required for all other plugin decorators to work!
            // Inventory
            Hud.TogglePlugin<BloodShardPlugin>(YES);                    // Skillbar blood shard label
            Hud.TogglePlugin<InventoryAndStashPlugin>(YES);             // Draw "A" or "P" marker on items, item greying and cubing is customized here
            Hud.TogglePlugin<InventoryFreeSpacePlugin>(YES);            // 
            Hud.TogglePlugin<InventoryKanaiCubedItemsPlugin>(YES);      // Inventory header cubed item icon and mouse hover description
            Hud.TogglePlugin<InventoryMaterialCountPlugin>(YES);        // Inventory bottom material counts
            Hud.TogglePlugin<StashPreviewPlugin>(YES);                  // 
            Hud.TogglePlugin<StashUsedSpacePlugin>(YES);                // 
            // Items
            Hud.TogglePlugin<CosmeticItemsPlugin>(YES);                 // Minimap cosmetic item markers
            Hud.TogglePlugin<HoveredItemInfoPlugin>(YES);               // 
            Hud.TogglePlugin<ItemsPlugin>(YES);                         // Minimap markers for ancient and primal items
            Hud.TogglePlugin<PickupRangePlugin>(NOO);                   // Show pickup range under player feet
            // LabelLists
            Hud.TogglePlugin<AttributeLabelListPlugin>(NOO);            // Skillbar miscellaneous info about skills and player attributes.
            Hud.TogglePlugin<TopExperienceStatistics>(NOO);             // Paragon point statistics middle top of the screen (mouse hover to see more)
            // Minimap
            Hud.TogglePlugin<MarkerPlugin>(YES);                        // Minimap POI markers like bounty or keywarden names
            Hud.TogglePlugin<SceneHintPlugin>(NOO);                     // ???
            // Monsters
            Hud.TogglePlugin<DangerousMonsterPlugin>(NOO);              // Minimap Draws small circle red and name over named dangerous monsters. See CustomizeDefault().
            Hud.TogglePlugin<EliteMonsterAffixPlugin>(YES);             // 
            Hud.TogglePlugin<EliteMonsterSkillPlugin>(YES);             // 
            Hud.TogglePlugin<ExplosiveMonsterPlugin>(NOO);              // 
            Hud.TogglePlugin<GoblinPlugin>(YES);                        // Minimap goblin markers
            Hud.TogglePlugin<MonsterPackPlugin>(NOO);                   // 
            Hud.TogglePlugin<MonsterRiftProgressionColoringPlugin>(YES);// Minimap rift progression monster colors
            Hud.TogglePlugin<StandardMonsterPlugin>(YES);               // Minimap  monster colors
            Hud.TogglePlugin<TopMonsterHealthBarPlugin>(YES);           // 
            // Players
            Hud.TogglePlugin<BannerPlugin>(NOO);                        // 
            Hud.TogglePlugin<HeadStonePlugin>(NOO);                     // 
            Hud.TogglePlugin<MultiplayerExperienceRangePlugin>(NOO);    // 
            Hud.TogglePlugin<OtherPlayersPlugin>(YES);                  // 
            Hud.TogglePlugin<PlayerSkillPlugin>(YES);                   // Minimap player castable "minion" markers (hydra, sentry, black hole etc.)
            Hud.TogglePlugin<SkillRangeHelperPlugin>(NOO);              // 
            // SkillBars
            Hud.TogglePlugin<OriginalHealthPotionSkillPlugin>(YES);     // Skillbar health potion cooldown timer
            Hud.TogglePlugin<OriginalSkillBarPlugin>(YES);              // Skillbar player skill cooldowns etc.
            Hud.TogglePlugin<UiHiddenPlayerSkillBarPlugin>(NOO);	    // 
        }

        void CustomizeSpecial()
        {
            // Add your own plugin customization code here so it is easy to keep separated from plugin's original code.
        }

        void CustomizeDefault()
        { 
            
			// Hud.RunOnPlugin<glq.GLQ_PlayerSkillBarPlugin>(plugin => {
            // plugin.AllSkill = false;
            // plugin.AddNames("134872");  
        // });
			
			Hud.RunOnPlugin<Gigi.PartyBuffPlugin>(plugin =>
		{
			ISnoPower[] onWiz = {
				Hud.Sno.SnoPowers.Wizard_Archon,
							};
					plugin.DisplayOnClassExceptMe(HeroClass.Wizard, onWiz);

		}); 
			
			
			
			// Bottom skillbar panel customization.

            // Hide DPS labels that are over normal skill "key" labels.
            Hud.RunOnPlugin<OriginalSkillBarPlugin>(plugin =>
            {
                plugin.SkillPainter.EnableSkillDpsBar = false;
                plugin.SkillPainter.EnableDetailedDpsHint = false;
            });

            // Inventory and Stash customization.

            // Disable item graying and cube animation.
            Hud.RunOnPlugin<InventoryAndStashPlugin>(plugin =>
            {
                plugin.LooksGoodDisplayEnabled = false;
                plugin.NotGoodDisplayEnabled = false;
                plugin.DefinitelyBadDisplayEnabled = false;
                plugin.CanCubedEnabled = false;
            });

            // Minimap customization.

            Hud.RunOnPlugin<DangerousMonsterPlugin>(plugin =>   // If you enabled this, here are my customizations!
            {
                plugin.Order = 501;                             // Draw after MyMonsterColoring.
                plugin.AddNames("Enslaved Nightmare");          // "Terror Demon" is included already.
                foreach (var decorator in plugin.Decorator.GetDecorators<MapShapeDecorator>())
                {
                    decorator.Radius = 3;                       // Increase radius for better visibility.
                }
            });
            // Show only Normal and Resplendent chests - hide others.
            Hud.RunOnPlugin<ChestPlugin>(plugin =>
            {
                plugin.LoreChestDecorator.Decorators.Clear();
            });

            // Worldmap and Minimap customization.

            // Make Pickup Range indicator more "prominent" but disable it. Some builds might find this useful, though.
            Hud.RunOnPlugin<PickupRangePlugin>(plugin =>
            {
                plugin.Enabled = false;
                plugin.FillBrush = Hud.Render.CreateBrush(6, 255, 255, 255, 0);
                plugin.OutlineBrush = Hud.Render.CreateBrush(24, 0, 0, 0, 3);

            });
            // Show only ancient and primal data, hide item labels.
            Hud.RunOnPlugin<ItemsPlugin>(plugin =>
            {
                plugin.EnableSpeakPrimal = false;
                plugin.EnableSpeakPrimalSet = false;

                plugin.LegendaryDecorator.Enabled = false;
                plugin.SetDecorator.Enabled = false;
                plugin.InArmorySetDecorator.Enabled = false;

                plugin.AncientDecorator.ToggleDecorators<GroundLabelDecorator>(true);
                plugin.AncientDecorator.Decorators.Add(new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6, 255, 235, 120, 0, true, false, false),
                    RadiusOffset = 14,
                    Up = true,
                });
                plugin.AncientSetDecorator.ToggleDecorators<GroundLabelDecorator>(true);
                plugin.AncientSetDecorator.Decorators.Add(new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6, 255, 0, 170, 0, true, false, false),
                    RadiusOffset = 14,
                    Up = true,
                });
                plugin.PrimalDecorator.ToggleDecorators<GroundLabelDecorator>(true);
                plugin.PrimalDecorator.Decorators.Add(new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 7, 255, 240, 20, 0, true, false, false),
                    RadiusOffset = 14,
                    Up = true,
                });
                plugin.PrimalSetDecorator.ToggleDecorators<GroundLabelDecorator>(true);
                plugin.PrimalSetDecorator.Decorators.Add(new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 7, 255, 240, 20, 0, true, false, false),
                    RadiusOffset = 14,
                    Up = true,
                });
                // Add ground circle for Death Breaths.
                plugin.DeathsBreathDecorator.Add(new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(192, 102, 202, 177, -2),
                    Radius = 1.25f,
                });
            });

            // Change Sentry minimap markers from orange triangle to white plus-sign.
            Hud.RunOnPlugin<PlayerSkillPlugin>(plugin =>
            {
                List<IWorldDecorator> decorators = new List<IWorldDecorator>();
                decorators.AddRange(plugin.SentryDecorator.Decorators);
                decorators.AddRange(plugin.SentryWithCustomEngineeringDecorator.Decorators);
                foreach (var decorator in decorators)
                {
                    if (decorator is MapShapeDecorator)
                    {
                        ((MapShapeDecorator)decorator).Brush = Hud.Render.CreateBrush(255, 255, 255, 255, 2.5f);
                        ((MapShapeDecorator)decorator).ShapePainter = new PlusShapePainter(Hud);
                        ((MapShapeDecorator)decorator).Radius = 4.5f;
                    }
                }
            });
        }
    }
}