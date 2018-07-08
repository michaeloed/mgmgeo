using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Configuration;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using SpaceEyeTools.EXControls;
using MyGeocachingManager.HMI;
using MyGeocachingManager.Geocaching;
using System.Collections;
using System.Net;

namespace MyGeocachingManager
{
	public class waypointtocache : IScript
	{
		private MainWindow _Daddy = null;
		public string Name {get {return "Copier le waypoint final";}}
		public string Description { get {return "Copier les coordonnées du waypoint final dans la cache";}}
		
		public void Initialize(MainWindow daddy)
		{
			_Daddy = daddy;
		}
				
		public bool DoIt()
		{
			if (_Daddy != null)
			{
				var caches = _Daddy.GetSelectedCaches();
                if (caches.Count == 0)
                    return false;
                
                int nbmodif = 0;
                foreach(Geocache geo in caches)
                {
                	// On cherche un waypoint commencant par FN (final)
                	String code_cherche = "FN" + geo._Code.Substring(2);
                	if (geo._waypoints.ContainsKey(code_cherche))
                	{
                		// On met à jour les coord avec celles de ce waypoint
                		Waypoint wpt = geo._waypoints[code_cherche];
                		
                		geo._Latitude = wpt._lat;
                        geo._Longitude = wpt._lon;
                        geo._dLatitude = MyTools.ConvertToDouble(wpt._lat);
                        geo._dLongitude = MyTools.ConvertToDouble(wpt._lon);
                        geo.UpdateDistanceToHome(_Daddy.HomeLat, _Daddy.HomeLon);
                        _Daddy._iNbModifiedCaches += geo.InsertModification("COORD");
                        nbmodif++;
                	}
                }
                
                // Better way to do that : only recreate for modified caches
                _Daddy.RecreateVisualElements(caches);

                // On redessine les caches sur la carte !
                if (nbmodif != 0)
                {
                	// On redessine la carte
            		_Daddy.BuildCacheMapNew(_Daddy.GetDisplayedCaches());
                }
				return true;
			}
			else
			{
				MessageBox.Show("C'est bon !");
				return false;
			}
		}
		
		public void Close()
		{
		}
	}	
}