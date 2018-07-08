using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGeocachingManager
{
    /// <summary>
    /// Class definining a language translation in MGM
    /// </summary>
    public class LanguageItem
    {

        /// <summary>
        /// Path to translation file
        /// </summary>
        public String _path = "";

        /// <summary>
        /// Language name in target language (i.e. Français)
        /// </summary>
        public String _name = "";

        /// <summary>
        /// Language name in English (i.e. French)
        /// </summary>
        public String _ename = "";

        /// <summary>
        /// Language locale (i.e. fr-FR)
        /// </summary>
        public String _locale = "";
    }
}
