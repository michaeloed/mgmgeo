using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyGeocachingManager.Geocaching.Filters
{
    /// <summary>
    /// Filter that will execute several filters
    /// A cache will be displayed if AT LEAST ONE filter authorize this cache to be displayed
    /// </summary>
    public class ChainedFiltersOR : CacheFilter
    {
        CacheFilter[] filters = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Collection of checked filters in the associated TabPage</param>
        public ChainedFiltersOR(CheckedListBox.CheckedItemCollection items)
        {
            if ((items != null) && (items.Count != 0))
            {
                filters = new CacheFilter[items.Count];
                int i = 0;
                foreach (object obj in items)
                {   
                    filters[i] = obj as CacheFilter;
                    i++;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Collection of checked filters in the associated TabPage</param>
        /// <param name="fltForPlaceholder">Filter to use if placeholder is found in collection</param>
        public ChainedFiltersOR(CheckedListBox.CheckedItemCollection items, CacheFilter fltForPlaceholder)
        {
            if ((items != null) && (items.Count != 0))
            {
                filters = new CacheFilter[items.Count];
                int i = 0;
                foreach (object obj in items)
                {   
                    filters[i] = obj as CacheFilter;
                    if (filters[i]._bToIgnore && (fltForPlaceholder != null))
                    	filters[i] = fltForPlaceholder;
                    i++;
                }
            }
        }
 
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">List of CacheFilter to be used (order will defines order of execution)</param>
        public ChainedFiltersOR(List<CacheFilter> items)
        {
            if ((items != null) && (items.Count != 0))
            {
                filters = new CacheFilter[items.Count];
                int i = 0;
                foreach (CacheFilter obj in items)
                {
                    filters[i] = obj;
                    i++;
                }
            }
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">List of CacheFilter to be used (order will defines order of execution)</param>
        public ChainedFiltersOR(CacheFilter[] items)
        {
        	if ((items != null) && (items.Count() != 0))
            {
        		filters = new CacheFilter[items.Count()];
                int i = 0;
                foreach (CacheFilter obj in items)
                {
                    filters[i] = obj;
                    i++;
                }
            }
        }

        /// <summary>
        /// Check if a cache shall be displayed based on filter definition
        /// </summary>
        /// <param name="cache">Cache to be checked</param>
        /// <returns>true if cache shall be displayed (passes the filter definition)</returns>
        public override bool ToBeDisplayed(Geocache cache)
        {
            if (filters == null)
                return false;
            foreach (CacheFilter filter in filters)
            {
                if ((filter != null)&&(filter.ToBeDisplayed(cache)))
                    return true;
            }
            return false;
        }
    }
}
