/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 04/02/2016
 * Time: 16:59
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;

namespace MyGeocachingManager.Geocaching.Filters
{
	/// <summary>
	/// Display only caches whose date of creation ends with one provided value
	/// </summary>
	public class CustomFilterDateCreation : CacheFilter
	{
		String[] _dates;
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="dates">list of matching dates (mm-dd)</param>
		public CustomFilterDateCreation(String[] dates)
		{
			_dates = dates;
			for(int i=0;i<_dates.Length;i++)
			{
				_dates[i] = "-" + _dates[i] + "T";
			}
		}
		
		
		/// <summary>
	    /// Check if a cache shall be displayed based on filter definition
	    /// </summary>
	    /// <param name="cache">Cache to be checked</param>
	    /// <returns>true if cache shall be displayed (passes the filter definition)</returns>
	    public override bool ToBeDisplayed(Geocache cache)
	    {
	    	foreach(String dt in _dates)
	    	{
	    		if (cache._DateCreation.Contains(dt))
	    			return true;
	    	}
	    	return false;
	    }
	}
	
}
