using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGeocachingManager.Geocaching.Filters
{
    /// <summary>
    /// Display all caches without offline data (no valid OfflineCacheData attribute)
    /// OfflineCacheData contains many extra information for caches, such as statistics, spoilers, images, etc... 
    /// </summary>
    public class CustomFilterNoOfflineData : CacheFilter
    {
        /// <summary>
        /// reference to an OfflineData element, shall contains the list of OfflineCacheData stored in MainWindow
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
                    if (ocd._NotDownloaded)
                        return true;
                    else
                        return false;
                }
                else
                    return true;
            }
            else
                return true;
        }
    }
}
