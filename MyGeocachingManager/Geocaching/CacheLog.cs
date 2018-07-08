using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using SpaceEyeTools;

namespace MyGeocachingManager.Geocaching
{
    /// <summary>
    /// Class for a geocache log
    /// </summary>
    public class CacheLog
    {
        /// <summary>
        /// Log date
        /// </summary>
        public String _Date;
        
        /// <summary>
        /// Log type
        /// </summary>
        public String _Type;

        /// <summary>
        /// User who logged
        /// </summary>
        public String _User;

        /// <summary>
        /// Log text
        /// </summary>
        public String _Text;

        /// <summary>
        /// True if the log is encoded (ROT13)
        /// </summary>
        public String _Encoded;

        /// <summary>
        /// Internal key, not serialized to fasten log chronological sort
        /// </summary>
        public String _SortingKey = ""; // Not saved

        // For GPX export usage only

        /// <summary>
        /// Log id for GPX export usage only
        /// </summary>
        public String _LogId;

        /// <summary>
        /// Finder id for GPX export usage only
        /// </summary>
        public String _FinderId;

        MainWindow _daddy = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public CacheLog(MainWindow daddy)
        {
            _daddy = daddy;
        }

        /// <summary>
        /// Create a translator key for log type
        /// </summary>
        /// <param name="logType">log type</param>
        /// <returns>key to be used with a translator</returns>
        public String CreateLogTypeTranslationKey(String logType)
        {
            String s = "LOG_" + logType;
            s = s.Replace(" ", "_");
            s = s.Replace("'", "");
            return s;
        }

        private string getImgSrcHTML(String path, String prefix)
        {
            return "<img src=\"file:\\\\" + path + Path.DirectorySeparatorChar + "Log" + Path.DirectorySeparatorChar + prefix + ".gif\">";
        }

        /// <summary>
        /// Create a HTML bloc for this log (old fashioned)
        /// </summary>
        /// <returns>HTML bloc for this log (old fashioned)</returns>
        public String ToHTML()
        {
            string path = _daddy.GetResourcesDataPath() + Path.DirectorySeparatorChar + "Img";
            String s;
            String logtext = _Text;
            // Est-ce de l'HTML ?
            
            if (!logtext.ToLower().Contains("<br>"))
            {
            	// Ce n'est pas un log HTML
            	logtext = logtext.Replace("\r","");
            	logtext = logtext.Replace("\n","<br>");
            }
            
            s = "<br>" + getImgSrcHTML(path, _Type) + "<b>" + _daddy.GetTranslator().GetString(CreateLogTypeTranslationKey(_Type)) + "</b> (" +
                _User
                + ")" +
                "<span style=\"margin-left:30px;\">" + _Date.Substring(0, 10) + "</span><br>" + logtext + "<br>";

            return s;
        }

        /// <summary>
        /// Return an XML "groundspeak:log" structure for the log, compliant with Geocaching GPX format
        /// </summary>
        /// <returns>XML "groundspeak:log" structure for the log, compliant with Geocaching GPX format</returns>
        public String ToGPXChunk()
        {
            String s = "";
            /*
                    <groundspeak:log id="281005348">
                      <groundspeak:date>2012-11-18T20:00:00Z</groundspeak:date>
                      <groundspeak:type>Found it</groundspeak:type>
                      <groundspeak:finder id="1894610">nounie</groundspeak:finder>
                      <groundspeak:text encoded="False">La team barjos vient faire du caching dans le 78!
            Toujours en compagnie de sandrock et elgringosor, virée sympathique un peu humide le matin et plus tranquille l'après-midi à l'ouest.
            Des caches variées, bien pensées, astucieuses, avec différents décors, cela nous a permis de passer un bon moment à chercher des boiboites!
            Merci bellerive1 pour la cache!</groundspeak:text>
                    </groundspeak:log>

            */
            s += "        <groundspeak:log id=\"" + _LogId +"\">\r\n";
            s += "          <groundspeak:date>" + _Date + "</groundspeak:date>\r\n";
            s += "          <groundspeak:type>" + _Type + "</groundspeak:type>\r\n";
            if (_FinderId != "")
                s += "          <groundspeak:finder id=\"" + _FinderId + "\">" + MyTools.HtmlToXml(_User) + "</groundspeak:finder>\r\n";
            else
                s += "          <groundspeak:finder>" + MyTools.HtmlToXml(_User) + "</groundspeak:finder>\r\n";

            String val = MyTools.HtmlToXml(_Text);
            if (_Encoded.ToLower() == "true")
                s += "          <groundspeak:text encoded=\"True\">" + val + "</groundspeak:text>\r\n";
            else
                s += "          <groundspeak:text encoded=\"False\">" + val + "</groundspeak:text>\r\n";
            s += "        </groundspeak:log>\r\n";

            return s;
        }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>String version of the object</returns>
        public override string ToString()
        {
            string s = "_Date: " + _Date + "\r\n";
            s += "_Type: " + _Type + "\r\n";
            s += "_User: " + _User + "\r\n";
            s += "_Text: " + _Text + "\r\n";
           
            return s;
        }

        /// <summary>
        /// Clone current object
        /// </summary>
        /// <returns>cloned object</returns>
        public CacheLog Clone()
        {
            CacheLog log = new CacheLog(_daddy);
            log._Date = _Date;
            log._Encoded = _Encoded;
            log._FinderId = _FinderId;
            log._LogId = _LogId;
            log._SortingKey = _SortingKey;
            log._Text = _Text;
            log._Type = _Type;
            log._User = _User;

            return log;
        }

        /// <summary>
        /// ToString without text
        /// </summary>
        /// <returns>String version of the object</returns>
        public string ToShortString()
        {
            string s = "_Date: " + _Date + "\r\n";
            s += "_Type: " + _Type + "\r\n";
            s += "_User: " + _User + "\r\n";
            
            return s;
        }
    }
}
