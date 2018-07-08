using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGeocachingManager.Geocaching.Filters
{
    /// <summary>
    /// This filter will display only caches that are missing OR only caches that are not missing.
    /// To identify caches that are potentially missing, MGM uses the following criteria:
    /// If within the 5 last logs:
    /// - there is at least _minNbDNF negative conditions
    /// - and no positive condition more recent than the last negative condition
    /// Then the cache is flagged missing
    /// Note : Logs are ordered from the most recent to the most older
    /// 
    /// Negative conditions are:
    /// 0 : "didn't find it"
    /// 1 : "needs archived"
    /// 2 : "needs maintenance"
    /// 
    /// Positive conditions are:
    /// 0 : "found it"
    /// 1 : "enable listing"
    /// 2 : "owner maintenance"
    /// 3 : "update coordinates"
    /// 4 : "unarchive"
    /// </summary>
    public class CustomFilterExcludeMissingCaches : CacheFilter
    {
        int _minNbDNF = 3;
        bool _displayExcludedOnly = true;
        bool[] _tConditionsNegatives = null;
        bool[] _tConditionsPositives = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tConditionsNegatives">List of negative conditions to be used, based on this order:
        /// 0 : "didn't find it"
        /// 1 : "needs archived"
        /// 2 : "needs maintenance"
        /// </param>
        /// <param name="tConditionsPositives">List of positive conditions to be used, based on this order:
        /// 0 : "found it"
        /// 1 : "enable listing"
        /// 2 : "owner maintenance"
        /// 3 : "update coordinates"
        /// 4 : "unarchive"
        /// </param>
        /// <param name="minNbDNF">Minimum number of negative conditions</param>
        /// <param name="bDisplayOnlyExcluded">
        /// If true, display only missing caches,
        /// If false, exlcudes missing caches.</param>
        public CustomFilterExcludeMissingCaches(bool[] tConditionsNegatives, bool[] tConditionsPositives, int minNbDNF, bool bDisplayOnlyExcluded)
        {
            _minNbDNF = minNbDNF;
            _displayExcludedOnly = bDisplayOnlyExcluded;
            _tConditionsNegatives = tConditionsNegatives;
            _tConditionsPositives = tConditionsPositives;
        }

        /// <summary>
        /// Check if a cache shall be displayed based on filter definition
        /// </summary>
        /// <param name="tConditionsNegatives">List of negative conditions to be used, based on this order:
        /// 0 : "didn't find it"
        /// 1 : "needs archived"
        /// 2 : "needs maintenance"
        /// </param>
        /// <param name="tConditionsPositives">List of positive conditions to be used, based on this order:
        /// 0 : "found it"
        /// 1 : "enable listing"
        /// 2 : "owner maintenance"
        /// 3 : "update coordinates"
        /// 4 : "unarchive"
        /// </param>
        /// <param name="minNbDNF">Minimum number of negative conditions</param>
        /// <param name="cache">Cache to be checked</param>
        /// <returns>True if cache can be displayed</returns>
        static public bool ToBeDisplayed(bool[] tConditionsNegatives, bool[] tConditionsPositives, int minNbDNF, Geocache cache)
        {
            // Le principe est le suivant :
            // Si dans les 5 derniers logs il y a au moins _minNbDNF conditions négatives et aucune condition positive près les négatives,
            // alors on filtre (exclusion) la cache.
            // Note : les logs sont classés du plus récent au plus ancien
            // _tConditionsNegatives:
            // 0 : "didn't find it"
            // 1 : "needs archived"
            // 2 : "needs maintenance"
            // _tConditionsPositives:
            // 0 : "found it"
            // 1 : "enable listing"
            // 2 : "owner maintenance"
            // 3 : "update coordinates"
            // 4 : "unarchive"
            int iNbDNFSoFar = 0;
            bool bMissingCache = false;
            foreach (CacheLog log in cache._Logs)
            {
                String type = log._Type.ToLower();
                if (tConditionsPositives[0] && (type == "found it"))
                    break;
                else if (tConditionsPositives[1] && (type == "enable listing"))
                    break;
                else if (tConditionsPositives[2] && (type == "owner maintenance"))
                    break;
                else if (tConditionsPositives[3] && (type == "update coordinates"))
                    break;
                else if (tConditionsPositives[4] && (type == "unarchive"))
                    break;
                else if (tConditionsNegatives[0] && (type == "didn't find it"))
                    iNbDNFSoFar++;
                else if (tConditionsNegatives[1] && (type == "needs archived"))
                    iNbDNFSoFar++;
                else if (tConditionsNegatives[2] && (type == "needs maintenance"))
                    iNbDNFSoFar++;

                if (iNbDNFSoFar >= minNbDNF)
                {
                    // On considère la cache comme disparue !!
                    bMissingCache = true;
                    break;
                }
                // Sinon on continue à compter
            }

            
            // on garde seulement les caches présentes
            if (!bMissingCache)
                return true; // on affiche la cache
            else
                return false; // on l'exclut
        }

        /// <summary>
        /// Check if a cache shall be displayed based on filter definition
        /// </summary>
        /// <param name="cache">Cache to be checked</param>
        /// <returns>true if cache shall be displayed (passes the filter definition)</returns>
        public override bool ToBeDisplayed(Geocache cache)
        {
            bool bVisible = CustomFilterExcludeMissingCaches.ToBeDisplayed(_tConditionsNegatives, _tConditionsPositives, _minNbDNF, cache);
            if (_displayExcludedOnly)
            {
                // on garde seulement les caches disparues
                return !bVisible;
            }
            else
            { 
                // on garde seulement les caches présentes
                return bVisible;
            }
        }
    }
}
