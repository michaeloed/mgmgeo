using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGeocachingManager.Geocaching.Filters
{
    /// <summary>
    /// Display all caches without popularity
    /// </summary>
    public class CustomFilterNoStats : CacheFilter
    {
        /// <summary>
        /// Check if a cache shall be displayed based on filter definition
        /// </summary>
        /// <param name="cache">Cache to be checked</param>
        /// <returns>true if cache shall be displayed (passes the filter definition)</returns>
        public override bool ToBeDisplayed(Geocache cache)
        {
            // If no OCD or no stats, display it
            if ((cache._Ocd == null) || (cache._Ocd._iNbFavs == -1) || (cache._Ocd._dRating == -1.0) || (cache._Ocd._dRatingSimple == -1.0))
                return true;
            else
                return false;
        }
    }
}
