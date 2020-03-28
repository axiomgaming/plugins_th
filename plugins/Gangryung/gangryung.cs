
using System.Collections.Generic;
using System.Linq;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.gangryung
{

    public class OtherPlayersHeadsPlugin : BasePlugin, IInGameWorldPainter, ICustomizer
    {

        public Dictionary<HeroClass, WorldDecoratorCollection> DecoratorByClass { get; set; }
        public float NameOffsetX { get; set; }
        public float NameOffsetY { get; set; }
        public float NameOffsetZ { get; set; }
        public bool ShowCompanions { get; set; }
        public WorldDecoratorCollection ZDPSDecorator { get; set; }


        public OtherPlayersHeadsPlugin()
        {
            Enabled = true;
            DecoratorByClass = new Dictionary<HeroClass, WorldDecoratorCollection>();
            NameOffsetX = 0.0f;
            NameOffsetY = 0.0f;
            NameOffsetZ = 12.0f;
            ShowCompanions = false;
        }


        public override void Load(IController hud)
        {
            base.Load(hud);

            var grounLabelBackgroundBrush = Hud.Render.CreateBrush(120, 0, 0, 0, 0);

            ZDPSDecorator = new WorldDecoratorCollection(
                new MapLabelDecorator2(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 255, 255, false, false, 128, 0, 0, 0, true),
                    UpUp = true,
                });

            
            DecoratorByClass.Add(HeroClass.Necromancer, new WorldDecoratorCollection(
               new MapLabelDecorator2(Hud)
               {
                   LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 215, 201, 164, false, false, 128, 0, 0, 0, true),
                   Down = false,
               },
               new GroundLabelDecorator(Hud)
               {
                   BackgroundBrush = grounLabelBackgroundBrush,
                   BorderBrush = Hud.Render.CreateBrush(255, 215, 201, 164, 1),
                   TextFont = Hud.Render.CreateFont("tahoma", 6f, 255, 215, 201, 164, false, false, 128, 0, 0, 0, true),
               }
               ));
           
        }


        public void PaintWorld(WorldLayer layer)
        {
            var players = Hud.Game.Players.Where(player => !player.IsMe && player.CoordinateKnown && Hud.Game.Me.SnoArea.Sno == player.SnoArea.Sno && (player.HeadStone == null));
            foreach (var player in players)
            {

                var HeroTexture = Hud.Texture.GetTexture(890155253);                    //Default Icon


                if (player.HeroClassDefinition.HeroClass.ToString() == "Necromancer")       //Necro Icon
                {
                    if (player.HeroIsMale) HeroTexture = Hud.Texture.GetTexture(3285997023);
                    else HeroTexture = Hud.Texture.GetTexture(473831658);
                }
                else if (player.HeroClassDefinition.HeroClass.ToString() == "DemonHunter")
                {
                    if (player.HeroIsMale) HeroTexture = Hud.Texture.GetTexture(3785199803);
                    else HeroTexture = Hud.Texture.GetTexture(2939779782);
                }

                float PlayersHeadOpacity = 1f;
                var Elites = Hud.Game.Monsters.Where(M => M.IsAlive && M.Rarity != ActorRarity.Normal && M.Rarity != ActorRarity.RareMinion && M.Rarity != ActorRarity.Hireling && M.FloorCoordinate.XYDistanceTo(player.FloorCoordinate) <= 25);
                if (Elites.Count() > 0) PlayersHeadOpacity = 0.20f;

                Hud.Render.GetMinimapCoordinates(player.FloorCoordinate.X, player.FloorCoordinate.Y, out float textureX, out float textureY);



                //Display only HeroClass Necromancer
                if ((HeroTexture == Hud.Texture.GetTexture(3285997023) || HeroTexture == Hud.Texture.GetTexture(473831658)) && IsZDPS(player))                               
                {
                    HeroTexture.Draw(textureX - 11, textureY - 11, 22.3f, 24.1f, PlayersHeadOpacity);                               //Object Draw Icon
                    ZDPSDecorator.Paint(layer, null, player.FloorCoordinate, "ZNecro");                                             //zNecro only, 3 or 3+, line 185
                }
                   

                if (!DecoratorByClass.TryGetValue(player.HeroClassDefinition.HeroClass, out WorldDecoratorCollection decorator)) continue;              //Creates Icon on mini map

               //decorator.Paint(layer, null, player.FloorCoordinate.Offset(NameOffsetX, NameOffsetY, NameOffsetZ), player.BattleTagAbovePortrait);     //BattleTag name under icon
                //if (IsZDPS(player)) ZDPSDecorator.Paint(layer, null, player.FloorCoordinate, "ZNecro");                                                 //zNecro only, 3 or 3+, line 185

            }


            if (ShowCompanions && Hud.Game.NumberOfPlayersInGame == 1)
            {
                var companions = Hud.Game.Actors.Where(C => C.SnoActor.Sno == ActorSnoEnum._hireling_scoundrel || C.SnoActor.Sno == ActorSnoEnum._hireling_enchantress || C.SnoActor.Sno == ActorSnoEnum._hireling_templar);
                foreach (var companion in companions)
                {
                    var CompTexture = Hud.Texture.GetTexture(890155253);
                    if (companion.SnoActor.Sno == ActorSnoEnum._hireling_scoundrel) CompTexture = Hud.Texture.GetTexture(441912908); // scoundrel
                    else if (companion.SnoActor.Sno == ActorSnoEnum._hireling_enchantress) CompTexture = Hud.Texture.GetTexture(2807221403); // enchantress
                    else if (companion.SnoActor.Sno == ActorSnoEnum._hireling_templar) CompTexture = Hud.Texture.GetTexture(1094113362); // templar
                    else continue;

                    float CompanionsHeadOpacity = 1f;
                    var Elites = Hud.Game.Monsters.Where(M => M.IsAlive && M.Rarity != ActorRarity.Normal && M.Rarity != ActorRarity.RareMinion && M.Rarity != ActorRarity.Hireling && M.FloorCoordinate.XYDistanceTo(companion.FloorCoordinate) <= 25);
                    if (Elites.Count() > 0) CompanionsHeadOpacity = 0.20f;

                    Hud.Render.GetMinimapCoordinates(companion.FloorCoordinate.X, companion.FloorCoordinate.Y, out float textureX, out float textureY);
                    CompTexture.Draw(textureX - 11, textureY - 11, 22.3f, 24.1f, CompanionsHeadOpacity);

                }

            }


        }

        /*
         * Three item types needed: zBarb.Contains(OculusRing, ZodiacRing)
         */
        private bool IsZDPS(IPlayer player)
        {
            int Points = 0;

            var AhavarionSpearOfLycander = player.Powers.GetBuff(318868);
            if (AhavarionSpearOfLycander == null || !AhavarionSpearOfLycander.Active) {} else {Points++;}

            //var BriggsWrath = player.Powers.GetBuff(475252);
            //if (BriggsWrath == null || !BriggsWrath.Active) {} else {Points++;}

            //var EfficaciousToxin = player.Powers.GetBuff(403461);
            //if (EfficaciousToxin == null || !EfficaciousToxin.Active) {} else {Points++;}

            var OculusRing = player.Powers.GetBuff(402461);
            if (OculusRing == null || !OculusRing.Active) { } else { Points++; }

            var ZodiacRing = player.Powers.GetBuff(402459);
            if (ZodiacRing == null || !ZodiacRing.Active) { } else { Points++; }

            //if (player.Offense.SheetDps < 1000000f) Points++;
            //if (player.Offense.SheetDps > 1500000f) Points--;

            //if (player.Defense.EhpMax > 80000000f) Points++;

            //var ConventionRing = player.Powers.GetBuff(430674);
            //if (ConventionRing == null || !ConventionRing.Active) {} else {Points--;}

            //var Stricken = player.Powers.GetBuff(428348);
            //if (Stricken == null || !Stricken.Active) {} else {Points--;}

            if (Points >= 3) { return true; } else { return false; }

        }

        public void Customize()
        {
            Hud.TogglePlugin<OtherPlayersPlugin>(false);  // disables OtherPlayersPlugin
        }
    }


    public class MapLabelDecorator2 : IWorldDecorator
    {

        public bool Enabled { get; set; }
        public WorldLayer Layer { get; private set; }
        public IController Hud { get; private set; }

        public IFont LabelFont { get; set; }
        public bool Up { get; set; }
        public bool UpUp { get; set; }
        public bool Down { get; set; }
        public float RadiusOffset { get; set; }

        public MapLabelDecorator2(IController hud)
        {
            Enabled = true;
            Layer = WorldLayer.Map;
            Hud = hud;
            UpUp = false;
            Up = false;
            Down = false;
        }

        public void Paint(IActor actor, IWorldCoordinate coord, string text)
        {
            if (!Enabled) return;
            if (LabelFont == null) return;
            if (string.IsNullOrEmpty(text)) return;

            Hud.Render.GetMinimapCoordinates(coord.X, coord.Y, out float mapx, out float mapy);

            var layout = LabelFont.GetTextLayout(text);
            if (Up)
            {
                LabelFont.DrawText(layout, mapx - layout.Metrics.Width / 2, mapy + RadiusOffset - layout.Metrics.Height);
            }
            else if (UpUp)
            {
                LabelFont.DrawText(layout, mapx - layout.Metrics.Width / 2, mapy + RadiusOffset - (layout.Metrics.Height) * 1.7f);
            }
            else if (Down)
            {
                LabelFont.DrawText(layout, mapx - layout.Metrics.Width / 2, mapy + RadiusOffset + layout.Metrics.Height);
            }
            else
            {
                LabelFont.DrawText(layout, mapx - layout.Metrics.Width / 2, mapy - RadiusOffset);
            }
        }

        public IEnumerable<ITransparent> GetTransparents()
        {
            yield break;
        }

    }
}