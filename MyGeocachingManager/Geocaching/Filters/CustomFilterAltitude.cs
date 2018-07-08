using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGeocachingManager.Geocaching.Filters
{
    /// <summary>
    /// Display all caches based on their altitude
    /// </summary>
    public class CustomFilterAltitude : CacheFilter
    {
        double _altitude = 0.0;
        int _comparison = 0; // 0 >=, 1 =, 2 <=

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="altitude">Altitude to check against, in meters</param>
        /// <param name="comparison">Comparison to perform
        /// 0: cache altitude shall be >= provided altitude
        /// 1: cache altitude shall be = provided altitude
        /// 2: cache altitude shall be inferior or equal to provided altitude
        /// </param>
        public CustomFilterAltitude(double altitude, string comparison)
        {
            _altitude = altitude;
            if (comparison == ">=")
                _comparison = 0;
            else if (comparison == "=")
                _comparison = 1;
            else if (comparison == "<=")
                _comparison = 2;
        }

        /// <summary>
        /// Check if a cache shall be displayed based on filter definition
        /// </summary>
        /// <param name="cache">Cache to be checked</param>
        /// <returns>true if cache shall be displayed (passes the filter definition)</returns>
        public override bool ToBeDisplayed(Geocache cache)
        {
            if ((cache._Ocd != null) && (cache._Ocd._dAltiMeters != Double.MaxValue))
            {
                if ((_comparison == 0) && (cache._Ocd._dAltiMeters >= _altitude))
                    return true;
                else if ((_comparison == 1) && (cache._Ocd._dAltiMeters == _altitude))
                    return true;
                else if ((_comparison == 2) && (cache._Ocd._dAltiMeters <= _altitude))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
    }
}
