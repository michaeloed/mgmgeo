using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MyGeocachingManager.Geocaching;
using GMap.NET;
using SpaceEyeTools;

namespace MyGeocachingManager.HMI
{
	/// <summary>
	/// Description of BrowserWrappe.
	/// </summary>
	public class BrowserWrapper : WebBrowser
	{
		MainWindow _daddy = null;
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="daddy"></param>
		public BrowserWrapper(MainWindow daddy)
		{
			_daddy = daddy;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnNavigating(WebBrowserNavigatingEventArgs e)
		{
			String url = e.Url.ToString().ToLower();
			if (url.StartsWith("mgmgeo:"))
        	{
				String code = url.Replace("mgmgeo:","");
				code = code.Replace("/","");
				code = code.ToUpper();
				try
	            {
	                Geocache geo = _daddy._caches[code];
	                OfflineCacheData ocd = null;
	                if (_daddy._od._OfflineData.ContainsKey(geo._Code))
	                {
	                    ocd = _daddy._od._OfflineData[geo._Code];
	                }
	                
	                
	            	// Truc par défaut
	            	_daddy._cacheDetail.LoadPageCache(geo, _daddy._bUseKm, ocd, false, false);
	            }
	            catch (Exception ex)
	            {
	            	_daddy.ShowException("", "Cache description", ex);
	            }
        	}
        	else if (url.StartsWith("mgmgeom:"))
        	{
				String code = url.Replace("mgmgeom:","");
				code = code.Replace("/","");
				code = code.ToUpper();
				try
	            {
	                Geocache geo = _daddy._caches[code];
	            	TabPage map = _daddy.ShowCacheMapInCacheDetail();
                	_daddy._cacheDetail._gmap.Position = new PointLatLng(geo._dLatitude, geo._dLongitude);
                	if (map != null)
                		_daddy._cacheDetail.tabControlCD.SelectTab(map);
	            }
	            catch (Exception ex)
	            {
	            	_daddy.ShowException("", "Display cache on map", ex);
	            }
        	}
        	else if (url.StartsWith("mgmgeomxy:"))
        	{
				String xy = url.Replace("mgmgeomxy:","");
				xy = xy.Replace("/","");
				xy = xy.ToUpper();
				String[] latlon = xy.Split('#');
				try
	            {
					double lat = MyTools.ConvertToDouble(latlon[0]);
					double lon = MyTools.ConvertToDouble(latlon[1]);
	                TabPage map = _daddy.ShowCacheMapInCacheDetail();
                	_daddy._cacheDetail._gmap.Position = new PointLatLng(lat, lon);
                	if (map != null)
                		_daddy._cacheDetail.tabControlCD.SelectTab(map);
	            }
	            catch (Exception ex)
	            {
	            	_daddy.ShowException("", "Display coord on maps", ex);
	            }
        	}
        	else
        	{
        		base.OnNavigating(e);
        	}
			
		}
		
	}
}
