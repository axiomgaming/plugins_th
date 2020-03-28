//################################################################################
//# ..:: created with TCT Version 5.2 for THUD v7.6 (17.11.20.0) ::.. by RealGsus #
//################################################################################

using Turbo.Plugins.Default;

namespace Turbo.Plugins.BM
{

    public class OtherPlayersNameColorByClassPlugin : BasePlugin, ICustomizer
    {

        public OtherPlayersNameColorByClassPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }


        public void Customize()
        {
            Hud.RunOnPlugin<OtherPlayersPlugin>(plugin =>
            {
                plugin.DecoratorByClass[HeroClass.Barbarian].GetDecorators<MapLabelDecorator>().ForEach(d =>
                {
                    d.LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 247, 150, 70, true, false, 128, 0, 0, 0, true);
                    d.Up = true;
                });
                plugin.DecoratorByClass[HeroClass.Barbarian].GetDecorators<GroundLabelDecorator>().ForEach(d =>
                {
                    d.BorderBrush = Hud.Render.CreateBrush(255, 247, 150, 70, 1);
                    d.TextFont = Hud.Render.CreateFont("tahoma", 6f, 255, 247, 150, 70, true, false, 128, 0, 0, 0, true);
                });
             
                plugin.DecoratorByClass[HeroClass.Crusader].GetDecorators<MapLabelDecorator>().ForEach(d =>
                {
                    d.LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 255, 255, false, false, 128, 0, 0, 0, true);
                    d.Up = true;
                });
                plugin.DecoratorByClass[HeroClass.Crusader].GetDecorators<GroundLabelDecorator>().ForEach(d =>
                {
                    d.BorderBrush = Hud.Render.CreateBrush(255, 255, 255, 255, 1);
                    d.TextFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 255, 255, false, false, 128, 0, 0, 0, true);
                });
             
                plugin.DecoratorByClass[HeroClass.DemonHunter].GetDecorators<MapLabelDecorator>().ForEach(d =>
                {
                    d.LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 0, 0, false, false, 128, 0, 0, 0, true);
                    d.Up = true;
                });
                plugin.DecoratorByClass[HeroClass.DemonHunter].GetDecorators<GroundLabelDecorator>().ForEach(d =>
                {
                    d.BorderBrush = Hud.Render.CreateBrush(255, 255, 0, 0, 1);
                    d.TextFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 0, 0, true, false, 128, 0, 0, 0, true);
                });
             
                plugin.DecoratorByClass[HeroClass.Monk].GetDecorators<MapLabelDecorator>().ForEach(d =>
                {
                    d.LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 0, 255, 0, true, false, 128, 0, 0, 0, true);
                    d.Up = true;
                });
                plugin.DecoratorByClass[HeroClass.Monk].GetDecorators<GroundLabelDecorator>().ForEach(d =>
                {
                    d.BorderBrush = Hud.Render.CreateBrush(255, 0, 255, 0, 1);
                    d.TextFont = Hud.Render.CreateFont("tahoma", 9f, 255, 0, 255, 0, true, false, 128, 0, 0, 0, true);
                });
             
                plugin.DecoratorByClass[HeroClass.WitchDoctor].GetDecorators<MapLabelDecorator>().ForEach(d =>
                {
                    d.LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 255, 204, true, false, 128, 0, 0, 0, true);
                    d.Up = true;
                });
                //font face, size, ??, R, G, B
                plugin.DecoratorByClass[HeroClass.WitchDoctor].GetDecorators<GroundLabelDecorator>().ForEach(d =>
                {
                    d.BorderBrush = Hud.Render.CreateBrush(255, 255, 255, 204, 1);
                    d.TextFont = Hud.Render.CreateFont("tahoma", 12f, 255, 255, 255, 0, true, false, 128, 0, 0, 0, true);
                });
             
                plugin.DecoratorByClass[HeroClass.Wizard].GetDecorators<MapLabelDecorator>().ForEach(d =>
                {
                    d.LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 51, 153, false, false, 128, 0, 0, 0, true);
                    d.Up = true;
                });
                plugin.DecoratorByClass[HeroClass.Wizard].GetDecorators<GroundLabelDecorator>().ForEach(d =>
                {
                    d.BorderBrush = Hud.Render.CreateBrush(255, 255, 51, 153, 1);
                    d.TextFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 51, 153, true, false, 128, 0, 0, 0, true);
                });
             
                plugin.DecoratorByClass[HeroClass.Necromancer].GetDecorators<MapLabelDecorator>().ForEach(d =>
                {
                    d.LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 59, 204, 255, true, false, 128, 0, 0, 0, true);
                    d.Up = true;
                });
                plugin.DecoratorByClass[HeroClass.Necromancer].GetDecorators<GroundLabelDecorator>().ForEach(d =>
                {
                    d.BorderBrush = Hud.Render.CreateBrush(255, 59, 204, 255, 1);
                    d.TextFont = Hud.Render.CreateFont("tahoma", 9f, 255, 59, 204, 255, true, false, 128, 0, 0, 0, true);
                });
             
            });
        }

    }

}
