﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGeocachingManager.Geocaching.Filters
{
    /// <summary>
    /// Display all caches that have been modified by MGM
    /// </summary>
    public class CustomFilterModified : CacheFilter
    {
        /// <summary>
        /// Check if a cache shall be displayed based on filter definition
        /// </summary>
        /// <param name="cache">Cache to be checked</param>
        /// <returns>true if cache shall be displayed (passes the filter definition)</returns>
        public override bool ToBeDisplayed(Geocache cache)
        {
            return cache.HasBeenModified;
        }
    }
}