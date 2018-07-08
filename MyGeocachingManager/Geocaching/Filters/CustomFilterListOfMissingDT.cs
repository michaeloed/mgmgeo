/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 06/07/2016
 * Time: 18:51
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyGeocachingManager.Geocaching.Filters
{
	/// <summary>
	/// Description of CustomFilterListOfMissingDT.
	/// </summary>
	public class CustomFilterListOfMissingDT : CacheFilter
	{
		List<String> _missingDT = null;
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="missingDT">list of missing DT</param>
		public CustomFilterListOfMissingDT(List<String> missingDT)
		{
			_missingDT = missingDT;
		}
		
		/// <summary>
        /// Check if a cache shall be displayed based on filter definition
        /// </summary>
        /// <param name="cache">Cache to be checked</param>
        /// <returns>true if cache shall be displayed (passes the filter definition)</returns>
        public override bool ToBeDisplayed(Geocache cache)
        {
        	String aDT = cache._D + cache._T;
        	if (_missingDT.Contains(aDT))
        		return true;
            else
                return false;
        }
	}
}
