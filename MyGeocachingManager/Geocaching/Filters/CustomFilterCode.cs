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
	/// Display only caches whose code is contained within a provided list
	/// </summary>
	public class CustomFilterCode : CacheFilter
	{
		List<String> _codes;
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="codes">list of matching codes</param>
		public CustomFilterCode(List<String> codes)
		{
			_codes = codes;
		}
		
		
		/// <summary>
	    /// Check if a cache shall be displayed based on filter definition
	    /// </summary>
	    /// <param name="cache">Cache to be checked</param>
	    /// <returns>true if cache shall be displayed (passes the filter definition)</returns>
	    public override bool ToBeDisplayed(Geocache cache)
	    {
	    	return _codes.Contains(cache._Code);
	    		
	    }
	}
	
}
