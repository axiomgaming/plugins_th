// Modificación del plugin EliteBarPlugin de Gigi. El original puede encontrarse en:
// https://www.ownedcore.com/forums/diablo-3/turbohud/turbohud-community-plugins/612897-english-gigi-elitebarplugin.html

using System.Linq;
using System.Collections.Generic;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.Gigi
{
    public class EliteBarPlugin : BasePlugin, IInGameWorldPainter
    {
	public WorldDecoratorCollection HitBoxDecorator { get; set; }
        public IFont LightFont { get; set; }
        public IFont RedFont { get; set; }
		public IFont BlueFont { get; set; }
        public IFont NameFont { get; set; }
        public IBrush cBrush { get; set; }
        public IFont cFont { get; set; }
        public IBrush BackgroundBrush { get; set; }
        public IBrush BorderBrush { get; set; }
		public IBrush RegularBorder{ get; set; }
        public IBrush RareBrush { get; set; }
        public IBrush RareJuggerBrush { get; set; }
		public IBrush FrozenBorder { get; set; }
        public IBrush RareMinionBrush { get; set; }
        public IBrush ChampionBrush { get; set; }
        public IBrush BossBrush { get; set; }
        public bool JuggernautHighlight { get; set; }
		public bool FrozenHighlight { get; set; }
        public bool MissingHighlight { get; set; }
        public bool ShowRareMinions { get ; set; }
        public bool ShowDebuffAndCC { get; set; }
        public bool ShowBossHitBox { get; set; }
        public bool ShowMonsterType { get; set; }
        public bool CircleNonIllusion { get; set; }
        public float XPos { get; set; }
        public float YPos { get; set; }
        public float XScaling { get; set; }
        public float YScaling { get; set; }
        public string PercentageDescriptor { get; set; }
        public Dictionary<MonsterAffix, string> DisplayAffix;
        private float px, py, h, w2;
		public bool OnlyGR { get; set; }
		public bool ShowCurses { get; set; }

 	public IFont BlancoFontA { get; set; }
 	public IFont NaranjaFontA { get; set; }
 	public IFont AmarilloFontA { get; set; }
 	public IFont RojoFontA { get; set; }
 	public IFont JugFontA { get; set; }
 	public IFont MoradoFontA { get; set; }
 	public IFont MarronFontA { get; set; }
 	public IFont GrisFontA { get; set; }
 	public IFont AzulFontA { get; set; }
 	public IFont AzulFFontA { get; set; }
 	public IFont VerdeFontA { get; set; }
 	public IFont VerdeCFontA { get; set; }
	public IFont TeleportFontA { get; set; }
	public IFont HordaFontA { get; set; }

    public IFont ColorA { get; set; }

        public EliteBarPlugin()
        {
            Enabled = true;

        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            //Configuration
            MissingHighlight = true;
            JuggernautHighlight = true;
			FrozenHighlight = true;
            ShowMonsterType = true;
			CircleNonIllusion = false;
            ShowRareMinions = false;
			ShowBossHitBox = false;
            ShowDebuffAndCC = true;

            XScaling = 0.8f;
            YScaling = 1.15f;
            PercentageDescriptor = "0.00";
            XPos = Hud.Window.Size.Width * 0.125f;
            YPos = Hud.Window.Size.Height * 0.0333f;
            DisplayAffix = new Dictionary<MonsterAffix, string>();

            OnlyGR = false;
			ShowCurses = true;


            //Colorization
            LightFont = Hud.Render.CreateFont("tahoma", 7f, 160, 255, 255, 255, false, false, true);
            RedFont = Hud.Render.CreateFont("tahoma", 7f, 200, 255, 0, 0, false, false, true);
			BlueFont = Hud.Render.CreateFont("tahoma", 7f, 200, 0, 0, 255, false, false, true);
            NameFont = Hud.Render.CreateFont("tahoma", 7f, 255, 255, 255, 255, false, false, true);
            BackgroundBrush = Hud.Render.CreateBrush(255, 125, 120, 120, 0);
            BorderBrush = Hud.Render.CreateBrush(255, 255, 255, 255, 1);
            RareBrush = Hud.Render.CreateBrush(255, 255, 128, 0, 0);
            RareJuggerBrush = Hud.Render.CreateBrush(255, 225, 100, 50, 0);
			RegularBorder = Hud.Render.CreateBrush(255, 255, 255, 255, 1);
			FrozenBorder = Hud.Render.CreateBrush(255, 0, 108, 255, 2);
            RareMinionBrush = Hud.Render.CreateBrush(220, 200, 100, 0, 0);
            ChampionBrush = Hud.Render.CreateBrush(255, 0, 128, 255, 0);
            BossBrush = Hud.Render.CreateBrush(255, 200, 20, 0, 0);

 	    BlancoFontA = Hud.Render.CreateFont("tahoma", 7f, 255, 255, 255, 255, false, false, true);
 	    NaranjaFontA = Hud.Render.CreateFont("tahoma", 7f, 255, 255, 170, 0, false, false, true);
 	    AmarilloFontA = Hud.Render.CreateFont("tahoma", 7f, 255, 255, 255, 0, false, false, true);
 	    RojoFontA = Hud.Render.CreateFont("tahoma", 7f, 250, 255, 50, 50, false, false, true);
 	    JugFontA = Hud.Render.CreateFont("tahoma", 7f, 200, 255, 0, 0, false, false, true);
 	    MoradoFontA = Hud.Render.CreateFont("tahoma", 7f, 250, 255, 200, 255, false, false, true);
 	    MarronFontA = Hud.Render.CreateFont("tahoma", 7f, 250, 190, 95, 0, false, false, true);
 	    GrisFontA = Hud.Render.CreateFont("tahoma", 7f, 250, 128, 128, 128, false, false, true);
 	    AzulFontA = Hud.Render.CreateFont("tahoma", 7f, 255, 0, 180, 255, false, false, true);
 	    AzulFFontA = Hud.Render.CreateFont("tahoma", 7f, 255, 40, 80, 255, false, false, true);
 	    VerdeFontA = Hud.Render.CreateFont("tahoma", 7f, 200, 0, 250, 0, false, false, true);
 	    VerdeCFontA = Hud.Render.CreateFont("tahoma", 7f, 220, 50, 250, 0, false, false, true);
		TeleportFontA = Hud.Render.CreateFont("tahoma", 7f, 220, 100, 250, 250, false, false, true);
		HordaFontA = Hud.Render.CreateFont("tahoma", 7f, 220, 140, 80, 200, false, false, true);


            //HitBoxDecorator for Bosses and NonClone-Illusionist
            HitBoxDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
                    Brush = Hud.Render.CreateBrush(255, 57, 194, 29, 3),
                    Radius = -1
                }
            );
        }

		private float AfijosColores (MonsterAffix afijo, float coordx, float coordy)  {

			ColorA = null ;
			switch (afijo)  {

                case MonsterAffix.Frozen:
						ColorA = LightFont ;
						break;
                case MonsterAffix.FrozenPulse:
						ColorA = LightFont;
						break;
				case MonsterAffix.Wormhole:
						ColorA = NaranjaFontA;
                        break;
				case MonsterAffix.Illusionist:
						ColorA = VerdeCFontA;
						break;
                case MonsterAffix.Juggernaut:
						ColorA = JugFontA;
                        break;
                case MonsterAffix.Waller:
						ColorA = MarronFontA;
						break;
                case MonsterAffix.Arcane:
						ColorA = MoradoFontA;
						break;
                case MonsterAffix.HealthLink:
						ColorA = AzulFontA;
						break;
				case MonsterAffix.Thunderstorm:
						ColorA = AzulFFontA;
						break;
				case MonsterAffix.Shielding:
						ColorA = AmarilloFontA;
						break;
				case MonsterAffix.Desecrator:
						ColorA = RojoFontA;
						break;
                case MonsterAffix.Molten:
						ColorA = RojoFontA;
						break;
				case MonsterAffix.FireChains:
						ColorA = RojoFontA;
						break;
				case MonsterAffix.Teleporter:
						ColorA = TeleportFontA;
						break;
				case MonsterAffix.Mortar:
						ColorA = GrisFontA;
						break;
				case MonsterAffix.Poison:
						ColorA = VerdeFontA;
						break;
				case MonsterAffix.Plagued:
						ColorA = VerdeFontA;
						break;
				case MonsterAffix.Horde:
						ColorA = HordaFontA;
						break;
				default:
						ColorA = LightFont;
						break;

                }
           var d = ColorA.GetTextLayout(DisplayAffix[afijo]);
           ColorA.DrawText(d, coordx, coordy);
           return (coordx + LightFont.GetTextLayout("-").Metrics.Width + d.Metrics.Width) ;

        }

        private void DrawHealthBar(WorldLayer layer, IMonster m, ref float yref){
            if (m.Rarity == ActorRarity.RareMinion && !ShowRareMinions) return;     //no minions
            if (m.SummonerAcdDynamicId != 0) return;                                //no clones

            var wint = m.CurHealth / m.MaxHealth; string whptext;
	    if ((wint < 0) || (wint > 1)) {  wint = 1 ; whptext = "bug" ;  }
            else { whptext = (wint * 100).ToString(PercentageDescriptor) + "%";   }
            var w = wint * w2;
            var per = LightFont.GetTextLayout(whptext);

            var y = YPos + py * 8 * yref;
//            IBrush cBrush = null;
//            IFont cFont = null;
            cBrush = null;
            cFont = null;

            //Brush selection
            switch(m.Rarity){
                case ActorRarity.Boss:
                    cBrush = BossBrush;
                    break;
                case ActorRarity.Champion:
                    cBrush = ChampionBrush;
                    break;
                case ActorRarity.Rare:
                    cBrush = RareBrush;
                    break;
                case ActorRarity.RareMinion:
                    cBrush = RareMinionBrush;
                    break;
                default:
                    cBrush = BackgroundBrush;
                    break;
            }

            //Jugger Highlight
            if (JuggernautHighlight && m.Rarity == ActorRarity.Rare && HasAffix(m, MonsterAffix.Juggernaut)){
                cFont = RedFont;
                cBrush = RareJuggerBrush;
            }
            // else if (FrozenHighlight && m.Rarity == ActorRarity.Rare && HasAffix(m, MonsterAffix.Frozen)){
                // cFont = BlueFont;
                // BorderBrush = FrozenBorder;
            // }
			// else if (FrozenHighlight && m.Rarity == ActorRarity.Champion && HasAffix(m, MonsterAffix.Frozen)){
                // cFont = BlueFont;
				// BorderBrush = RegularBorder;
            // }
			else
                cFont = NameFont;
				BorderBrush = RegularBorder;

            //Missing Highlight
            if (MissingHighlight && (m.Rarity == ActorRarity.Champion || m.Rarity == ActorRarity.Rare) && !m.IsOnScreen){
                var missing = RedFont.GetTextLayout("\u26A0");
                RedFont.DrawText(missing, XPos - 17, y - py);
            }

            //Circle Non-Clones and Boss
            if (CircleNonIllusion && m.SummonerAcdDynamicId == 0 && HasAffix(m, MonsterAffix.Illusionist) || m.Rarity == ActorRarity.Boss && ShowBossHitBox)
                    HitBoxDecorator.Paint(layer, m, m.FloorCoordinate, string.Empty);

            string d = string.Empty;
			//Show Debuffs on Monster
            if (ShowDebuffAndCC){
                string textDebuff = null;
                //if (m.Locust) textDebuff += (textDebuff == null ? "" : ", ") + "Locust";
                //if (m.Palmed) textDebuff += (textDebuff == null ? "" : ", ") + "Palm";
                //if (m.Haunted) textDebuff += (textDebuff == null ? "" : ", ") + "Haunt";
                //if (m.MarkedForDeath) textDebuff += (textDebuff == null ? "" : ", ") + "Mark";
                //if (m.Strongarmed) textDebuff += (textDebuff == null ? "" : ", ") + "Strongarm"; // No funciona, reemplazado por otro código
				if (m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_2_Visual_Effect_None, 318772) == 1) //318772	2	power: ItemPassive_Unique_Ring_590_x1
					textDebuff += (textDebuff == null ? "" : ", ") + "Strongarm";
                string textCC = null;
                if (m.Frozen) textCC += (textCC == null ? "" : ", ") + "Frozen";
                if (m.Chilled) textCC += (textCC == null ? "" : ", ") + "Chilled";
                if (m.Slow) textCC += (textCC == null ? "" : ", ") + "Slow";
                if (m.Stunned) textCC += (textCC == null ? "" : ", ") + "Stunned";
                if (m.Invulnerable) textCC += (textCC == null ? "" : ", ") + "Invulnerable";
                if (m.Blind) textCC += (textCC == null ? "" : ", ") + "Blind";
				if (textDebuff != null) { d = textDebuff; }
				if (textCC != null) { d += ((d != string.Empty)? " | ":"") + textCC ; }

            }
			if (ShowCurses) {
					string Curses = null;
					if (m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_2_Visual_Effect_None, 471845) == 1) //471845 1 power: Frailty
						Curses += (Curses == null ? "" : " ") + "Cursed"; //Curses += (Curses == null ? "" : " ") + "저주"; 
					//if (m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_2_Visual_Effect_None, 471869) == 1)  //471869 1 power: Leech
						//Curses += (Curses == null ? "" : " ") + "L";
					// if (m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_2_Visual_Effect_None, 471738) == 1) //471738 1 power: Decrepify
						// Curses += (Curses == null ? "" : " ") + "";
					if (Curses != null) { d += ((d != string.Empty)? " | ":"") + Curses ; }
			}
			if (d != string.Empty) { LightFont.DrawText(LightFont.GetTextLayout(d), XPos + 65 + w2, y - py); }

            //Draw Rectangles
            BackgroundBrush.DrawRectangle(XPos, y, w2, h);
            BorderBrush.DrawRectangle(XPos, y, w2, h);
            cBrush.DrawRectangle(XPos, y, (float)w, h);
            LightFont.DrawText(per, XPos + 8 + w2, y - py);

            //Draw MonsterType
            if (ShowMonsterType){
                var name = cFont.GetTextLayout(m.SnoMonster.NameLocalized);
                cFont.DrawText(name, XPos + 3, y - py);
            }

            //increase linecount
            yref += 0.95f;
        }

        private void DrawPack(WorldLayer layer, IMonsterPack p, ref float yref){
            //Check if any affixes are wished to be displayed
            bool elitevivo = false;
            foreach(IMonster m in p.MonstersAlive) {
                 if ((m.Rarity == ActorRarity.Rare) || (m.Rarity == ActorRarity.Champion) ) { elitevivo = true; }
             }

            if (elitevivo || ShowRareMinions) {
            	if (DisplayAffix.Any() ){
                    var y = YPos + py * 8 * yref;  var despl = XPos; bool conafijos = false ;
             	       foreach(ISnoMonsterAffix afx in p.AffixSnoList) {        //iterate affix list
               	           if (DisplayAffix.Keys.Contains(afx.Affix))          //if affix is an key
              	              despl = AfijosColores (afx.Affix, despl , y - py); conafijos = true ;
              	       }
                    if (conafijos)  yref += 1.0f;
            	}
           	 //iterate all alive monsters of pack and print healthbars
           	 foreach(IMonster m in p.MonstersAlive)
            	    DrawHealthBar(layer, m, ref yref);
            }
        }

        private bool HasAffix(IMonster m, MonsterAffix afx){
            return m.AffixSnoList.Any(a => a.Affix == afx);
        }

        public void PaintWorld(WorldLayer layer)
        {
            //Spacing
            if ((OnlyGR) && (Hud.Game.Me.InGreaterRiftRank == 0)) return ;
            px = Hud.Window.Size.Width *  0.00155f * XScaling;
            py = Hud.Window.Size.Height * 0.001667f * YScaling;
            h = py * 6;
            w2 = px * 60;


            var packs = Hud.Game.MonsterPacks.Where(x => x.MonstersAlive.Any());
            var bosses = Hud.Game.AliveMonsters.Where(m => m.Rarity == ActorRarity.Boss);

            float yref = 0f;
            foreach(IMonster m in bosses)
                DrawHealthBar(layer, m, ref yref);
            yref += 0.5f;                                           //spacing between RG and Elites
            foreach(IMonsterPack p in packs){
                DrawPack(layer, p, ref yref);
                yref += 0.4f;                                       //spacing between Elites
            }
		}
    }

}
