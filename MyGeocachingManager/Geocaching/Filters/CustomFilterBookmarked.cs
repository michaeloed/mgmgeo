using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGeocachingManager.Geocaching.Filters
{
    /// <summary>
    /// Display all caches that are bookmarked (_bBookmarked of OfflineCacheData)
    /// </summary>
    public class CustomFilterBookmarked : CacheFilter
    {
        /// <summary>
        /// Reference to OfflineData object
        /// </summary>
        public OfflineData _od = null;

        /// <summary>
        /// Check if a cache shall be displayed based on filter definition
        /// </summary>
        /// <param name="cache">Cache to be checked</param>
        /// <returns>true if cache shall be displayed (passes the filter definition)</returns>
        public override bool ToBeDisplayed(Geocache cache)
        {

            if (_od != null)
            {
                if (_od._OfflineData.ContainsKey(cache._Code))
                {
                    OfflineCacheData ocd = _od._OfflineData[cache._Code];
                    if (ocd._bBookmarked)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }
    }
}
