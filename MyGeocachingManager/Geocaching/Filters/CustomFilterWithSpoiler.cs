using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGeocachingManager.Geocaching.Filters
{
    /// <summary>
    /// Display all caches which have downloaded spoilers
    /// </summary>
    public class CustomFilterWithSpoiler : CacheFilter
    {
        /// <summary>
        /// Check if a cache shall be displayed based on filter definition
        /// </summary>
        /// <param name="cache">Cache to be checked</param>
        /// <returns>true if cache shall be displayed (passes the filter definition)</returns>
        public override bool ToBeDisplayed(Geocache cache)
        {
            if ((cache._Ocd != null) && (cache._Ocd._ImageFilesSpoilers.Count != 0))
                return true;
            else
                return false;
        }
    }
}
