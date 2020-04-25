using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Turbo.Plugins.Default;

// @author LuxuryMaster
//
// Last edited: January 4, 2019
//
// ----------- Change log -----------
// v1.1:
//    - Changed the way plugin keeps track of the mobs. Previously, it was keeping track of highest hp of mob within the pack.
//        however, that's not accurate in case the mobs of elite is low and if we are doing computation of mobs spawning from the pylon using nemesis bracer
//		  because we are using highest_hp_mobs as the basis of nemesis elite mobs and packs computation.
//
//        version 1.1 keeps track of highest mob of encountered throughout the rift to more accurately estimate the nemesis spawn
//
//      - Fixed an issue where plugin is appearing in rift guardian then disappears shortly after. Now the plugin isnt displayed on rift guardian.
//
//      - Fixed an issue where plugin % value was showing "NAN" when either the conduit dmg is equal to hp of mobs or when all mobs are dead.
//
//		- When all the players are in the game are "dead", the plugin disappears from the UI
//
//      - Fixed an issue where plugin was showing the value when there were elites and all the elites were dead. Now plugin disappears when there are no more alive elite packs nearby.
//
// ----------------------------------
//
//  To install this plugin: Exit exisint hud (ctrl+ end) if running one then place this file under the following directory path then start your hud:
//             <your_hud_installation_dir>\plugings\Default\LabelLists
//
// This plugin detects the best time to take the conduit. It automatically detects the following:
//
// - The plugin doesn't displays any ui on rift guardian or if there is no elite packs nearby
// - Whether there is a necro using curse (15%) or (18%) curse and adjusts total hp of mobs accordingly
// - Whether the player using plugin is wearing nemesis or not and adjusts total hp of mobs accordingly. If barb is wearing nemesis and
//   monk is not, barb's plugin view will be different than monk's because barb's plugin adds the elite + mobs spawns from the conduit
// - If any player in the team is dead, the plugin adjusts total dps of the conduit accordingly so the team can decide whether to take conduit with dead player or not
// - Conduit dps is different in each gr level. Greater the gr level, the greater dps of the conduit. Plugin computes the best estimate of the conduit
// - The plugin only works in greater rift. For the usage in the rift level, it assumes it's same as greater rift level 60
//
// The layout looks like the following:
// ------------------------------------------------------------------
// | greater_rift_level | offset_%_of_total_mobs_hp_within_40_yards |
// ------------------------------------------------------------------
// - Green light on offset_%: It's safe to take conduit. total hp of mobs is less than team dps of the conduit
// - Yellow light on offset_%: It's getting close to take the conduit. Total hp of the mobs is approximately @cond_yellow_light_threshold of the conduit (18% by default)
// - Red light on offset_%: DO NOT take conduit. total hp of mobs is greater than team dps of the conduit (i.e > @cond_yellow_light_threshold)
//
// - Lastly, to best optimize the performance of the code, It only displays the UI in the following conditions:
//    1) If a player is in greater rift and
//      2) There is a elite pack nearby
//
namespace Turbo.Plugins.Default
{
public class ConduitPylonStatisticsPlugin : BasePlugin, IInGameTopPainter
{
public HorizontalTopLabelList LabelList {
        get; private set;
}

private double highest_mob_hp_in_trillions = 0;

public ConduitPylonStatisticsPlugin()
        : base()
{
        Enabled = true;
}

public override void Load(IController hud)
{
        base.Load(hud);

        LabelList = new HorizontalTopLabelList(hud)
        {
                LeftFunc = () =>
                {
                        return Hud.Window.Size.Width / 2 - Hud.Window.Size.Height * 0.08f - 370; // width / 2 였슴   // -250 였음
                },
                TopFunc = () =>
                {
                        return Hud.Window.Size.Height * 0.001f;
                },
                WidthFunc = () =>
                {
                        return Hud.Window.Size.Height * 0.08f;
                },
                HeightFunc = () =>
                {
                        return Hud.Window.Size.Height * 0.018f;
                }
        };
}

public void PaintTopInGame(ClipState clipState)
{
        if (clipState != ClipState.BeforeClip) return;
        if (Hud.Game.SpecialArea != SpecialArea.GreaterRift) return;

        // If we are encountering rift guardian, do not show the plugin
        int count = Hud.Game.AliveMonsters.Count(m => m.SnoMonster.Priority == MonsterPriority.boss);
        if (count == 1) return;

        var expandedHintFont = Hud.Render.CreateFont("tahoma", 6, 255, 200, 200, 200, false, false, true);

        // Turn on the plugin only if there are any elites nearby
        var packs = Hud.Game.MonsterPacks;
        if (!packs.Any()) return;

        // Make plugin disappear if there are no longer packs alive.
        // This fixes edge case where there were elites and no longer elites alive
        double alive_pack_monsters_count = 0;

        foreach (var pack in packs) {
                alive_pack_monsters_count += pack.MonstersAlive.Count();
        }

        if(alive_pack_monsters_count == 0) {
                return;
        }


        // Drop label (1) : Current greater rift level
        var currentLevelDecorator = new TopLabelDecorator(Hud)
        {
                TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 57, 137, 205, true, false, true),
                BackgroundTexture1 = Hud.Texture.Button2TextureOrange,
                BackgroundTexture2 = Hud.Texture.BackgroundTextureBlue,
                BackgroundTextureOpacity1 = 1.0f,
                BackgroundTextureOpacity2 = 0.5f,
                TextFunc = () => "gr-" + (Hud.Game.SpecialArea == SpecialArea.Rift ? "60" : Hud.Game.Me.InGreaterRiftRank + ""),
                ExpandDownLabels = new List<TopLabelDecorator>(),
        };

        var w = Hud.Window.Size.Height * 0.1f; // 0.1f
        var h = Hud.Window.Size.Height * 0.04f;

        // Configurable variables
        bool assume_mobs_cursed = false; // If we assume the mobs are cursed, program trims down the hp of the mobs by 85% to improve the accurancy of the timing
        bool is_player_wearing_nemesis = false; // If the barb is wearing nemesis, program factors in the highest mob of the hp to factor in the hp of the elite popping out of the conduit
        int num_elite_mobs_spawned_from_nemesis = 3; // On average, pylon spawns minions too and it's very close to the hp of elites * 3
        double cursed_mob_threshold = 0.85; // If mobs are cursed only need to get it down to 85%. In case necro is using early grave, this can be adjusted to 87%

        // Find out if there is a necro in the game and using cursed scycle skill
        foreach(var player in Hud.Game.Players)
        {
                if (player.HeroClassDefinition.HeroClass == HeroClass.Necromancer)
                {
                        foreach(var skill in player.Powers.UsedSkills) {
                                if(skill.RuneNameEnglish == "Cursed Scythe") {
                                        assume_mobs_cursed = true;
                                }

                                if(skill.RuneNameEnglish == "Early Grave") {
                                        cursed_mob_threshold = 0.82;
                                }
                        }
                }
        }

        // Find out if you are wearing nemesis bracers
        var items = Hud.Game.Items.Where(item => item.Location == ItemLocation.Bracers);
        foreach (var item in items)
        {
                if(item.SnoItem.Code == "Unique_Bracer_106_x1") // ISnoItem Unique_Bracer_106_x1 { get; } // 962426467 - Nemesis Bracers
                {
                        is_player_wearing_nemesis = true;
                }
        }

        // Conduit pylon constants
        const int elect_dmg_per_hit_base = 610248;
        const double cond_yellow_light_threshold = 1.18; // 18% from the max_hp_mobs
        const double pylon_dmg_multiplier_min = 0.4;
        const double pylon_dmg_multiplier_max = 0.5;
        const double pylon_multiplier_avg =(pylon_dmg_multiplier_min + pylon_dmg_multiplier_max) / 2;
        const int cond_pylon_duration_seconds = 30;
        const double one_trillion = 1000000000000;

        // Calculate - gr_life_multiplier_in_current_gr: grand total of electrocute damage based on the current greater rift level
        int gr_base_level = 60; // T13 = gr 60
        double gr_life_multiplier_base = 13178; // based on gr level 60
        double gr_life_multiplier_in_current_gr = gr_life_multiplier_base;

        double gr_life_multiplier_increase_per_lv = 1.17; // life multiplier increases 17% per level
        int current_gr_level = (int) Hud.Game.Me.InGreaterRiftRank;

        // Rift is equivilent of greater rift level 60 - TODO: Only enable plugin in GR
        var gr_level_text = (Hud.Game.SpecialArea == SpecialArea.Rift ? 60 : Hud.Game.Me.InGreaterRiftRank) + "";
        var gr_level_title = "greater_rift_level";

        // Get the reference all nearby monsters including trash
        var monsters = Hud.Game.AliveMonsters;

        double totalAliveMonstersHealthSum_in_trillions = 0;
        int totalAliveMonsters = 0;

        var alive = monsters.ToList();

        var center = Hud.Window.CreateWorldCoordinate(0, 0, 0);

        var n = 0;

        // If there are no alive monsters nearby, make plugin disappear
        if(alive.Count == 0) {
                return;
        }

        // Only scan the monsters within the hud screen range - 40 yards
        if (alive.Any(x => x.FloorCoordinate.IsOnScreen()))
        {
                foreach (var monster in alive.Where(x => x.FloorCoordinate.IsOnScreen()))
                {
                        // Change the center positioning of the current cooridnate
                        center.Add(monster.FloorCoordinate);

                        // If we assume mobs are cursed, only add the threshold amount of hp. If not, we add full hp. Both values are trimmed by trillion
                        double on_screen_alive_monster_health_in_trillions = assume_mobs_cursed ? (monster.CurHealth * cursed_mob_threshold)/ one_trillion : monster.CurHealth / one_trillion;
                        totalAliveMonstersHealthSum_in_trillions += on_screen_alive_monster_health_in_trillions;

                        // We keep track of highest hp mob of the monster to factor in the elite and elite minions spawn from the conduit pylon
                        if(on_screen_alive_monster_health_in_trillions > highest_mob_hp_in_trillions) {
                                highest_mob_hp_in_trillions = on_screen_alive_monster_health_in_trillions;
                        }

                        totalAliveMonsters++;
                        n++;
                }
        }
        else
        {
                foreach (var monster in alive)
                {
                        center.Add(monster.FloorCoordinate);

                        // If we assume mobs are cursed, only add the threshold amount of hp. If not, we add full hp. Both values are trimmed by trillion
                        double alive_monster_health_in_trillions = assume_mobs_cursed ? (monster.CurHealth * cursed_mob_threshold)/ one_trillion : monster.CurHealth / one_trillion;
                        totalAliveMonstersHealthSum_in_trillions += alive_monster_health_in_trillions;

                        // We keep track of highest hp mob of the monster to factor in the elite and elite minions spawn from the conduit pylon
                        if(alive_monster_health_in_trillions > highest_mob_hp_in_trillions) {
                                highest_mob_hp_in_trillions = alive_monster_health_in_trillions;
                        }

                        totalAliveMonsters++;
                        n++;
                }
        }

        // If a player is wearing nemesis, factor in the elite coming out from the pylon and add the highest HP of the mob currently to factor in the elite popping out of the conduit pylon
        if(is_player_wearing_nemesis) {
                totalAliveMonstersHealthSum_in_trillions += (highest_mob_hp_in_trillions * num_elite_mobs_spawned_from_nemesis);
        }

        // Adjust the center coordinate dynamically
        center.Set(center.X / n, center.Y / n, center.Z / n);
        var centerScreenCoordinate = center.ToScreenCoordinate(false);
        var y = centerScreenCoordinate.Y;

        // Will use base gr level 60 multiplier in case estimator is used below gr 60. This is calculated compoundly rather than exponentially
        if(Hud.Game.Me.InGreaterRiftRank <= gr_base_level)
        {
                gr_life_multiplier_in_current_gr = 13178;
        }
        else
        {
                for (int i = 0; i < current_gr_level - gr_base_level; i++)
                {
                        gr_life_multiplier_in_current_gr = (double) (gr_life_multiplier_in_current_gr * gr_life_multiplier_increase_per_lv);
                }
        }

        // Electrocute Damage per hit=610248.7×(Min:0.4 - Max:0.5)×GR Life Multiplier
        // For example, in GR level 90 (GR Life Multiplier=1463698.8), the pylon deals 357.3B - 446.6B damage per hit. Gears, buffs, debuffs are irrelevant.
        double elec_total_in_current_gr = (double) (elect_dmg_per_hit_base * pylon_multiplier_avg * gr_life_multiplier_in_current_gr);
        double elec_total_in_current_gr_in_trillions = (double)(elec_total_in_current_gr / one_trillion);

        // Conduit pylon makes 5-10 elect hits per second
        // https://www.diablofans.com/forums/diablo-iii-general-forums/diablo-iii-general-discussion/176376-pylons-mechanics-and-usage#conduit
        double elect_hits_per_second_avg = 7.5;

        // Based on the current number of mobs alive, calculate the elect_hits_per_second_avg
        int elect_hits_per_second = 0;

        // If there are less than 7 mobs alive, use whatever the current count. Otherwise, use avg because conduit can hit only 5-10 mobs simultaneously
        if(totalAliveMonsters <= 7) {
                elect_hits_per_second = totalAliveMonsters;
        }
        else {
                elect_hits_per_second = (int) elect_hits_per_second_avg;
        }

        // Calculate the grand total expected damage of one player during entire conduit cycle
        double total_conduit_dps_per_player_in_trillions = elec_total_in_current_gr_in_trillions * elect_hits_per_second * cond_pylon_duration_seconds;

        // Calculate the dps based on the current players in the game. If any player is dead, make sure expected damage is reflected so barb doesn't take cond when any is dead
        var players = Hud.Game.Players;
        var num_players_alive = 0;
        var num_players_dead = 0;

        foreach(var player in players)
        {
                if(!player.IsDead)
                {
                        num_players_alive++;
                }
                else
                {
                        num_players_dead++;
                }
        }

        // Display: hp_mobs - Calculate the total hps of mobs whites and elites
        double total_conduit_dps_team_in_trillions = total_conduit_dps_per_player_in_trillions * num_players_alive;
        var totalAliveMonstersHealthSum_in_trillions_truncated = Math.Truncate(totalAliveMonstersHealthSum_in_trillions * 100) / 100;
        var hp_mobs_combined_text = totalAliveMonstersHealthSum_in_trillions_truncated + " (" + totalAliveMonsters + ")";
        var hp_mobs_combined_title = "hp_mobs(t)";

        // Display: total_conduit_dps - Calculate the grand total expected damage of entire team damage during entire conduit cycle
        var total_conduit_dps_team_in_trillions_truncated = Math.Truncate(total_conduit_dps_team_in_trillions * 100) / 100;
        var expected_cond_dps_text = total_conduit_dps_team_in_trillions_truncated + "";
        var expected_cond_dps_title = "t_cond_dps(t)";

        var num_players_alive_text = num_players_alive + "";
        var num_players_alive_title = "num_p_alive";

        // Drop label (1) : Players alive
        currentLevelDecorator.ExpandDownLabels.Add(
                new TopLabelDecorator(Hud)
                        {
                                TextFont = Hud.Render.CreateFont("tahoma", 6, 180, 255, 255, 255, true, false, true),
                                ExpandedHintFont = expandedHintFont,
                                ExpandedHintWidthMultiplier = 2,
                                BackgroundTexture1 = Hud.Texture.Button2TextureOrange,
                                BackgroundTexture2 = Hud.Texture.BackgroundTextureBlue,
                                BackgroundTextureOpacity1 = 1.0f,
                                BackgroundTextureOpacity2 = 0.5f,
                                HideBackgroundWhenTextIsEmpty = true,
                                TextFunc = () => "p_alive",
                                HintFunc = () => num_players_alive_text,
                        });

        // Drop label (2) : Player wearing nemesis? true or false
        currentLevelDecorator.ExpandDownLabels.Add(
                new TopLabelDecorator(Hud)
                        {
                                TextFont = Hud.Render.CreateFont("tahoma", 6, 180, 255, 255, 255, true, false, true),
                                ExpandedHintFont = expandedHintFont,
                                ExpandedHintWidthMultiplier = 2,
                                BackgroundTexture1 = Hud.Texture.Button2TextureOrange,
                                BackgroundTexture2 = Hud.Texture.BackgroundTextureBlue,
                                BackgroundTextureOpacity1 = 1.0f,
                                BackgroundTextureOpacity2 = 0.5f,
                                HideBackgroundWhenTextIsEmpty = true,
                                TextFunc = () => "p_nem",
                                HintFunc = () => is_player_wearing_nemesis ? "Yes" : "No",
                        });

        // Drop label (3) : Player in the team using curse? Yes / No
        currentLevelDecorator.ExpandDownLabels.Add(
                new TopLabelDecorator(Hud)
                        {
                                TextFont = Hud.Render.CreateFont("tahoma", 6, 180, 255, 255, 255, true, false, true),
                                ExpandedHintFont = expandedHintFont,
                                ExpandedHintWidthMultiplier = 2,
                                BackgroundTexture1 = Hud.Texture.Button2TextureOrange,
                                BackgroundTexture2 = Hud.Texture.BackgroundTextureBlue,
                                BackgroundTextureOpacity1 = 1.0f,
                                BackgroundTextureOpacity2 = 0.5f,
                                HideBackgroundWhenTextIsEmpty = true,
                                TextFunc = () => "t_curse",
                                HintFunc = () => assume_mobs_cursed ? "Yes/" + ((1 - cursed_mob_threshold) * 100 )+ "%" : "No",
                        });

        // Drop label (4) : Total hp of mobs in trillions
        currentLevelDecorator.ExpandDownLabels.Add(
                new TopLabelDecorator(Hud)
                        {
                                TextFont = Hud.Render.CreateFont("tahoma", 6, 180, 255, 255, 255, true, false, true),
                                ExpandedHintFont = expandedHintFont,
                                ExpandedHintWidthMultiplier = 2,
                                BackgroundTexture1 = Hud.Texture.Button2TextureOrange,
                                BackgroundTexture2 = Hud.Texture.BackgroundTextureBlue,
                                BackgroundTextureOpacity1 = 1.0f,
                                BackgroundTextureOpacity2 = 0.5f,
                                HideBackgroundWhenTextIsEmpty = true,
                                TextFunc = () => "hp_mobs(t)",
                                HintFunc = () => totalAliveMonstersHealthSum_in_trillions_truncated + "",
                        });

        // Drop label (5) : Total hp of conduit in trillions
        currentLevelDecorator.ExpandDownLabels.Add(
                new TopLabelDecorator(Hud)
                        {
                                TextFont = Hud.Render.CreateFont("tahoma", 6, 180, 255, 255, 255, true, false, true),
                                ExpandedHintFont = expandedHintFont,
                                ExpandedHintWidthMultiplier = 2,
                                BackgroundTexture1 = Hud.Texture.Button2TextureOrange,
                                BackgroundTexture2 = Hud.Texture.BackgroundTextureBlue,
                                BackgroundTextureOpacity1 = 1.0f,
                                BackgroundTextureOpacity2 = 0.5f,
                                HideBackgroundWhenTextIsEmpty = true,
                                TextFunc = () => "cond_dps(t)",
                                HintFunc = () => total_conduit_dps_team_in_trillions_truncated + "",
                        });

        LabelList.LabelDecorators.Clear();

        LabelList.LabelDecorators.Add(currentLevelDecorator);

        // Make the plugin disappear if everyone in the team is dead. This only happens when computed total_conduit_dps_team_in_trillions = 0
        if(total_conduit_dps_team_in_trillions == 0) {
                return;
        }

        // Green light: Take conduit!! hp_mobs is less than the total dps of conduit
        if(totalAliveMonstersHealthSum_in_trillions > 0 && totalAliveMonstersHealthSum_in_trillions < total_conduit_dps_team_in_trillions)
        {
                LabelList.LabelDecorators.Add(new TopLabelDecorator(Hud)
                                {
                                        BorderBrush = Hud.Render.CreateBrush(255, 0, 255, 0, -1), // RGB(255, 0, 255) = Green
                                        TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 57, 137, 205, true, false, true),
                                        BackgroundTexture1 = Hud.Texture.Button2TextureOrange,
                                        BackgroundTexture2 = Hud.Texture.BackgroundTextureBlue,
                                        BackgroundTextureOpacity1 = 1.0f,
                                        BackgroundTextureOpacity2 = 0.5f,
                                        TextFunc = () => Math.Truncate((1 - (totalAliveMonstersHealthSum_in_trillions / total_conduit_dps_team_in_trillions) * 100) * 100) / 100 + "%",
                                });
        }
        // Yellow light: Getting close ready to take conduit - hp mobs are greater expected dps of conduit but it's within @cond_yellow_light_threshold
        else if((totalAliveMonstersHealthSum_in_trillions > total_conduit_dps_team_in_trillions) &&
                (totalAliveMonstersHealthSum_in_trillions <= (total_conduit_dps_team_in_trillions * cond_yellow_light_threshold)))
        {
                LabelList.LabelDecorators.Add(new TopLabelDecorator(Hud)
                                {
                                        BorderBrush = Hud.Render.CreateBrush(255, 255, 255, 0, -1),
                                        TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 57, 137, 205, true, false, true),
                                        BackgroundTexture1 = Hud.Texture.Button2TextureOrange,
                                        BackgroundTexture2 = Hud.Texture.BackgroundTextureBlue,
                                        BackgroundTextureOpacity1 = 1.0f,
                                        BackgroundTextureOpacity2 = 0.5f,
                                        TextFunc = () => Math.Truncate((((totalAliveMonstersHealthSum_in_trillions / total_conduit_dps_team_in_trillions) - 1)  * 100) * 100) / 100 + "%",
                                });
        }
        // Red light: Not even close to take conduit - hp mobs are greater expected dps of conduit but it's greater than @cond_yellow_light_threshold
        else if((totalAliveMonstersHealthSum_in_trillions > total_conduit_dps_team_in_trillions) &&
                (totalAliveMonstersHealthSum_in_trillions > (total_conduit_dps_team_in_trillions * cond_yellow_light_threshold)))
        {
                LabelList.LabelDecorators.Add(new TopLabelDecorator(Hud)
                                {
                                        BorderBrush = Hud.Render.CreateBrush(255, 255, 0, 0, -1),
                                        TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 57, 137, 205, true, false, true),
                                        BackgroundTexture1 = Hud.Texture.Button2TextureOrange,
                                        BackgroundTexture2 = Hud.Texture.BackgroundTextureBlue,
                                        BackgroundTextureOpacity1 = 1.0f,
                                        BackgroundTextureOpacity2 = 0.5f,
                                        TextFunc = () => Math.Truncate((((totalAliveMonstersHealthSum_in_trillions / total_conduit_dps_team_in_trillions) - 1) * 100) * 100) / 100 + "%",
                                });
        }
        // No light: Display normal light we aren't ready for anything
        else
        {
                LabelList.LabelDecorators.Add(new TopLabelDecorator(Hud)
                                {
                                        TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 57, 137, 205, true, false, true),
                                        BackgroundTexture1 = Hud.Texture.Button2TextureOrange,
                                        BackgroundTexture2 = Hud.Texture.BackgroundTextureBlue,
                                        BackgroundTextureOpacity1 = 1.0f,
                                        BackgroundTextureOpacity2 = 0.5f,
                                        TextFunc = () => Math.Truncate((((totalAliveMonstersHealthSum_in_trillions / total_conduit_dps_team_in_trillions) -1) * 100) * 100) / 100 + "%",
                                });
        }
        LabelList.Paint();
}
}
}
