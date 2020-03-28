using Turbo.Plugins.Default;

namespace Turbo.Plugins.DavMonster
{
	public class DAV_BossConfigPlugin : BasePlugin, ICustomizer {
		public DAV_BossConfigPlugin() {
			Enabled = true;
		}
		
		public void Customize() {
			
			// Add the Boss Watch List
			Hud.RunOnPlugin<DAV_BossAnimeLog>(plugin => {
				// Add Boss for checking the animation sno number
				
				// plugin.BossList.Add("Agnidox"); // (empowered Demonic Hellflyer)
				// plugin.BossList.Add("Blighter"); // (empowered Herald of Pestilence)
				// plugin.BossList.Add("Bloodmaw"); // (empowered Executioner)
				// plugin.BossList.Add("Bone Warlock"); // (empowered Skeletal Summoner)
				// plugin.BossList.Add("Cold Snap"); // (empowered Izual)
				// plugin.BossList.Add("Crusader King"); // (empowered Skeleton King)
				// plugin.BossList.Add("Ember"); // (empowered Morlu Caster)
				// plugin.BossList.Add("Erethon"); // (empowered Corrupted Angel)
				// plugin.BossList.Add("Eskandiel"); // (empowered Corpse Raiser)
				// plugin.BossList.Add("Hamelin"); // (empowered Rat King)
				// plugin.BossList.Add("Infernal Maiden"); // (empowered Fire Maiden)
				// plugin.BossList.Add("Man Carver"); // (empowered Butcher)
				// plugin.BossList.Add("Orlash"); // (empowered Terror Demon)
				// plugin.BossList.Add("Perdition"); // (empowered Rakanoth)
				// plugin.BossList.Add("Perendi"); // (empowered Mallet Lord)
				// plugin.BossList.Add("Raiziel"); // (empowered Exarch)
				// plugin.BossList.Add("Rime"); // (empowered Xah'Rith the Keywarden)
				// plugin.BossList.Add("Sand Shaper"); // (empowered Zoltun Kulle)
				// plugin.BossList.Add("Saxtris"); // (empowered Deceiver)
				// plugin.BossList.Add("Stonesinger"); // (empowered Sand Dweller)
				// plugin.BossList.Add("Tethrys"); // (empowered Succubus)
				// plugin.BossList.Add("The Binder"); // (empowered Cydaea)
				// plugin.BossList.Add("The Choker"); // (empowered Barbed Lurker)
				// plugin.BossList.Add("Vesalius"); // (empowered Vidian)
				// plugin.BossList.Add("Voracity"); // (empowered Ghom)
				
				//BOSS Skill, Thanks for user evan6944's work
				plugin.BossSkillBoard.Add("Man Carver", "Add(s)\tNone\n\nSkills\t1. Mark of Fire (lasts 15s)\n\t2. Charge\n\t3. Heavy Smash\n\t4. Ancient Spear (75%)\n\t5. Sickle Grab (50%)\n\nAffix\t1. Waller");
				plugin.BossSkillBoard.Add("The Choker", "Add(s)\t1. Slime (100%) [1 - 3 per cast (Cap: >10)]\n\nSkills\t1. Poison Worms\n\t2. Poison Blast\n\t3. Plagued Circle (<80%)\n\nAffix\tNone");
				plugin.BossSkillBoard.Add("Hamelin", "Add(s)\t1. Ratlings x 10 (100%) [Cap: 10]\n\t2. Ratlings x 2 - 3 (<90%) [Cap: 20]\n\nSkills\t1. Rat-nado (lasts 20s)\n\t2. Plagued Arena (lasts 10s)\n\t3. Digger (teleport) \n\nAffix\tNone");
				plugin.BossSkillBoard.Add("Blighter", "Add(s)\tNone\n\nSkills\t1. Poison Nova\n\t2. Plague Rings\n\t3. Plague Sweep\n\t4. Plague Storm  (<50%)\n\nAffix\t1. Knockback");
				plugin.BossSkillBoard.Add("Infernal Maiden", "Add(s)\tNone\n\nSkills\t1. Overhead Attack\n\t2. Whirling Mortar\n\t3. Fire Nova (<45%)\n\nAffix\t1. Teleporter");
				plugin.BossSkillBoard.Add("Erethon", "Add(s)\tNone\n\nSkills\t1. Dash\n\t2. Poison Blast\n\t3. Poison Balls\n\t4. Poison Explosion (<50%)\n\nAffix\tNone");
				plugin.BossSkillBoard.Add("Agnidox", "Add(s)\tNone\n\nSkills\t1. Fireball\n\t2. Flame Breath\n\t3. Mark of Fire (lasts 15s)\n\t4. Flame Nova (<50%)\n\nAffix\t1. Fast\n\t2. Mortar");
				plugin.BossSkillBoard.Add("Ember", "Add(s)\t1. Demented Fallen x 2 - 3 (95%) \n\t2. Fallen Shaman x 2 - 3 (60%) [Cap: 5]\n\nSkills\t1. Meteor\n\nAffix\t1. Teleporter");
				plugin.BossSkillBoard.Add("Tethrys", "Add(s)\tNone\n\nSkills\t1. Fireball 1 (100%, slow)\n\t2. Blood Star (lasts 5s)\n\t3. Geyser (60%, lasts 3s)\n\t4. Fireball 2 (40%, fast)\n\nAffix\t1. Teleporter\n\t2. Knockback");
				plugin.BossSkillBoard.Add("Vesalius", "Add(s)\tNone\n\nSkills\t1. Energy Barrage\n\t2. Gateway\n\nAffix\t1. Wormhole\n\t2. Frozen Pulse");
				plugin.BossSkillBoard.Add("Saxtris", "Add(s)\t1. Winged Larvae x 8 - 15 (75%) [Cap: 2 sets]\n\t2. Snakechild x 8 - 15 (50%) [Cap: 2 sets]\n\nSkills\t1. Energy Twister (lasts 30s)\n\nAffix\t1. Vortex");
				plugin.BossSkillBoard.Add("Cold Snap", "Add(s)\tNone\n\nSkills\t1. Charge\n\t2. Frozen Nova\n\t3. Frozen Storm (50%)\n\nAffix\t1. Frozen Pulse");
				plugin.BossSkillBoard.Add("Bloodmaw", "Add(s)\tNone\n\nSkills\t1. Leaping Strike\n\t2. Leap\n\nAffix\t1. Fast");
				plugin.BossSkillBoard.Add("Stonesinger", "Add(s)\t1. Fissure (100%, lasts 20s) [Cap: 3]\n\nSkills\t1. Shovel\n\t2. Charge\n\nAffix\t1. Knockback");
				plugin.BossSkillBoard.Add("Perdition", "Add(s)\tNone\n\nSkills\t1. Blade Cleave\n\t2. Teleport Strike\n\t3. Volley\n\nAffix\t1. Fast");
				plugin.BossSkillBoard.Add("Bone Warlock", "Add(s)\tBones (95%) [Cap: 5]\n\t1. Quick Bones\n\t2. Reflecting Bones\n\t3. Mortar Bones\n\t4. Knockback Bones\n\nSkills\t1. Arcane Bolt\n\nAffix\t1. Wormhole");
				plugin.BossSkillBoard.Add("Rime", "Add(s)\tNone\n\nSkills\t1. Frost Pools\n\t2. Frost Ring\n\t3. Volley\n\nAffix\t1. Teleporter");
				plugin.BossSkillBoard.Add("Raiziel", "Add(s)\tNone\n\nSkills\t1. Lightning Orb\n\t2. Holy Bolt Nova (75%, 25%)\n\nAffix\t1. Teleporter");
				plugin.BossSkillBoard.Add("Crusader King", "Add(s)\tSkeletons (100%, 3 - 8/cast) [Cap: 14 - 15]\n\t1. Returned\n\t2. Returned Archer\n\t3. Forgotten Soldier\n\nSkills\t1. Arcane Nova\n\t2. Teleport Strike\n\t3. Spinning Strike\n\nAffix\t1. Jailer");
				plugin.BossSkillBoard.Add("Perendi", "Add(s)\t1. Stonecrusher (100%, 1 - 5/cast, lasts 20s) [Cap: None]\n\nSkills\t1. Cave-In\n\nAffix\t1. Fast\n\t2. Teleporter");
				plugin.BossSkillBoard.Add("The Binder", "Add(s)\t1. Spiderlings (85%, 5-7/cast) [Cap: 10]\n\nSkills\t1. Venomballs\n\t2. Net Toss\n\t3. Poison Spit (65%)\n\nAffix\t1. Fast (65%)");
				plugin.BossSkillBoard.Add("Voracity", "Add(s)\t1. Acid Slime (65%, 2/cast) [Cap: 4/player]\n\nSkills\t1. Flatulence (lasts 65s)\n\t2. Bile Spew (35%)\n\nAffix\tNone");
				plugin.BossSkillBoard.Add("Sand Shaper", "Add(s)\tNone\n\nSkills\t1. Fireball\n\t2. Energy Twister (75%, lasts 30s)\n\t3. Cave-In (65%)\n\t4. Slow Time (65%, lasts 15s)\n\nAffix\t1. Teleporter");
				plugin.BossSkillBoard.Add("Orlash", "Add(s)\t1. Echoes (100%) [Cap: 2-3]\n\nSkills\t1. Lightning Breath\n\nAffix\t1. Teleporter\n\t2. Waller\n\t3. Fast");
				plugin.BossSkillBoard.Add("Eskandiel", "Add(s)\t1. Bones (100%) [Cap: 10-13]\n\tCanine Bones\n\tSpitting Bones\n\tHungering Bones\n\tRisen Bones\n\nSkills\t1. Repulsion\n\t2. Tug\n\nAffix\t1. Teleporter\n\t2. Arcane Enchanted\n\t3. Fast");
			});
			
			Hud.RunOnPlugin<DAV_BossWarmingPlugin>(plugin => {
				plugin.BossOffsetX = -20.0f;
				plugin.BossOffsetY = 0.0f;
				plugin.BossOffsetZ = 10.0f;
				plugin.MeOffsetX = -15.0f;
				plugin.MeOffsetY = 5.0f;
				plugin.MeOffsetZ = 0.0f;
				plugin.ShowOrlashClone = false;
				plugin.GRonly = true;
				plugin.onBoss = false;
				plugin.onMe = true;
				
				//BOSS Warming, Thanks for user evan6944's work
				
				//Orlash
				plugin.WarmingMessage.Add(AnimSnoEnum._terrordemon_attack_firebreath, "Breath");
				plugin.WarmingMessage.Add(AnimSnoEnum._terrordemon_attack_01, "Attack");
				plugin.WarmingMessage.Add(AnimSnoEnum._terrordemon_generic_cast, "Alternate");
				
				//Bloodmaw
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_westmarchbrute_taunt, "Jump");
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_westmarchbrute_b_attack_06_in, "Cut");
				
				//Crusader King
				plugin.WarmingMessage.Add(AnimSnoEnum._skeletonking_cast_summon, "Summon");
				plugin.WarmingMessage.Add(AnimSnoEnum._skeletonking_whirlwind_start, "Whirlwind Start");
				plugin.WarmingMessage.Add(AnimSnoEnum._skeletonking_whirlwind_loop, "Whirlwind Loop");
				plugin.WarmingMessage.Add(AnimSnoEnum._skeletonking_whirlwind_end, "Whirlwind End");
				plugin.WarmingMessage.Add(AnimSnoEnum._skeletonking_teleport, "Teleport");
				plugin.WarmingMessage.Add(AnimSnoEnum._skeletonking_attack_02, "Attack 2");
				
				//Infernal Maiden
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_deathmaiden_fire_attack_01, "Fire Attack");
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_deathmaiden_attack_04_aoe, "AoE");
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_deathmaiden_attack_special_360_01, "360 Attack");
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_deathmaiden_attack_special_flip_01, "Flip Attack");
				
				//Man Carver
				plugin.WarmingMessage.Add(AnimSnoEnum._butcher_attack_chain_01_in, "Chain Attack");
				plugin.WarmingMessage.Add(AnimSnoEnum._butcher_attack_charge_01_in, "Charge");
				plugin.WarmingMessage.Add(AnimSnoEnum._butcher_attack_fanofchains, "Chain");
				plugin.WarmingMessage.Add(AnimSnoEnum._butcher_attack_05_telegraph, "Attack");
				
				//Saxtris
				plugin.WarmingMessage.Add(AnimSnoEnum._snakeman_melee_generic_cast_01, "Melee");
				plugin.WarmingMessage.Add(AnimSnoEnum._snakeman_melee_attack_01, "Melee2");
				
				//Hamelin
				plugin.WarmingMessage.Add(AnimSnoEnum._p4_ratking_spawn_01, "Spawn");
				plugin.WarmingMessage.Add(AnimSnoEnum._p4_ratking_burrow_in, "Burrow");
				plugin.WarmingMessage.Add(AnimSnoEnum._p4_ratking_summon_01, "Summon");
				plugin.WarmingMessage.Add(AnimSnoEnum._p4_ratking_roar_summon, "Roar Summon");
				
				//Bone Warlock
				plugin.WarmingMessage.Add(AnimSnoEnum._skeletonsummoner_generic_cast, "Cast");
				plugin.WarmingMessage.Add(AnimSnoEnum._skeletonsummoner_attack_01, "Attack");
				
				//Perendi
				plugin.WarmingMessage.Add(AnimSnoEnum._malletdemon_generic_cast, "Generic Attack");
				plugin.WarmingMessage.Add(AnimSnoEnum._malletdemon_attack_01, "Attack");
				
				//The Choker
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_squigglet_generic_cast, "Attack"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_squigglet_rangedattack_v2, "Ranged Attack"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_squigglet_strafe_attack_left_01, "Strafe Left"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_squigglet_strafe_attack_right_01, "Strafe Right"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_squigglet_taunt_01, "Taunt"); 
				
				//Eskandiel
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_dark_angel_generic_cast, "Cast"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_dark_angel_cast, "Dark Angel"); 
				
				//Voracity
				plugin.WarmingMessage.Add(AnimSnoEnum._gluttony_attack_chomp, "Chomp"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._gluttony_attack_areaeffect, ""); 
				plugin.WarmingMessage.Add(AnimSnoEnum._gluttony_attack_ranged_01, "Ranged Attack"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._gluttony_attack_sneeze, "Sneeze"); 
				
				//Vesalius
				plugin.WarmingMessage.Add(AnimSnoEnum._p6_envy_cast_02, "Cast"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._p6_envy_teleport_start_02, "Teleport"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._p6_envy_attack_01, "Attack"); 
				
				//Stonesinger
				plugin.WarmingMessage.Add(AnimSnoEnum._sandmonster_temp_rock_throw, "Rock Throw"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._sandmonsterblack_attack_03_sandwall, "Sandwall"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._sandmonster_attack_01, "Attack"); 

				//Agnidox
				plugin.WarmingMessage.Add(AnimSnoEnum._demonflyer_mega_fireball_01, "Fireball"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._demonflyer_mega_firebreath_01, "Firebreath"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._demonflyer_mega_attack_01, "Attack"); 
				
				//Cold Snap
				plugin.WarmingMessage.Add(AnimSnoEnum._bigred_firebreath_combo_01, "Firebreath"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._bigred_charge_01, "Charge"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._bigred_attack_02, "Attack"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._bigred_generic_cast_01, "Cast"); 
				
				//The Binder
				plugin.WarmingMessage.Add(AnimSnoEnum._mistressofpain_attack_01, "Attack"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._mistressofpain_attack_spellcast_summon_webpatch, "Webs"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._mistressofpain_attack_spellcast_poison, "Poison"); 
				
				//Ember
				plugin.WarmingMessage.Add(AnimSnoEnum._morluspellcaster_generic_cast, "Cast"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._morluspellcaster_attack_aoe_01, "AoE");

				//Tethrys
				plugin.WarmingMessage.Add(AnimSnoEnum._succubus_generic_cast_01, "Cast");  // nFire Ball\nGEYSER
				plugin.WarmingMessage.Add(AnimSnoEnum._succubus_attack_melee_01, "Melee"); 
				
				//Raiziel
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_sniperangel_firebomb_01, "Firebomb"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_sniperangel_temp_cast_01, "Cast"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_sniperangel_lightning_spray_01, "Lightning Spray"); 
				
				//Sand Shaper
				plugin.WarmingMessage.Add(AnimSnoEnum._zoltunkulle_aoe_01, "AoE"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._zoltunkulle_direct_cast_04, "Direct Cast"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._zoltunkulle_taunt_01, ""); 
				plugin.WarmingMessage.Add(AnimSnoEnum._zoltunkulle_omni_cast_05_fadeout, "Cast 1"); 
				plugin.WarmingMessage.Add(AnimSnoEnum._zoltunkulle_omni_cast_01, "Cast 2");  
				plugin.WarmingMessage.Add(AnimSnoEnum._zoltunkulle_omni_cast_04, "Cast 3");  
								
				//Rime
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_lr_boss_morluspellcaster_generic_cast, "Cast");  //Volley\nFrost Ring\nFront Pools
				plugin.WarmingMessage.Add(AnimSnoEnum._p2_morluspellcaster_attack_melee_01_uber, "Uber");  
				
				//Blighter
				plugin.WarmingMessage.Add(AnimSnoEnum._creepmob_attack_04_in, "Attack");  
				plugin.WarmingMessage.Add(AnimSnoEnum._creepmob_generic_cast, "Cast");  
				plugin.WarmingMessage.Add(AnimSnoEnum._creepmob_attack_01, "Attack 2");  
				plugin.WarmingMessage.Add(AnimSnoEnum._creepmob_attack_04_middle, "Attack 3");  
				
				//Perdition
				plugin.WarmingMessage.Add(AnimSnoEnum._lordofdespair_attack_energyblast, "Energy Blast");  
				plugin.WarmingMessage.Add(AnimSnoEnum._lordofdespair_attack_stab, "Stab");  
				plugin.WarmingMessage.Add(AnimSnoEnum._lordofdespair_attack_teleport_full, "Teleport");  
				plugin.WarmingMessage.Add(AnimSnoEnum._lordofdespair_spellcast, "Spellcast");  
								
				//Erethon	
				plugin.WarmingMessage.Add(AnimSnoEnum._x1_lr_boss_angel_corrupt_a_cast_01, "Cast");  
				plugin.WarmingMessage.Add(AnimSnoEnum._angel_corrupt_attack_01, "Attack 1");  
				plugin.WarmingMessage.Add(AnimSnoEnum._angel_corrupt_attack_dash_in, "Dash");
			});
		}
	}
}