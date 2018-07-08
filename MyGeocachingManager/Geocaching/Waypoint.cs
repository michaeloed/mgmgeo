using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Globalization;
using System.Resources;
using SpaceEyeTools;

namespace MyGeocachingManager.Geocaching
{
    /// <summary>
    /// Class defining a geocaching Waypoint
    /// </summary>
    public class Waypoint
    {

        /// <summary>
        /// Enumeration defining which type waypoint we are dealing with
        /// </summary>
        public enum WaypointOrigin
        {
            /// <summary>
            /// Waypoint comes from a GPX file
            /// </summary>
            GPX = 0,
            /// <summary>
            /// Waypoint comes from a GPX file and is modified
            /// </summary>
            MODIFIED = 1,
            /// <summary>
            /// Waypoint is a user created waypoint
            /// </summary>
            CUSTOM = 2
        };

        /// <summary>
        /// Waypoint origin
        /// </summary>
        public WaypointOrigin _eOrigin = WaypointOrigin.GPX;

        /// <summary>
        /// Geocaching code of parent cache
        /// </summary>
        public String _GCparent = "";

        /// <summary>
        /// Waypoint latitude
        /// </summary>
        public String _lat = "";

        /// <summary>
        /// Waypoint longitude
        /// </summary>
        public String _lon = "";

        /// <summary>
        /// Waypoint time of creation (from GPX)
        /// </summary>
        public String _time = "";

        /// <summary>
        /// Waypoint name (geocaching code)
        /// </summary>
        public String _name = "";

        /// <summary>
        /// Waypoint comment
        /// </summary>
        public String _cmt = "";

        /// <summary>
        /// Waypoint description
        /// </summary>
        public String _desc = "";

        /// <summary>
        /// Waypoint readable name
        /// </summary>
        public String _url = "";

        /// <summary>
        /// Waypoint url (from Geocaching.com)
        /// </summary>
        public String _urlname = "";

        /// <summary>
        /// Waypoint symbol
        /// </summary>
        public String _sym = "";

        /// <summary>
        /// Waypoint type
        /// </summary>
        public String _type = "";

        /// <summary>
        /// Last time waypoint was exported by MGM 
        /// </summary>
        public DateTime _DateExport;

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>String version of the object</returns>
        public override string ToString()
        {
            String s = "";
            s += "_GCparent:" + _GCparent + "\r\n";
            s += "_lat:" + _lat + "\r\n";
            s += "_lon:" + _lon + "\r\n";
            s += "_time:" + _time + "\r\n";
            s += "_name:" + _name + "\r\n";
            s += "_cmt:" + _cmt + "\r\n";
            s += "_desc:" + _desc + "\r\n";
            s += "_url:" + _url + "\r\n";
            s += "_urlname:" + _urlname + "\r\n";
            s += "_sym:" + _sym + "\r\n";
            s += "_type:" + _type + "\r\n";
            s += "_DateExport:" + _DateExport.ToShortDateString() + " " + _DateExport.ToShortTimeString() + "\r\n";
            
            return s;
        }

        /// <summary>
        /// Shall be called after waypoint creation and initialisation.
        /// Will Compute parent geocache code based on waypoint name.
        /// Will also translate waypoint types in Geocaching "English", needed if waypoint is created by C:Geo
        /// </summary>
        /// <param name="tableWptsTypeTranslated">table of translated waypoint types, to cope with C:Geo wird habit of translating types...</param>
        public void PostTreatmentData(Dictionary<String, String> tableWptsTypeTranslated)
        {
            if (_name.Length >= 3)
            {
                _GCparent = "GC" + _name.Substring(2);
            }

            // Il faudrait gérer les types qui ont été traduits, par exemple dans le cas de C:Geo
            foreach(KeyValuePair<String, String> paire in tableWptsTypeTranslated)
            {
                // On cherche un texte traduit
                if (paire.Value.Contains(_type))
                {
                    // on remplace par le texte anglais
                    _type = paire.Key;
                    break;
                }
            }
        }

        /// <summary>
        /// Return an XML "wpt" structure for the waypoint, compliant with Geocaching GPX format
        /// </summary>
        /// <returns>XML "wpt" structure for the waypoint, compliant with Geocaching GPX format</returns>
        public String ToGPXChunk()
        {
            String s = "";
            /*
  <wpt lat="48.77863" lon="1.924557">
    <time>2008-11-15T14:44:52.657</time>
    <name>W31J5NT</name>
    <cmt>La cache avec les coordonnées cryptées de la cache finale / Cache with encrypted coordinates of the final cache</cmt>
    <desc>Cryptage / Encryption</desc>
    <url>http://www.geocaching.com/seek/wpt.aspx?WID=141a45ca-39ce-4f1a-bd37-206bf33bc210</url> <== Option
    <urlname>Cryptage / Encryption</urlname> <== Option
    <sym>Stages of a Multicache</sym>
    <type>Waypoint|Stages of a Multicache</type>
  </wpt>
            */
            
            s += "  <wpt lat=\"" + _lat + "\" lon=\""+ _lon + "\">\r\n";
            s += "    <time>" + _time + "</time>\r\n";
            s += "    <name>" + _name + "</name>\r\n";

            if (_cmt != "")
                s += "    <cmt>" + MyTools.HtmlToXml(_cmt) + "</cmt>\r\n";
            else
                s += "    <cmt />\r\n";

            s += "    <desc>" + MyTools.HtmlToXml(_desc) + "</desc>\r\n";

            if (_url != "")
                s += "    <url>" + _url + "</url>\r\n";
            if (_urlname != "")
                s += "    <urlname>" + MyTools.HtmlToXml(_urlname) + "</urlname>\r\n";

            s += "    <sym>" + _sym + "</sym>\r\n";
            s += "    <type>Waypoint|" + _type + "</type>\r\n";
            s += "  </wpt>\r\n";
           
            return s;
        }
        
        /// <summary>
        /// Create a clone of the waypoint
        /// </summary>
        /// <returns>Cloned waypoint</returns>
        public Waypoint Clone()
        {
        	Waypoint w = new Waypoint();
        	w._cmt = this._cmt;
        	w._DateExport = this._DateExport;
        	w._desc = this._desc;
        	w._eOrigin = this._eOrigin;
        	w._GCparent = this._GCparent;
        	w._lat = this._lat;
        	w._lon = this._lon;
        	w._name = this._name;
        	w._sym = this._sym;
        	w._time = this._time;
        	w._type = this._type;
        	w._url = this._url;
        	w._urlname = this._urlname;
        	
        	return w;
        }
    }
}
