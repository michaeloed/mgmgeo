using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGeocachingManager.Geocaching.Filters
{
    /// <summary>
    /// Display all caches that are not translated
    /// </summary>
    public class CustomFilterNonTranslated : CacheFilter
    {
        /// <summary>
        /// Check if a cache shall be displayed based on filter definition
        /// </summary>
        /// <param name="cache">Cache to be checked</param>
        /// <returns>true if cache shall be displayed (passes the filter definition)</returns>
        public override bool ToBeDisplayed(Geocache cache)
        {
            // for logs & descriptions
            String keyBing = "<br><b><font color=#FF0000>[TRANSLATION - START]</font></b>";

            // for hint
            String keyBing2 = "[TRANSLATION - START]";

            // display only caches NOT translated
            if (IsTranslated(cache._LongDescription, keyBing))
                return false;

            if (IsTranslated(cache._ShortDescription, keyBing))
                return false;

            if (IsTranslated(cache._Hint, keyBing2))
                return false;

            foreach (CacheLog log in cache._Logs)
            {
                if (IsTranslated(log._Text, keyBing2))
                    return false;
            }

            return true;
        }

        private bool IsTranslated(String inputString, String keyBing)
        {
            if (inputString == "")
                return false;
            int ipos = inputString.IndexOf(keyBing);
            if (ipos != -1)
            {
                // found !
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
