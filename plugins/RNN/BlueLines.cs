using System.Collections.Generic;
using Turbo.Plugins.Default;
using System.Linq;
using System;

namespace Turbo.Plugins.RNN
{
    public class BlueLines : BasePlugin, IInGameWorldPainter
    {
		private List<IWorldCoordinate> CoordElites { get; set; }
		private Dictionary<IMonsterPack,int> PackColor { get; set;}
		private IBrush BlueBrush1 { get; set; }		
		private IBrush BlueBrush2 { get; set; }
		private IBrush BlueBrush3 { get; set; }
		private IBrush BlueBrush4 { get; set; }
		private IBrush BlueBrush5 { get; set; }
			
		public bool InGR_On { get; set; }
		public bool OutGR_On { get; set; }
		
        public BlueLines()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			Order = -50;
			CoordElites = new List<IWorldCoordinate>();
			PackColor = new Dictionary<IMonsterPack,int>();

			InGR_On = true; 	// Draw in GR 
			OutGR_On = true;	// Draw out GR
					
			BlueBrush1 = Hud.Render.CreateBrush(200, 0, 50, 250, 2, SharpDX.Direct2D1.DashStyle.Solid, SharpDX.Direct2D1.CapStyle.Flat, SharpDX.Direct2D1.CapStyle.Triangle);				
			BlueBrush2 = Hud.Render.CreateBrush(200, 0, 250, 50, 2, SharpDX.Direct2D1.DashStyle.Solid, SharpDX.Direct2D1.CapStyle.Flat, SharpDX.Direct2D1.CapStyle.Triangle);
			BlueBrush3 = Hud.Render.CreateBrush(200, 250, 150, 50, 2, SharpDX.Direct2D1.DashStyle.Solid, SharpDX.Direct2D1.CapStyle.Flat, SharpDX.Direct2D1.CapStyle.Triangle);			
			BlueBrush4 = Hud.Render.CreateBrush(200, 220, 50, 220, 2, SharpDX.Direct2D1.DashStyle.Solid, SharpDX.Direct2D1.CapStyle.Flat, SharpDX.Direct2D1.CapStyle.Triangle);
			BlueBrush5 = Hud.Render.CreateBrush(200, 250, 0, 0, 2, SharpDX.Direct2D1.DashStyle.Solid, SharpDX.Direct2D1.CapStyle.Flat, SharpDX.Direct2D1.CapStyle.Triangle);	
        }
		
// Método estricto para intentar conservar colores asignados.
		private IBrush GETBRUSHn(IMonsterPack pack, ref IEnumerable<IMonsterPack> packs) {  
			int c = 0;
			if (PackColor.ContainsKey(pack)) { c = PackColor[pack]; }  // Si es un pack ya registrado le seguimos asignando el mismo color
			else {  // Aquí solo entrará ocasionalmente, cuando se detecte un pack nuevo u otro pack que ya no tiene ningún color asignado (porque lo dejamos atrás y se liberó el color que usaba)		
				List<int> lc1 = new List<int> {1,2,3,4,5}; // para obtener la lista de colores que no usa ningun pack registrado
				List<int> lc2 = new List<int> {1,2,3,4,5}; // para obtener la lista de colores que no usa ningun pack cercano
				List<IMonsterPack> TmpList = new List<IMonsterPack>();
				foreach (var p in PackColor.Keys) {     
					int a = PackColor[p];
					if (lc1.Contains(a)) { lc1.Remove(a); }
					if (!packs.Contains(p)) { TmpList.Add(p); }
					else  if (lc2.Contains(a)) { lc2.Remove(a); }					
				}
				if (lc1.Count() > 0) c = lc1[0] ;  //si hay algún color no usado en los pack registrados, asignaremos primero éste
				else if  (lc2.Count() > 0) 					
				{  			
					TmpList = TmpList.OrderByDescending(x => x.LastActive.ElapsedMilliseconds).ToList(); 
					foreach (var p in TmpList) 	
					{
						int a1 = PackColor[p];
						if (lc2.Contains(a1)) {
							if (c == 0) c = a1;
							if (c == a1) { PackColor.Remove(p); }
						}
					}											
				}
				else { c = 5; } // Se repetirá con el color 5 cuando no haya colores libres disponibles
				if (c == 0) { c = 5 ; /* Hud.Sound.Speak( "Bug en BlueLines" ); */ }
				PackColor.Add(pack,c);	
			}
			return (c == 1)? BlueBrush1 : (  (c == 2)? BlueBrush2 : ((c == 3)? BlueBrush3 : ((c == 4)? BlueBrush4:BlueBrush5))  );
		}
		
		public void PaintWorld(WorldLayer layer)
        {
			if (Hud.Game.IsInTown) return;
			if (Hud.Game.SpecialArea == SpecialArea.GreaterRift) {
				if (!InGR_On) return;
			}
			else if (!OutGR_On )  return;
												
			var packs = Hud.Game.MonsterPacks.Where(x => x.MonstersAlive.Any() && x.IsFullChampionPack); 
			
			foreach(IMonsterPack p in packs) {					
				foreach (IMonster m in p.MonstersAlive)
				{			// if (m.SummonerAcdDynamicId == 0) not enough, add check extra for patch bug when take pylon and Champion near			
					if ( (m.Rarity == ActorRarity.Champion) && (m.SummonerAcdDynamicId == 0)) 
					{ 	
						CoordElites.Add(m.FloorCoordinate);													
					}					
				}
				var n = CoordElites.Count(); 
				if (n > 1)
				{
					if (n > 2)
					{
						CoordElites = CoordElites.OrderBy(x => x.ToScreenCoordinate().X).ToList();
					}
					 					
					for (var i = 1; i < n ; i++) 
					{							
						GETBRUSHn(p, ref packs).DrawLineWorld(CoordElites[i-1],CoordElites[i]);
					}
				}
				if (n != 0) CoordElites.Clear();											
			}					
        }
    }
}