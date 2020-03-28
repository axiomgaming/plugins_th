//plugin modified . Although it keeps the name GLQ, it is not the original, modified enough to adapt it to what I need

using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.glq
{

    public class GLQ_GreaterRiftPylonMarkerExtendedPlugin : BasePlugin, IInGameTopPainter
    {
        public IBrush ProgressionLineBrush { get; set; }
        public Dictionary<string, Tuple<double,string>> ProgressionofShrines { get; set; }
        public Dictionary<uint,string> Pylon_Buff { get; set; }
		public List<string> Pylon_State { get; set; }
        public IFont GreaterRiftFont { get; set; }
        public IFont GreaterRiftFont2 { get; set; }
		public IFont GreaterRiftFontUsed { get; set; }

        public GLQ_GreaterRiftPylonMarkerExtendedPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            GreaterRiftFont = Hud.Render.CreateFont("tahoma", 6, 255, 215, 110, 215, false, false, 160, 0, 0, 0, true);
            GreaterRiftFont2 = Hud.Render.CreateFont("tahoma", 6, 255, 250, 150, 250, false, false, 160, 0, 0, 0, true);
			GreaterRiftFontUsed = Hud.Render.CreateFont("tahoma", 6, 255, 200, 200, 200, false, false, 250, 0, 0, 0, true);
            ProgressionLineBrush = Hud.Render.CreateBrush(255, 125, 175, 240, 1f);
            ProgressionofShrines = new Dictionary<string, Tuple<double,string>>();
			Pylon_State = new List<String> {};
            Pylon_Buff = new Dictionary<uint, string>();
            Pylon_Buff.Add(262935,"Power"); Pylon_Buff.Add(266258,"Channeling"); Pylon_Buff.Add(266254,"Shield");
            Pylon_Buff.Add(266271,"Speed"); Pylon_Buff.Add(403404,"Conduit"); 
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;

            if (Hud.Game.SpecialArea == SpecialArea.GreaterRift)
            {
                var percent = Hud.Game.RiftPercentage;
                if (percent <= 0)  {
                    ProgressionofShrines.Clear();
					Pylon_State.Clear();
                    return;
                }
                var ui = Hud.Render.GreaterRiftBarUiElement;

                if (ui.Visible)  {
                    var uiRect = ui.Rectangle;               
                    var shrines = Hud.Game.Shrines;
                    Tuple<double,string> valuesOut; 
                    var poder = "";
                    foreach (var actor in shrines)  { 
                        switch ((uint)actor.SnoActor.Sno)   {                           
                            case 330695: poder = "Power";  break;
                            case 330696:
                            case 398654: poder = "Conduit";  break;
                            case 330697: poder = "Channeling";  break;
                            case 330698: poder = "Shield";  break;
                            case 330699: poder = "Speed";  break;
                            default: poder = actor.SnoActor.Sno.ToString("F0");  break;
                        }
// Para parchear que Hud.Game.Me.SnoArea.NameLocalized no se actualice inmediatamente al entrar en un mapa. Hay problemas si el pilón está cerca de la puerta
                        if  (ProgressionofShrines.TryGetValue(poder, out valuesOut)) {     
                             if (valuesOut.Item2 != Hud.Game.Me.SnoArea.NameEnglish) { 
                               Tuple<double,string> updateValues = new Tuple<double,string>(valuesOut.Item1, Hud.Game.Me.SnoArea.NameEnglish);
                               ProgressionofShrines[poder] = updateValues;
                            }
                        }
                        else {
                                Tuple<double,string> updateValues = new Tuple<double,string>(percent, Hud.Game.Me.SnoArea.NameEnglish);
                                ProgressionofShrines.Add(poder, updateValues);
                        }
						if (actor.IsDisabled || actor.IsOperated) { 
						 if (!Pylon_State.Contains(poder)) { Pylon_State.Add(poder); }
						}
                        
                                  
                   }
                   
                   Dictionary<uint,string>.KeyCollection listabufos = Pylon_Buff.Keys;
                   foreach (uint bufo in listabufos)  { 
                       if (Hud.Game.Me.Powers.BuffIsActive(bufo, 0)) {
                           if (!Pylon_State.Contains(Pylon_Buff[bufo])) { Pylon_State.Add(Pylon_Buff[bufo]); }
                       }
                   }

                   var py = Hud.Window.Size.Height / 30 ; var anterior = 0d; var contador = 0; var text = "";
                   var ancho = (GreaterRiftFont.GetTextLayout("00,0%")).Metrics.Width ; var alto =  (GreaterRiftFont.GetTextLayout("00,0%")).Metrics.Height;
					Dictionary<string, Tuple<double,string>>.KeyCollection keyColl = ProgressionofShrines.Keys;

                   foreach (string s in keyColl)  { 
                        if  (ProgressionofShrines.TryGetValue(s, out valuesOut)) {
                             contador += 1 ; 
                            
                             double porcentaje = valuesOut.Item1; //porcentaje al que salio el pilon
                             string piso = valuesOut.Item2.Replace("[TEMP] Loot Run Level","Layer");; //piso en el que salio el pilon

                             var xPos = (float)( uiRect.Left + uiRect.Width / 100.0f * porcentaje);
                            
                             ProgressionLineBrush.DrawLine(xPos, uiRect.Bottom, xPos, uiRect.Bottom + py, 0); 
                             var PilonApilon = (contador > 1) ? ("+" + (porcentaje - anterior).ToString("F0") + "%") : "";
                             text = porcentaje.ToString("F1") + "%" + "\r\n" + s + "\r\n"+ piso + "\r\n" + PilonApilon  ;
                             anterior = porcentaje;
							 if (Pylon_State.Contains(s)) { GreaterRiftFontUsed.DrawText(text, xPos - ancho , uiRect.Bottom + py, true); }
                             else { GreaterRiftFont.DrawText(text, xPos - ancho , uiRect.Bottom + py, true); }

                             if ( ( contador != 4) && (contador == ProgressionofShrines.Count) )  {
                                 var DesdePîlon = "+" + (percent - porcentaje).ToString("F1") + "%";
                                 GreaterRiftFont2.DrawText(DesdePîlon, uiRect.Left + uiRect.Width /2 - GreaterRiftFont2.GetTextLayout(DesdePîlon).Metrics.Width / 2 , uiRect.Bottom + uiRect.Height * 0.2f, true);

                             }

                        }  
                   }
                }
            }
        }

    }
}