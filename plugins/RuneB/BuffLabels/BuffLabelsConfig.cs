﻿using Turbo.Plugins.Default;

namespace Turbo.Plugins.RuneB
{

    public class BuffLabelsConfig : BasePlugin, ICustomizer
    {

        public BuffLabelsConfig()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }

        public void Customize()
        {
            Hud.RunOnPlugin<RuneB.BuffLabelsPlugin>(plugin =>
            {
                //REMOVE OR ADD // IN FRONT OF LINES OF CODE TO ACTIVATE OR DEACTIVATE THEM
                //plugin.Debug = true; // show all labels added over time to see the animations.

                //--------------------- CHANGE LOOK EXAMPLES --------------------------- 
                //POSITION, MARGIN, SIZE & ANIMATION
                plugin.YPos = 0.55f; //Vertical position (0 == top, 1 == bottom)
                plugin.XPos = 0.5f; //Horizontal position (0 == left, 1 == right)

                //plugin.YPosIncrement = 0.01f; // Vertical distance between labels
                //plugin.SmoothMovement = false; // if true, animate movement to new position. else, jump straight there.
                //plugin.SmoothSpeed = 0.1f; //Animation speed.

                //plugin.SizeModifier = 1.2f; // change size of labels and text.
                plugin.NumRows = 6; //The amount of rows allowed. (if 1, all labels will align horizontally. Set a number higher than the highest number of buffs to keep all in one column.)

                //--------------------- ADD BUFF EXAMPLES ---------------------------
                //How to add buffs (Find sno's in /doc/sno_powers.txt): 
                //    plugin.Labels.Add(new RuneB.Label(<Shown buff name>, <Sno>, <Icon count>, <A brush>));

                //Monk:
                //plugin.Labels.Add(new RuneB.Label("Flying Dragon", 246562, 1, Hud.Render.CreateBrush(100, 50, 200, 255, 0)));

                //Wizard
                //plugin.Labels.Add(new RuneB.Label("Archon", 134872, 2, Hud.Render.CreateBrush(100, 0, 80, 215, 0))); 
                //plugin.Labels.Add(new RuneB.Label("Magic Weapon", 76108, 0, Hud.Render.CreateBrush(100, 0, 45, 130, 0))); 
                //plugin.Labels.Add(new RuneB.Label("Energy Armor", 86991, 0, Hud.Render.CreateBrush(100, 140, 1, 170, 0)));

                //Barb
                //plugin.Labels.Add(new RuneB.Label("War Cry", 375483, 0, Hud.Render.CreateBrush(100, 100, 50, 40, 0)));
                //plugin.Labels.Add(new RuneB.Label("Berserker", 79607, 0, Hud.Render.CreateBrush(100, 45, 100, 55, 0))); //Wrath of the Beserker
				plugin.Labels.Add(new RuneB.Label("Band of Might", 447060, 1, Hud.Render.CreateBrush(100, 0, 204, 204, 0)));
                //Crusader
                //plugin.Labels.Add(new RuneB.Label("Akarat's Champ", 269032, 1, Hud.Render.CreateBrush(100, 70, 50, 40, 0)));
                //plugin.Labels.Add(new RuneB.Label("Iron Skin", 291804, 0, Hud.Render.CreateBrush(100, 90, 60, 70, 0)));

                //Witch Doctor
                //plugin.Labels.Add(new RuneB.Label("Arachyr", 30631, 5, Hud.Render.CreateBrush(100, 255, 66, 33, 0)));

				//Necromancer
				plugin.Labels.Add(new RuneB.Label("Buff", 478534, 1, Hud.Render.CreateBrush(100, 0, 204, 204, 0)));
				
                //--------------------- DISABLE DEFAULT BUFFS ---------------------------
                //plugin.ShowIgnorePain = false;
                //plugin.ShowInnerSanctuary = false;
                //plugin.ShowOculus = false;
            });
        }
    }
}

