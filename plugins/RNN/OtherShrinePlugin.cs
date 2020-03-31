using Turbo.Plugins.Default;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Turbo.Plugins.RNN
{
    public class OtherShrinePlugin : BasePlugin, ICustomizer, IInGameWorldPainter, INewAreaHandler, IAfterCollectHandler
    {
		private WorldDecoratorCollection DecoratorRectangle { get; set; }
		private WorldDecoratorCollection DecoratorRectangleGray { get; set; }
		private WorldDecoratorCollection DecoratorCircle { get; set; }
		private WorldDecoratorCollection DecoratorCircleGray { get; set; }
		private WorldDecoratorCollection DecoratorHealing { get; set; }
		private WorldDecoratorCollection DecoratorDefault { get; set; }
		private WorldDecoratorCollection LinePylon { get; set; }
		private WorldDecoratorCollection LinePool { get; set; }

		private TopLabelDecorator DecoratorPylonNemesys { get; set; }
		private TopLabelDecorator DecoratorPopupON { get; set; }
		private TopLabelDecorator DecoratorPopupOFF { get; set; }

		private string PylonNemesys { get; set;} = string.Empty;
		private string TextoPopup { get; set; }	= string.Empty;
		private IFont TextoFont { get; set; }

		private Dictionary<string, int> ShrineCoordReapp { get; set; } = new Dictionary<string, int>();
		private Dictionary<uint, ShrineType> PylonBuffType { get; set; }
		private Dictionary<ShrineType, ShrineData> ShrinesDic { get; set; } = new Dictionary<ShrineType, ShrineData>();
		private List<ShrineType> PylonsGR { get; set; } = new List<ShrineType>() { ShrineType.PowerPylon, ShrineType.ConduitPylon, ShrineType.ChannelingPylon, ShrineType.ShieldPylon, ShrineType.SpeedPylon };
		private int PGRn { get; set; }
		private int MyIndex { get; set; } = -1;
		private bool InRift { get; set; }

		public bool NotifyInTown { get; set; }
		public bool TTSViewPylon { get; set;}
		public bool TTSBuffPylon { get; set;}
		public string TTSViewPoolText { get; set;}

		public string LabelHealingWells	 { get; set; }
		public string LabelPoolOfReflection { get; set; }
		public string LabelPossiblePylon { get; set; }
		public bool LabelPylonExchange { get; set;}

		public bool HiddenPylonUsed { get; set;}
		public bool LineToPylon { get; set;}
		public float LineToPylonWidth { get; set;}

    public bool CircleHealingWells { get; set;}
    public bool CirclePoolOfReflection { get; set;}
		public bool ShowPylonSpawn { get; set; }
		public bool ShowPopup { get; set;}

		public float xPopup { get; set;}
		public float yPopup { get; set;}
		public float FontSizePopup { get; set; }

		public class ShrineData
		{
			public string LabelG { get; set; }
			public string LabelM { get; set; }
			public string TTSView { get; set; }
			public string TTSBuffON { get; set; }
			public string TTSBuffOFF { get; set; }
			public string PopupBuff { get; set; }
			public bool GR { get; set; } = false;
			public bool Buff { get; set; } = false;
			public int Finish { get; set; } = 0;

			public void Set(string labelg, string labelm, string ttsview  , string ttson , string ttsoff, string popup)
			{
				this.LabelG = labelg;
				this.LabelM = labelm;
				this.TTSView = ttsview;
				this.TTSBuffON = ttson;
				this.TTSBuffOFF = ttsoff;
				this.PopupBuff = popup;

			}
			public void Reset()
			{
				this.GR = false;
				this.Buff = false;
				this.Finish = 0;
			}
		}

		public class PopupPylon : IQueueItem
        {
			public bool On { get; set; }
            public string Text { get; set; }
            public DateTime QueuedOn { get; private set; }
            public TimeSpan LifeTime { get; private set; }

            public PopupPylon(bool on, string text, TimeSpan duration)
            {
				this.On = on;
                this.Text = text;
                this.LifeTime = duration;
                this.QueuedOn = DateTime.Now;
            }
        }

        public void Config(ShrineType t, string labelg, string labelm, string ttsview  , string ttson , string ttsoff, string popup)
		{
			ShrinesDic[t].Set(labelg, labelm, ttsview, ttson, ttsoff, popup);
        }

        public OtherShrinePlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			Order = 30001;

			LabelHealingWells = ""; 		// Texto en el minimapa para los pozos , null namelocalized, "" nothing
			LabelPoolOfReflection = "Pool"; 	// Texto en el minimapa para los pozos , null namelocalized, "" nothing
			LabelPossiblePylon = "Pylon?";  // Texto para lugares donde pueden aparecer posibles pilones.
			LabelPylonExchange = false;  	// Intercambiar etiquetas en mapa y minimpa. Si es false se mostrarán siempre

			NotifyInTown = false;  	     // TTS/Popup in Town
			TTSViewPylon = true; 	 	// Notificar con voz (TTS) Los Pilones y Santuarios
			TTSBuffPylon = true;  	 	// Notificar con voz (TTS) Cuando se recibe un bufo de un pilón/Santuario
			TTSViewPoolText = "Pool";   // Notificar con voz (TTS) Los pools of Reflection , leerá el texto indicado. Set to "" (or null) for not TTS

			HiddenPylonUsed = false; 	 // Ocultar o mostrar (en gris) las etiquetas de los pilones usados
			LineToPylon = true;  		 // Dibuja una línea amarilla en el minimapa hasta el pozo o pilón/santuario detectado
			LineToPylonWidth = 2f;

			CircleHealingWells = true; 	 // Ver Círculos rojos en los pozos no utilizados
			CirclePoolOfReflection = true; // Ver Círculos amarillos en los pozos no utilizados
			ShowPylonSpawn = true;
			ShowPopup = true;     // Mostrar un Popup cuando se recibe un Buff de un pilón

			xPopup = 0.5f;       // 0f ... 1f
			yPopup = 0.81f;       // 0f ... 1f
			FontSizePopup = 9f;

			// ShrineType.PoolOfReflection, ShrineType.HealingWell
			// ShrineType.PowerPylon, ShrineType.ConduitPylon, ShrineType.ChannelingPylon, ShrineType.ShieldPylon, ShrineType.SpeedPylon
			// ShrineType.BlessedShrine, ShrineType.EnlightenedShrine, ShrineType.FortuneShrine, ShrineType.FrenziedShrine, ShrineType.FleetingShrine, ShrineType.EmpoweredShrine
			// ShrineType.BanditShrine

			PylonBuffType = new Dictionary<uint, ShrineType>
			{
				{262935,ShrineType.PowerPylon}, {403404,ShrineType.ConduitPylon}, {266258,ShrineType.ChannelingPylon}, {266254,ShrineType.ShieldPylon}, {266271,ShrineType.SpeedPylon}, // Power, Conduit, Chann, Shield, Speed
				{278268,ShrineType.BlessedShrine}, {030476,ShrineType.BlessedShrine}, {278269,ShrineType.EnlightenedShrine}, {030477,ShrineType.EnlightenedShrine}, {263029,ShrineType.ConduitPylon}, // protección, experiencia
				{278270,ShrineType.FortuneShrine}, {030478,ShrineType.FortuneShrine}, {278271,ShrineType.FrenziedShrine}, {030479,ShrineType.FrenziedShrine}, {260348,ShrineType.FleetingShrine}, // fortuna, frenético, fugaz
				{260349,ShrineType.EmpoweredShrine}	 // potenciado
			};

			foreach (ShrineType shrine in Enum.GetValues(typeof(ShrineType)))
			{
				string msg = shrine.ToString().Replace("Shrine","").Replace("Pylon","") ;
				ShrinesDic.Add(	shrine,new ShrineData());
				ShrinesDic[shrine].Set(null, null, null, msg + " Active",msg + " Lost", msg);
			}

			TextoFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 100, 100, true, false, false);
			DecoratorDefault = new WorldDecoratorCollection ();

			DecoratorPylonNemesys = new TopLabelDecorator(Hud)
			{
				TextFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 50, true, false, false),
				BackgroundTexture1 = Hud.Texture.ButtonTextureOrange,
				BackgroundTexture2 = Hud.Texture.BackgroundTextureOrange,
				BackgroundTextureOpacity1 = 1.0f,
				BackgroundTextureOpacity2 = 1.0f,
				TextFunc = () =>  PylonNemesys,
				HintFunc = () => "Players with Nemesys",
			};

			DecoratorRectangle = new WorldDecoratorCollection(
				new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 255, 64, 2),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 4.0f,
                    ShapePainter = new RectangleShapePainter(Hud),
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 192, 255, 255, 55, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 5f,
                },
				new GroundLabelDecorator(Hud)
                {
                    BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0),
                    BorderBrush = Hud.Render.CreateBrush(192, 255, 255, 55, 1),
                    TextFont = Hud.Render.CreateFont("tahoma", 7f, 192, 255, 255, 55, false, false, 128, 0, 0, 0, true),
                }
			);

			DecoratorRectangleGray = new WorldDecoratorCollection(
				new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 150, 150, 150, 2),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 4.0f,
                    ShapePainter = new RectangleShapePainter(Hud),
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 200, 155, 155, 155, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 5f,
                },
				new GroundLabelDecorator(Hud)
                {
                    BackgroundBrush = Hud.Render.CreateBrush(255, 20, 20, 20, 0),
                    BorderBrush = Hud.Render.CreateBrush(192, 150, 150, 150, 1),
                    TextFont = Hud.Render.CreateFont("tahoma", 7f, 200, 155, 155, 155, false, false, 128, 0, 0, 0, true),
                }
			);

			DecoratorCircle = new WorldDecoratorCollection(  //No aplicar un ToggleDecorators directamente o habrá que modificar la parte del PoolOfReflection
				new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 255, 64, 2),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 4.0f,
                    ShapePainter = new CircleShapePainter(Hud),
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 192, 255, 255, 55, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 5f,
                }
			);

			DecoratorCircleGray = new WorldDecoratorCollection(   //No aplicar un ToggleDecorators directamente o habrá que modificar la parte del PoolOfReflection
				new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 150, 150, 150, 2),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 4.0f,
                    ShapePainter = new CircleShapePainter(Hud),
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 200, 155, 155, 155, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 5f,
                }
			);

			DecoratorHealing = new WorldDecoratorCollection(
				new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 64, 64, 2),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 4.0f,
                    ShapePainter = new CircleShapePainter(Hud),
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 220, 255, 64, 64, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 5f,
                }
			);
		}

        public void Customize()
        {
            Hud.TogglePlugin<ShrinePlugin>(false);
			LinePylon = new WorldDecoratorCollection(
                new MapShapeDecorator(Hud)
                {
					Brush = Hud.Render.CreateBrush(255, 255, 200, 50, LineToPylonWidth,SharpDX.Direct2D1.DashStyle.Dash),
                    ShapePainter = new LineFromMeShapePainter(Hud)
                }
            );
			LinePool = new WorldDecoratorCollection(
                new MapShapeDecorator(Hud)
                {
					Brush = Hud.Render.CreateBrush(255, 255, 255, 0, LineToPylonWidth,SharpDX.Direct2D1.DashStyle.Dash),
                    ShapePainter = new LineFromMeShapePainter(Hud)
                }
            );
			DecoratorPopupON = new TopLabelDecorator(Hud)
			{
                BorderBrush = Hud.Render.CreateBrush(255, 180, 147, 109, -1),
                BackgroundBrush = Hud.Render.CreateBrush(200, 10, 10, 10, 0),
                TextFont = Hud.Render.CreateFont("tahoma", FontSizePopup, 255, 0, 250, 0, true, false, false),
				TextFunc = () =>  TextoPopup
            };
			DecoratorPopupOFF = new TopLabelDecorator(Hud)
			{
                BorderBrush = Hud.Render.CreateBrush(255, 180, 147, 109, -1),
                BackgroundBrush = Hud.Render.CreateBrush(200, 10, 10, 10, 0),
                TextFont = Hud.Render.CreateFont("tahoma", FontSizePopup, 255, 250, 0, 0, true, false, false),
				TextFunc = () =>  TextoPopup
            };
        }

		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame || (MyIndex != Hud.Game.Me.Index) )   // Fix partialment the newGame limitation
			{
				MyIndex = Hud.Game.Me.Index;
				InRift = false;
				foreach (var p in ShrinesDic.Keys)	{ ShrinesDic[p].Reset(); }
			}
			if ((area.HostAreaSno == 288482) || (area.Sno == 288482))
			{
				if (!InRift) 		//Start Map New Rift (Nephalem o GR)
				{
					InRift = true;
					if (Hud.Game.Me.InGreaterRift) // new GR
					{
						PGRn = 0 ;
						foreach (var p in ShrinesDic.Keys)	{ ShrinesDic[p].Reset();  }
					}
				}
			}
			else
			{
				if (Hud.Game.IsInTown) { ShrineCoordReapp.Clear(); }
			}
		}

		public void AfterCollect()
		{
			if (InRift && (Hud.Game.Quests.FirstOrDefault(q => (q.SnoQuest.Sno == 337492) && (q.State == QuestState.started || q.State == QuestState.completed )) == null))	{ InRift = false; }
			if (!Hud.Game.IsInGame) return;
			var shrines = Hud.Game.Shrines.Where(s => s.DisplayOnOverlay && s.FloorCoordinate.IsValid && (s.Type != ShrineType.HealingWell) );
			foreach (var shrine in shrines)
			{
				string shrineName = string.Empty; int Reappears = 0;
				string coord = shrine.FloorCoordinate.ToString() ;
				if (ShrineCoordReapp.TryGetValue(coord, out Reappears) )
				{
					if (Reappears < 100) {  // Limited to 100 to avoid overflow. I continue counting up to 100 for curiosity and debug purposes
						Reappears++;
						if (Reappears == 1) // Required reappearances to differentiate it from a fake shrine.
						{
							if (shrine.Type == ShrineType.PoolOfReflection)
							{
								shrineName = (string.IsNullOrEmpty(TTSViewPoolText))? string.Empty: TTSViewPoolText ;
							}
							else
							{
								if ( PylonsGR.Contains(shrine.Type) && !ShrinesDic[shrine.Type].GR )
								{
									ShrinesDic[shrine.Type].GR = true;
									PGRn++;
								}
								shrineName = (TTSViewPylon)? ShrinesDic[shrine.Type].TTSView: string.Empty;
							}
							if (shrineName != string.Empty)
							{
                //pylon discovery
								Hud.Sound.Speak(shrineName ?? shrine.SnoActor.NameLocalized); // if == null it will be used localized name for that shrine , if == "" (string.Empty) no TTS for that shrine
							}
						}
						ShrineCoordReapp[coord] = Reappears;
					}
				}
				else ShrineCoordReapp.Add(coord,0); // new shrine detected -> saving (coordinate,reappears)
			}
			foreach (uint buff in PylonBuffType.Keys)
			{
				if (Hud.Game.Me.Powers.GetBuff(buff) == null) continue;
				var stype = PylonBuffType[buff];
				if ( Hud.Game.Me.Powers.BuffIsActive(buff,0)  )
				{
					var bfinish = Hud.Game.CurrentGameTick + (int) (60 * Hud.Game.Me.Powers.GetBuff(buff).TimeLeftSeconds[0]);
					if ( !ShrinesDic[stype].Buff || (bfinish > (ShrinesDic[stype].Finish + 60)) )
					{
						if (Hud.Game.Me.Powers.GetBuff(buff).TimeLeftSeconds[0] == 0)	{ /* Hud.Sound.Speak("Timeleft es cero"); */ continue; } // Patch Posible bug. Recibes bufo y TimeLeftSeconds es 0, ¿no inicializado aún?
						ShrinesDic[stype].Buff = true;
						ShrinesDic[stype].Finish = bfinish;
						if (Hud.Game.Me.Powers.GetBuff(buff).TimeElapsedSeconds[0] < 1)
						{
							var msg = ShrinesDic[stype].TTSBuffON;
							if (TTSBuffPylon && !string.IsNullOrEmpty(msg)) 	// null or "" -> no TTS
							{
								if (NotifyInTown || !Hud.Game.IsInTown) { Hud.Sound.Speak(msg);	} //pylon pickup
							}
							msg = ShrinesDic[stype].PopupBuff;
							if (ShowPopup && !string.IsNullOrEmpty(msg))  // null or "" -> no popup
							{
								if (NotifyInTown || !Hud.Game.IsInTown) { Hud.Queue.AddItem( new PopupPylon(true, msg, new TimeSpan(0, 0, 0, 0, 3000)) ); }
							}
						}
						if ( PylonsGR.Contains(stype) && !ShrinesDic[stype].GR )
						{
							PGRn++;
							ShrinesDic[stype].GR = true;
						}
					}
				}
				else if (ShrinesDic[stype].Buff)
				{
					var t = Hud.Game.Me.Powers.GetBuff(buff).LastActive.ElapsedMilliseconds;
					if ( (Hud.Game.CurrentGameTick > ShrinesDic[stype].Finish) || ((t > 100) && (t < 1000)) )  // Patch, erratic behavior BuffIsActive/LastActiveIwatch
					{
						ShrinesDic[stype].Buff = false;
						ShrinesDic[stype].Finish = Hud.Game.CurrentGameTick - 1;
						var msg = ShrinesDic[stype].TTSBuffOFF;
						if (TTSBuffPylon && !string.IsNullOrEmpty(msg))
						{
							if (NotifyInTown || !Hud.Game.IsInTown)		{  //pylon ends
                Hud.Sound.Speak(msg);
              }
						}
						msg = ShrinesDic[stype].PopupBuff;
						if (ShowPopup && !string.IsNullOrEmpty(msg))
						{
							if (NotifyInTown || !Hud.Game.IsInTown)		{ Hud.Queue.AddItem(new PopupPylon(false, msg, new TimeSpan(0, 0, 0, 0, 3000))); }
						}
					}
				}
			}
		}

        public void PaintWorld(WorldLayer layer)
        {
			if (!Hud.Game.IsInGame) return;
			if (layer != WorldLayer.Ground) return;
			if (!Hud.Game.IsInTown)
			{
				foreach (var shrine in Hud.Game.Shrines) // No usaré var shrines = Hud.Game.Shrines.Where(s => s.DisplayOnOverlay); // !Disabled && !Operated
				{
					if (shrine.Type == ShrineType.HealingWell)
					{
						if (shrine.DisplayOnOverlay)
						{
							if (CircleHealingWells) DecoratorHealing.Paint(WorldLayer.Map, shrine, shrine.FloorCoordinate, LabelHealingWells ?? shrine.SnoActor.NameLocalized);
						}
					}
					else
					{
						if (shrine.Type == ShrineType.PoolOfReflection)
						{
							if (shrine.DisplayOnOverlay)
							{
								if (CirclePoolOfReflection) DecoratorCircle.Paint(WorldLayer.Map, shrine, shrine.FloorCoordinate, LabelPoolOfReflection ?? shrine.SnoActor.NameLocalized);
								if (LineToPylon) LinePool.Paint(WorldLayer.Map, null, shrine.FloorCoordinate, "");
							}
						}
						else
						{
							if (shrine.DisplayOnOverlay || !HiddenPylonUsed)
							{
								bool cerca = (shrine.FloorCoordinate.IsValid && shrine.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) < 35)? true: false;
								DecoratorDefault = (shrine.DisplayOnOverlay)? DecoratorRectangle : DecoratorRectangleGray ;
								if ((!LabelPylonExchange && shrine.IsOnScreen) || cerca) //Mostrar GroundLabelDecorator cuando estás cerca
								{
									DecoratorDefault.Paint(WorldLayer.Ground, shrine, shrine.FloorCoordinate, (ShrinesDic[shrine.Type].LabelG)?? shrine.SnoActor.NameLocalized);
								}
								if (!LabelPylonExchange || !cerca) // Mostrar MapLabelDecorator cuando no estás cerca
								{
									DecoratorDefault.Paint(WorldLayer.Map, shrine, shrine.FloorCoordinate, (ShrinesDic[shrine.Type].LabelM)?? shrine.SnoActor.NameLocalized);
								}
								if (shrine.DisplayOnOverlay)
								{
									var players = Hud.Game.Players.Where(p => (p.BattleTagAbovePortrait != string.Empty) ); PylonNemesys = string.Empty;
									foreach (var player in players)
									{
										var Nemesis = player.Powers.GetBuff(318820);
										if ( (Nemesis != null) && Nemesis.Active)
										{
											if (player.IsMe) { PylonNemesys = string.Empty; break; }
											PylonNemesys += ((PylonNemesys == string.Empty)? "":"\n") + player.BattleTagAbovePortrait ;
										}
									}
									if (!string.IsNullOrEmpty(PylonNemesys))
									{
										if(PylonNemesys.IndexOf('\n') == -1) PylonNemesys = PylonNemesys + "\n";
										var l = TextoFont.GetTextLayout(PylonNemesys) ;
										DecoratorPylonNemesys.Paint(shrine.ScreenCoordinate.X - (int) (l.Metrics.Width * 0.75) , shrine.ScreenCoordinate.Y , l.Metrics.Width * 1.5f, l.Metrics.Height * 1.4f, HorizontalAlign.Center);
									}
									if (LineToPylon) LinePylon.Paint(WorldLayer.Map, null, shrine.FloorCoordinate, "");
								}
							}
						}
					}
				}
				if (ShowPylonSpawn)
				{
					var riftPylonSpawnPoints = Hud.Game.Actors.Where(s => s.SnoActor.Sno == ActorSnoEnum._markerlocation_tieredriftpylon);
					foreach (var actor in riftPylonSpawnPoints)
					{
						bool HayPylon;
						if ((Hud.Game.RiftPercentage < 100) && (PGRn < 4))
						{
							DecoratorDefault = DecoratorRectangle;  //DecoratorDefault = DecoratorRectangle; (for rectangle) or DecoratorDefault = DecoratorCircle; (for circle)
							HayPylon = true;
						}
						else
						{
							DecoratorDefault = DecoratorRectangleGray; // DecoratorDefault = DecoratorRectangleGray; (for rectangle) or DecoratorDefault = DecoratorCircleGray; (for circle)
							HayPylon = false;
						}
						DecoratorDefault.ToggleDecorators<MapLabelDecorator>(HayPylon); // replace HayPylon with true for show always the label
						DecoratorDefault.Paint(WorldLayer.Map, actor, actor.FloorCoordinate, LabelPossiblePylon );
					}
				}
			}
			if (NotifyInTown || !Hud.Game.IsInTown)
			{
				float h = FontSizePopup + Hud.Window.Size.Height * 0.04f;
				float y = Hud.Window.Size.Height * yPopup - h / 2;
				foreach (PopupPylon p in Hud.Queue.GetItems<PopupPylon>().Take(7))
				{
					TextoPopup = p.Text;
					float w = p.Text.Length*FontSizePopup + Hud.Window.Size.Width * 0.04f;
					float x = Hud.Window.Size.Width * xPopup - w / 2;
					if (p.On) { DecoratorPopupON.Paint(x, y , w, h, HorizontalAlign.Center); }
					else { DecoratorPopupOFF.Paint(x, y , w, h, HorizontalAlign.Center); }
					if (y < Hud.Window.Size.Height/2) y += h;
					else y -= h;
				}
			}
		}
	}
}
