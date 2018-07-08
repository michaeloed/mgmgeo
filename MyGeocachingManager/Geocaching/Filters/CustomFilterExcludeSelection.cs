using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace MyGeocachingManager.Geocaching.Filters
{
    /// <summary>
    /// Display all caches
    /// </summary>
    public class CustomFilterExcludeSelection : CacheFilter
    {
        HashSet<String> _hs = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hs">List of caches codes that shall be excluded from display</param>
        public CustomFilterExcludeSelection(HashSet<String> hs)
        {
            _hs = hs;
        }

        /// <summary>
        /// Check if a cache shall be displayed based on filter definition
        /// </summary>
        /// <param name="cache">Cache to be checked</param>
        /// <returns>true if cache shall be displayed (passes the filter definition)</returns>
        public override bool ToBeDisplayed(Geocache cache)
        {
            if (_hs.Contains(cache._Code))
                return false;
            else
                return true;
        }
    }
}
