using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GMap.NET;
using SpaceEyeTools;

namespace MyGeocachingManager.Geocaching.Filters
{
    /// <summary>
    /// Display all caches that are within a defined area
    /// </summary>
    public class CustomFilterArea : CacheFilter
    {
        List<PointLatLng[]> areasarray = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areas">List of list of points defining the area</param>
        public CustomFilterArea(List<List<PointLatLng>> areas)
        {
        	areasarray = new List<PointLatLng[]>();
        	foreach(List<PointLatLng> area in areas)
        	{
	            if (area.Count >= 3)
	            {
	            	areasarray.Add(area.ToArray());
	            }
        	}
        }

        /// <summary>
        /// Check if a cache shall be displayed based on filter definition
        /// </summary>
        /// <param name="cache">Cache to be checked</param>
        /// <returns>true if cache shall be displayed (passes the filter definition)</returns>
        public override bool ToBeDisplayed(Geocache cache)
        {
            if (areasarray != null)
            {
            	foreach(PointLatLng[] areaarray in areasarray)
            	{
                	PointLatLng ptcache = new PointLatLng(cache._dLatitude, cache._dLongitude);
                	if (PointInPolygon(ptcache, areaarray))
                		return true;
            	}
            }
            return false;
        }
        
        /// <summary>
        /// Check if a point is inside a polygon
        /// </summary>
        /// <param name="p">point to check</param>
        /// <param name="poly">polygon</param>
        /// <returns>true if point is present inside the polygon</returns>
        public static bool PointInPolygon(PointLatLng p, PointLatLng[] poly)
        {
            PointLatLng p1, p2;

            bool inside = false;

            if (poly.Length < 3)
            {
                return inside;
            }

            PointLatLng oldPoint = new PointLatLng(poly[poly.Length - 1].Lat, poly[poly.Length - 1].Lng);

            for (int i = 0; i < poly.Length; i++)
            {
                PointLatLng newPoint = new PointLatLng(poly[i].Lat, poly[i].Lng);

                if (newPoint.Lat > oldPoint.Lat)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.Lat < p.Lat) == (p.Lat <= oldPoint.Lat)
                && ((double)p.Lng - (double)p1.Lng) * (double)(p2.Lat - p1.Lat)
                 < ((double)p2.Lng - (double)p1.Lng) * (double)(p.Lat - p1.Lat))
                {
                    inside = !inside;
                }

                oldPoint = newPoint;
            }

            return inside;
        }
    }
}
