using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Drawing;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using SpaceEyeTools;
using SpaceEyeTools.Markdown;
using SpaceEyeTools.HMI;

namespace MyGeocachingManager.Geocaching
{
    /// <summary>
    /// Main classe of MGM :-)
    /// This is the class defining a Geocache
    /// </summary>
    public class Geocache
    {
        // To check for duplicates

        /// <summary>
        /// Last export date of Geocache
        /// </summary>
        public DateTime _DateExport = DateTime.Now;

        // Public var

        /// <summary>
        /// Geocaching code
        /// </summary>
        public String _Code = "";

        /// <summary>
        /// Cache name
        /// </summary>
        public String _Name = "";

        /// <summary>
        /// Cache url
        /// </summary>
        public String _Url = "";

        /// <summary>
        /// Cache owner
        /// </summary>
        public String _Owner = "";

        /// <summary>
        /// Cache creation date
        /// </summary>
        public String _DateCreation = "";

        /// <summary>
        /// Cache short decription
        /// </summary>
        public String _ShortDescription = "";

        /// <summary>
        /// Cache long decription
        /// </summary>
        public String _LongDescription = "";

        /// <summary>
        /// Cache difficulty
        /// </summary>
        public String _D = "";

        /// <summary>
        /// Cache terrain
        /// </summary>
        public String _T = "";

        /// <summary>
        /// Cache latitude
        /// </summary>
        public String _Latitude = "";

        /// <summary>
        /// Cache longitude
        /// </summary>
        public String _Longitude = "";

        /// <summary>
        /// Cache type
        /// </summary>
        public String _Type = "";

        /// <summary>
        /// Cache availability
        /// </summary>
        public String _Available = "True";

        /// <summary>
        /// Cache archived status
        /// </summary>
        public String _Archived = "False";

        /// <summary>
        /// Cache container size
        /// </summary>
        public String _Container = "";

        /// <summary>
        /// Cache hint (decoded)
        /// </summary>
        public String _Hint = "";

        /// <summary>
        /// Cache attributes
        /// </summary>
        public List<String> _Attributes = new List<string>();

        /// <summary>
        /// Cache logs
        /// </summary>
        public List<CacheLog> _Logs = new List<CacheLog>();

        /// <summary>
        /// True if cache has been found (information from GPX)
        /// </summary>
        public bool _bFound = false;

        /// <summary>
        /// True if cache has been found (information declared in MGM)
        /// </summary>
        public bool _bFoundInMGM = false;

        
        /// <summary>
        /// True if cache is owned by user
        /// </summary>
        public bool _bOwned = false;

        /// <summary>
        /// True if cache is manually selected by user in MGM
        /// </summary>
        public bool _bManualSelection = false;

        /// <summary>
        /// List of associated Travel bugs / Geocoins
        /// </summary>
        public Dictionary<String, String> _listTB = new Dictionary<string, string>();

        /// <summary>
        /// List of associated waypoints
        /// </summary>
        public Dictionary<String, Waypoint> _waypoints = new Dictionary<string, Waypoint>();

        /// <summary>
        /// List of extra waypoints : CUSTOM or MODIFIED origin
        /// </summary>
        public Dictionary<String, Waypoint> _waypointsFromMGM = new Dictionary<string, Waypoint>();


        /// <summary>
        /// Reference to associated OfflineCacheData
        /// </summary>
        public OfflineCacheData _Ocd = null;

        // Computed
        private Double _DistanceToHome = 0;
        private Double _DistanceToHomeMi = 0;
        private Double _dA = 0;
        private Double _dD = 0;
        private Double _dT = 0;
        private bool _bAvailable = true;
        private bool _bArchived = false;


        /// <summary>
        /// Return true if _bFound or _bFoundInMGM
        /// </summary>
        /// <returns>true if _bFound or _bFoundInMGM</returns>
        public bool IsFound()
        {
        	return (_bFound || _bFoundInMGM);
        }
        
        /// <summary>
        /// Retrieve origin of waypoint (_waypoints or _waypointsFromMGM)
        /// </summary>
        /// <param name="wpt">Waypoint to test</param>
        /// <returns>origin</returns>
        public Waypoint.WaypointOrigin GetWaypointOrigin(Waypoint wpt)
        {
        	if (_waypoints.ContainsKey(wpt._name))
            	return Waypoint.WaypointOrigin.GPX;
        	else
        		return wpt._eOrigin;
        }
        
        /// <summary>
        /// Retrieve merged list of waypoints (_waypoints and _waypointsFromMGM)
        /// </summary>
        /// <returns>merged dictionary</returns>
        public Dictionary<String, Waypoint> GetListOfWaypoints()
        {
        	Dictionary<String, Waypoint> dico = new Dictionary<string, Waypoint>();
        	foreach (KeyValuePair<String, Waypoint> paire in _waypoints)
            {
                Waypoint w = paire.Value;
                // A-t'on une version modifiée d'un WPT GPX issu de MGM ?
                if (_waypointsFromMGM.ContainsKey(paire.Key))
                {
                    // Oui, une version modifiée existe
                    w = _waypointsFromMGM[paire.Key];
                }
                dico.Add(w._name, w);
            }

            // Maintenant on ajoute ceux issus purement de MGM et non présent dans la cache originale
            foreach (KeyValuePair<String, Waypoint> paire in _waypointsFromMGM)
            {
                Waypoint w = paire.Value;
                // Est-ce une version modifiée d'un WPT GPX issu de MGM ?
                if (_waypoints.ContainsKey(paire.Key))
                {
                    // Oui, c'est une version modifiée
                    // On l'a déjà ajoutée..
                }
                else
                {
                    // Il est vraiment nouveau on l'ajoute
                    dico.Add(w._name, w);
                }
            }
            return dico;
        }
        
        /// <summary>
        /// Cache latitude
        /// </summary>
        public Double _dLatitude = 0.0;

        /// <summary>
        /// Cache longitude
        /// </summary>
        public Double _dLongitude = 0.0;

        /// <summary>
        /// Cache short description in HTML format
        /// </summary>
        public String _ShortDescriptionInHTML = "";

        /// <summary>
        /// Cache long description in HTML format
        /// </summary>
        public String _LongDescriptionInHTML = "";

        /// <summary>
        /// Cache attributes (readable)
        /// </summary>
        public String _txtAttributes = "";

        // For GPX export usage only

        /// <summary>
        /// Cache identifier
        /// </summary>
        public String _CacheId = "";

        /// <summary>
        /// Cache owner identifier
        /// </summary>
        public String _OwnerId = "";

        /// <summary>
        /// Cache country
        /// </summary>
        public String _Country = "";

        /// <summary>
        /// Cache state (from Country)
        /// </summary>
        public String _State = "";

        /// <summary>
        /// Name of geocacher who placed the cache
        /// </summary>
        public String _PlacedBy = "";

        /// <summary>
        /// Indicates if short desciption is available in HTML format
        /// </summary>
        public String _ShortDescHTML = "";

        /// <summary>
        /// Indicates if long desciption is available in HTML format
        /// </summary>
        public String _LongDescHTML = "";

        /// <summary>
        /// List of Travel bugs / geocoins identifiers
        /// </summary>
        public List<String> _listTBId = new List<string>();

        /// <summary>
        /// List of attributes identifiers
        /// </summary>
        public List<String> _listAttributesId = new List<string>();
        
        // Private stuff

        /// <summary>
        /// List of files containing this cache
        /// </summary>
        public List<String> _origin = new List<string>();

        /// <summary>
        /// Cache date of creation
        /// </summary>
        public DateTime _dtDateCreation = DateTime.MaxValue;

        /// <summary>
        /// Cache date of last log
        /// </summary>
        public DateTime _dtDateLastLog = DateTime.MaxValue;

        /// <summary>
        /// Contains modification keywords
        /// </summary>
        public List<String> _Modifications = new List<string>();

        /// <summary>
        /// Add a new modification
        /// </summary>
        /// <param name="key">modification keyword</param>
        /// <returns>1 if cache modified for the first time, 0 otherwise</returns>
        public int InsertModification(String key)
        {
            if (!_Modifications.Contains(key))
            {
                _Modifications.Add(key);
                return (_Modifications.Count() == 1) ? 1 : 0;
            }
            return 0;
        }

        /// <summary>
        /// Remove a modification
        /// </summary>
        /// <param name="key">modification keyword</param>
        /// <returns>1 if cache not modified anymore, 0 otherwise</returns>
        public int RemoveModification(String key)
        {
            if (_Modifications.Contains(key))
            {
                _Modifications.Remove(key);
                return (_Modifications.Count() == 0) ? 1 : 0;
            }
            return 0;
        }

        /// <summary>
        /// True if a cache has been modified
        /// </summary>
        public bool HasBeenModified
        {
            get
            {
                return (_Modifications.Count() != 0);
            }
        }

        MainWindow _daddy = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public Geocache(MainWindow daddy)
        {
            _daddy = daddy;
        }

        /// <summary>
        ///  Manually find a cache
        /// </summary>
        /// <returns>number of modifications applied to the cache</returns>
        public int ManualFound()
        {
        	if ((_bFound == false)&&(_bFoundInMGM == false))
			{
				_bFoundInMGM = true;
				return InsertModification("FOUND");
			}
        	else
        		return 0;
        }
        
        /// <summary>
        /// Return an XML "wpt" structure for the geocache, compliant with Geocaching GPX format
        /// </summary>
        /// <param name="bWithWaypoint">indicates if we want to include Geocache waypoints in export</param>
        /// <param name="iLengthWaypointBloc">return length of Waypoints bloc in bytes, used for GGZ export (index file generation)</param>
        /// <returns>XML "wpt" structure for the geocache, compliant with Geocaching GPX format</returns>
        public String ToGPXChunk(bool bWithWaypoint, ref int iLengthWaypointBloc/*, List<long> lstOffsetLengthWaypoints*/)
        {
            // <param name="lstOffsetLengthWaypoints">List of previous waypoints offset in generated file (used for GGZ export)</param>
            String s = "";

            s += "  <wpt lat=\"" + _Latitude + "\" lon=\"" + _Longitude + "\">\r\n";
         

            s += "    <time>" + _DateCreation + "</time>\r\n";
            s += "    <name>" + _Code + "</name>\r\n";
            s += "    <desc>" + MyTools.HtmlToXml(_Name) + " by " + MyTools.HtmlToXml(_PlacedBy) + ", " + _Type + " (" + _D + "/" + _T + ")</desc>\r\n";
            
            s += "    <url>" + _Url + "</url>\r\n";
            s += "    <urlname>" + MyTools.HtmlToXml(_Name) + "</urlname>\r\n";

           
            if (IsFound())
                s += "    <sym>Geocache Found</sym>\r\n";
            else
                s += "    <sym>Geocache</sym>\r\n";
        
            s += "    <type>Geocache|" + _Type + "</type>\r\n";
            s += "    <groundspeak:cache id=\"" + MyTools.HtmlToXml(_CacheId) // C'EST DU LUXE MAIS PARFOIS IL Y A DES TRUCS MERDIQUES ICI...
                + "\" available=\"" + _Available 
                + "\" archived=\"" + _Archived + "\" xmlns:groundspeak=\"http://www.groundspeak.com/cache/1/0/1\">\r\n";
            s += "      <groundspeak:name>" + MyTools.HtmlToXml(_Name) + "</groundspeak:name>\r\n";
            s += "      <groundspeak:placed_by>" + MyTools.HtmlToXml(_PlacedBy) + "</groundspeak:placed_by>\r\n";
            if (_OwnerId != "")
                s += "      <groundspeak:owner id=\"" + _OwnerId + "\">" + MyTools.HtmlToXml(_Owner) + "</groundspeak:owner>\r\n";
            else
                s += "      <groundspeak:owner>" + MyTools.HtmlToXml(_Owner) + "</groundspeak:owner>\r\n";
            s += "      <groundspeak:type>" + _Type + "</groundspeak:type>\r\n";
            s += "      <groundspeak:container>" + _Container + "</groundspeak:container>\r\n";
            if (_Attributes.Count == 0)
                s += "      <groundspeak:attributes />\r\n";
            else
            {
                s += "      <groundspeak:attributes>\r\n";
                for (int i = 0; i < _Attributes.Count; i++)
                {
                    String name = _Attributes[i].Replace("-no", "");
                    String inc = "1";
                    if (_Attributes[i].EndsWith("-no"))
                        inc = "0";
                    s += "        <groundspeak:attribute id=\"" + _listAttributesId[i] + "\" inc=\"" + inc + "\">" + name + "</groundspeak:attribute>\r\n";
                }
                s += "      </groundspeak:attributes>\r\n";
            }
            s += "      <groundspeak:difficulty>" + _D + "</groundspeak:difficulty>\r\n";
            s += "      <groundspeak:terrain>" + _T + "</groundspeak:terrain>\r\n";
            s += "      <groundspeak:country>" + _Country + "</groundspeak:country>\r\n";
            if (_State == "")
            {
                s += "      <groundspeak:state>\r\n      </groundspeak:state>\r\n";
            }
            else
            {
                s += "      <groundspeak:state>" + _State + "</groundspeak:state>\r\n";
            }
            
            String newShortDescription = _ShortDescription;
            // On ajoute les notes si besoin
            if ((_Ocd != null) && (String.IsNullOrEmpty(_Ocd._Comment) == false))
            {
            	newShortDescription = "<b>" + _daddy.GetTranslator().GetString("LblNote") + "</b>: " + _Ocd._Comment.Replace("\r\n","<br>").Replace("\n","<br>") + "<br>" + newShortDescription;
            }
            
            if (newShortDescription == "")
            {
                s += "      <groundspeak:short_description html=\"" + _ShortDescHTML + "\">\r\n      </groundspeak:short_description>\r\n";
            }
            else
            {
                String val = MyTools.HtmlToXml(newShortDescription);
                s += "      <groundspeak:short_description html=\"" + _ShortDescHTML + "\">" + val + "</groundspeak:short_description>\r\n";
            }
            
            if (_LongDescription == "")
            {
                s += "      <groundspeak:long_description html=\"" + _LongDescHTML + "\">\r\n      </groundspeak:long_description>\r\n";
            }
            else
            {
                String val = MyTools.HtmlToXml(_LongDescription);
                s += "      <groundspeak:long_description html=\"" + _LongDescHTML + "\">" + val + "</groundspeak:long_description>\r\n";
            }
            if (_Hint == "")
                s += "      <groundspeak:encoded_hints>\r\n      </groundspeak:encoded_hints>\r\n";
            else
                s += "      <groundspeak:encoded_hints>" + MyTools.HtmlToXml(_Hint) + "</groundspeak:encoded_hints>\r\n";
            s += "      <groundspeak:logs>\r\n";
            foreach (CacheLog log in _Logs)
            {
                s += log.ToGPXChunk();
            }
            s += "      </groundspeak:logs>\r\n";
            if (_listTB.Count == 0)
            {
                s += "      <groundspeak:travelbugs />\r\n";
            }
            else
            {
                s += "      <groundspeak:travelbugs>\r\n";
                int i=0;
                foreach (KeyValuePair<String, String> att in _listTB)
                {
                    s += "        <groundspeak:travelbug id=\"" + _listTBId[i] + "\" ref=\"" + att.Key + "\">\r\n";
                    s += "          <groundspeak:name>" + MyTools.HtmlToXml(att.Value) + "</groundspeak:name>\r\n";
                    s += "       </groundspeak:travelbug>\r\n";
                    i++;
                }
                s += "      </groundspeak:travelbugs>\r\n";
            }

            s += "    </groundspeak:cache>\r\n";

            // close the cache wpt
            s += "  </wpt>\r\n";

            // Les waypoints
            iLengthWaypointBloc = 0;
            if (bWithWaypoint)
            {
            	Dictionary<String, Waypoint> dicowpts = GetListOfWaypoints();
            	String wptstring = "";
            	foreach (KeyValuePair<String, Waypoint> paire in dicowpts)
                {
                    Waypoint w = paire.Value;
                   	wptstring = w.ToGPXChunk();
                    iLengthWaypointBloc += Encoding.UTF8.GetByteCount(wptstring);
                    s += wptstring;
                }
            }
            return s;
        }

        /// <summary>
        /// Update internal data after object creation, initialisation or modification
        /// </summary>
        public void UpdatePrivateData(String owner)
        {
            _dLatitude = MyTools.ConvertToDouble(_Latitude);
            _dLongitude = MyTools.ConvertToDouble(_Longitude);
            if (_Archived.ToLower() == "true")
                _bArchived = true;
            else
                _bArchived = false;

            if (_Available.ToLower() == "true")
                _bAvailable = true;
            else
                _bAvailable = false;

             _ShortDescriptionInHTML = _ShortDescription;
             if (_ShortDescHTML.ToLower() == "false")
             {
                 _ShortDescriptionInHTML = _ShortDescriptionInHTML.Replace("\r\n", "<br>");
                 _ShortDescriptionInHTML = _ShortDescriptionInHTML.Replace("\n", "<br>");
             }
             _LongDescriptionInHTML = _LongDescription;
             if (_LongDescHTML.ToLower() == "false")
             {
                 _LongDescriptionInHTML = _LongDescriptionInHTML.Replace("\r\n", "<br>");
                 _LongDescriptionInHTML = _LongDescriptionInHTML.Replace("\n", "<br>");
             }

            // Convert DateCreation & Date Last log into DateTime elements
             try
             {
                 _dtDateCreation = MyTools.ParseDate(MyTools.CleanDate(_DateCreation)); //DateTime.Parse(MyTools.CleanDate(_DateCreation));
             }
             catch (Exception)
             {
                 _dtDateCreation = DateTime.MaxValue;
             }
             try
             {
                 if (_Logs.Count != 0)
                     _dtDateLastLog = MyTools.ParseDate(MyTools.CleanDate(_Logs[0]._Date)); //DateTime.Parse(MyTools.CleanDate(_Logs[0]._Date));
                 else
                     _dtDateLastLog = DateTime.MaxValue;
             }
             catch (Exception)
             {
                 _dtDateLastLog = DateTime.MaxValue;
             }

            // To fasten, search in attributes
            String atts = "";
            foreach (String s in _Attributes)
                atts += s + ";";
            atts = atts.ToLower();
            atts = atts.Replace("/", "");
            _txtAttributes = atts;

            if (owner != "")
            {
            	if (owner == _Owner.ToLower())
            	{
            		_bOwned = true;
            	}
            	else
            	{
            		_bOwned = false;
            	}
            	
            }
            
            _dD = MyTools.ConvertToDouble(_D);
            _dT = MyTools.ConvertToDouble(_T);
            _dA = 0.0;
        }

        /// <summary>
        /// True is cache is archived
        /// </summary>
        /// <returns>True is cache is archived</returns>
        public bool getArchived()
        {
            return _bArchived;
        }

        /// <summary>
        /// True is cache is available
        /// </summary>
        /// <returns>True is cache is available</returns>
        public bool getAvailable()
        {
            return _bAvailable;
        }

        /// <summary>
        /// Return awesomeness
        /// </summary>
        /// <returns>awesomeness</returns>
        public Double getA()
        {
            return _dA;
        }

        /// <summary>
        /// Return difficulty
        /// </summary>
        /// <returns>difficulty</returns>
        public Double getD()
        {
            return _dD;
        }

        /// <summary>
        /// Return terrain
        /// </summary>
        /// <returns>terrain</returns>
        public Double getT()
        {
            return _dT;
        }

        private string getImgSrcHTML(String path, String cat, String prefix)
        {
            return "<img src=\"file:\\\\" + path + Path.DirectorySeparatorChar + cat + Path.DirectorySeparatorChar + prefix + ".gif\">";
        }

        private string getImgSrcHTML(String path, String cat, String prefix, String suffix)
        {
            return "<img src=\"file:\\\\" + path + Path.DirectorySeparatorChar + cat + Path.DirectorySeparatorChar + prefix + suffix + "\">";
        }
        

        /// <summary>
        /// Integrate waypoints list in geocache long description
        /// </summary>
        /// <returns>long description with waypoints integrated</returns>
        public String ReturnLongDescriptionInHTMLWithWpts()
        {
            return IncludeWaypointsIcons(_LongDescriptionInHTML);
        }

        private String IncludeWaypointsIcons(String description)
        {
        	if (description == null)
        		description = "";
        	
            bool bUseRealWpts = false;
            Dictionary<String, Waypoint> dicowpts = GetListOfWaypoints();
            if ((dicowpts != null) && (dicowpts.Count != 0))
                bUseRealWpts = true;
            string path = _daddy.GetResourcesDataPath() + Path.DirectorySeparatorChar + "Img";
            String imgin = getImgSrcHTML(path, "Wpts", "Invisible");
            String imgvi = getImgSrcHTML(path, "Wpts", "Visible");

            description = description.Replace("Additional Hidden Waypoints",
                "<HR><BR><b>" + _daddy.GetTranslator().GetString("LblAddiWpts") + "</b><BR>"
                + ((bUseRealWpts)?"<!--":"") // Pour masquer ces maudits waypoints ?
                );
            description = description.Replace("Additional Waypoints",
                "<HR><BR><b>" + _daddy.GetTranslator().GetString("LblAddWpts") + "</b><BR>"
                + ((bUseRealWpts)? "<!--" : "") // Pour masquer ces maudits waypoints ?
                );

            // Et maintenant la vraie liste des waypoints
            if (bUseRealWpts)
            {
                String sWaypointsDesc = "";
                sWaypointsDesc = "-->" + sWaypointsDesc; // Pour masquer ces maudits waypoints ?

                // On ajoute les waypoints
                foreach (KeyValuePair<String, Waypoint> paire in dicowpts)
                {
                    Waypoint w = paire.Value;
                   
                    String sym = w._sym;
                    String name = paire.Key;
                    String img = getImgSrcHTML(path, "Wpts", sym);
                    String title = w._desc;
                    String desc = w._cmt;
                    desc = desc.Replace("\r\n", "<br>");
                    desc = desc.Replace("\n", "<br>");

                    sWaypointsDesc += imgvi + "&nbsp;&nbsp;" + img + "&nbsp;&nbsp;<b>" + name + "</b> - " + title + "<BR>";
                    sWaypointsDesc += CoordConvHMI.ConvertDegreesToDDMM(MyTools.ConvertToDouble(w._lat), true) +
                        "&nbsp;" + CoordConvHMI.ConvertDegreesToDDMM(MyTools.ConvertToDouble(w._lon), false) + "<BR>";
                    if (desc != "")
                        sWaypointsDesc += desc + "<BR>";
                    sWaypointsDesc += "<BR>";
                }

                sWaypointsDesc += "<BR>";
                description += sWaypointsDesc;
            }

            return description;
        }

        /// <summary>
        /// Convert geocache in HTML for display in internal browser
        /// </summary>
        /// <param name="bUseKm">If true, distance will be in kilometers
        /// If false they will be in miles</param>
        /// <param name="ocd">Associated OfflineCacheData</param>
        /// <param name="bUseOfflineData">Indicate if OfflineCacheData shall be used</param>
        /// <returns>HTML string</returns>
        public String ToHTML(bool bUseKm, OfflineCacheData ocd, bool bUseOfflineData)
        {
            string path = _daddy.GetUserDataPath();
            // Load template html
            System.IO.StreamReader myFile =
                new System.IO.StreamReader(path + Path.DirectorySeparatorChar + "template_gc.dat");
            string myString = myFile.ReadToEnd();
            myFile.Close();

            myString = GenerateHtmlCode(bUseKm, ocd, bUseOfflineData, _daddy.GetResourcesDataPath() + Path.DirectorySeparatorChar + "Img", myString);

            return myString;
        }

        private string GenerateHtmlCode(bool bUseKm, OfflineCacheData ocd, bool bUseOfflineData, string path, string myString)
        {
            // Replace placeholders in file with correct values
            myString = myString.Replace("<#CODE#>", _Code);
            
            //if (_daddy.GetSupportedCacheTypes().Contains(_Type))
            if (MyTools.InsensitiveContainsInStringList(GeocachingConstants.GetSupportedCacheTypes(),_Type))
                myString = myString.Replace("<#TYPEIMG#>", getImgSrcHTML(path, "Type", _Type));
            else
                myString = myString.Replace("<#TYPEIMG#>", getImgSrcHTML(path, "", "Fail", ".png"));

            if (_Available.ToLower() == "true")
                myString = myString.Replace("<#NAME#>", _Name);
            else
                myString = myString.Replace("<#NAME#>", "<span style=\"text-decoration:line-through;\">" + _Name + "</span>");

            myString = myString.Replace("<#URLGC#>", _Url);
            myString = myString.Replace("<#VIEWONGC#>", _daddy.GetTranslator().GetString("HTMLViewGeocaching"));
            myString = myString.Replace("<#LATTXT#>", _daddy.GetTranslator().GetString("HTMLLatitude"));
            myString = myString.Replace("<#LATVAL#>", _Latitude);
            myString = myString.Replace("<#LONVAL#>", _Longitude);
            myString = myString.Replace("<#LONTXT#>", _daddy.GetTranslator().GetString("HTMLLongitude"));
            String link = "https://maps.google.com/maps?q=" + _Latitude + "+" + _Longitude;
            myString = myString.Replace("<#LINKMAPS#>", link);
            
            String internalmap = "<a href=\"MGMGEOMXY:" + _Latitude + "#" + _Longitude + "\">" + getImgSrcHTML(path, "", "Earth");
            myString = myString.Replace("<#VIEWMAPMGMLINK#>", internalmap);
            
            
            myString = myString.Replace("<#VIEWMAPSTXT#>", _daddy.GetTranslator().GetString("HTMLViewGmaps"));
            String notarch = (_Archived.ToLower() == "true") ? "False" : "True";
            myString = myString.Replace("<#AVAILTXT#>", _daddy.GetTranslator().GetString("HTMLAvailable"));
            myString = myString.Replace("<#AVAILIMG#>", getImgSrcHTML(path, "", _Available));
            myString = myString.Replace("<#NOTARCHTXT#>", _daddy.GetTranslator().GetString("HTMLNotArchived"));
            myString = myString.Replace("<#NOTARCHIMG#>", getImgSrcHTML(path, "", notarch));

            myString = myString.Replace("<#CREATOR#>", _PlacedBy);
            myString = myString.Replace("<#ACACHEBY#>", _daddy.GetTranslator().GetString("HTMLACacheBy"));
            myString = myString.Replace("<#HIDENAT#>", _daddy.GetTranslator().GetString("HTMLHidden"));
            myString = myString.Replace("<#DATECREATION#>", MyTools.CleanDate(_DateCreation));
            myString = myString.Replace("<#DIFFTXT#>", _daddy.GetTranslator().GetString("HTMLDifficulty"));
            myString = myString.Replace("<#DIFFIMG#>", getImgSrcHTML(path, "Star", _D));
            myString = myString.Replace("<#CONTAINERTXT#>", _daddy.GetTranslator().GetString("HTMLSize"));
            myString = myString.Replace("<#CONTAINERIMG#>", getImgSrcHTML(path, "Size", _Container));
            myString = myString.Replace("<#CONTAINER#>", _Container);
            myString = myString.Replace("<#FAVTXT#>", _daddy.GetTranslator().GetString("LVFavs"));
            myString = myString.Replace("<#FAVIMG#>", getImgSrcHTML(path, "", "Fav", ".png"));
            if ((ocd != null) && (ocd._iNbFavs != -1))
            {
                myString = myString.Replace("<#FAV#>", ocd._iNbFavs.ToString());
            }
            else
            {
                myString = myString.Replace("<#FAV#>", "?");
            }

            myString = myString.Replace("<#RATINGTXT#>", _daddy.GetTranslator().GetString("LVRating"));
            if ((ocd != null) && (ocd._dRating != -1.0))
            {
                myString = myString.Replace("<#RATINGIMG#>", getImgSrcHTML(path, "Ratios", "ratio_" + ((int)(ocd._dRating * 100.0)).ToString(), ".png"));
                //myString = myString.Replace("<#RATINGIMG#>", getImgSrcHTML(path, "Star", _daddy.RatingToImageStar(ocd._dRating)));
                myString = myString.Replace("<#RATING#>", ocd._dRating.ToString("0.0%"));
            }
            else
            {
                myString = myString.Replace("<#RATINGIMG#>", "");
                myString = myString.Replace("<#RATING#>", "?");
            }

            myString = myString.Replace("<#VISITTXT#>", _daddy.GetTranslator().GetString("LVFoundDNF"));
            int nbfound = -1;
            int nbdnf = -1;
            if (ocd != null)
            {
                nbfound = ocd._iNbFounds;
                nbdnf = ocd._iNbNotFounds;
            }
            if ((nbfound != -1) && (nbdnf != -1))
            {
                if (nbdnf == -1)
                    nbdnf = 0;
                if (nbfound == -1)
                    nbfound = 0;
                int total = (nbfound + nbdnf);
                String lbl = total.ToString() + " (" + nbfound.ToString() + "/" + nbdnf.ToString() + ")";
                myString = myString.Replace("<#VISIT#>", lbl);
            }
            else
            {
                myString = myString.Replace("<#VISIT#>", "?");
            }

            myString = myString.Replace("<#TERRTXT#>", _daddy.GetTranslator().GetString("HTMLTerrain"));
            myString = myString.Replace("<#TERRIMG#>", getImgSrcHTML(path, "Star", _T));
            myString = myString.Replace("<#LONLATDMS#>", CoordConvHMI.ConvertDegreesToDDMM(_dLatitude, true) + "&nbsp;" + CoordConvHMI.ConvertDegreesToDDMM(_dLongitude, false));
            myString = myString.Replace("<#WHERE#>", _daddy.GetTranslator().GetString("HTMLWhere"));
            myString = myString.Replace("<#STATE#>", _State);
            myString = myString.Replace("<#COUNTRY#>", _Country);
            myString = myString.Replace("<#DISTTXT#>", _daddy.GetTranslator().GetString("HTMLDistfromHome"));

            if (bUseKm)
            {
                myString = myString.Replace("<#DIST#>", String.Format("{0:0.#}", DistanceToHome()));
                myString = myString.Replace("<#DISTLBL#>", _daddy.GetTranslator().GetString("LVKm"));
            }
            else
            {
                myString = myString.Replace("<#DIST#>", String.Format("{0:0.#}", DistanceToHomeMi()));
                myString = myString.Replace("<#DISTLBL#>", _daddy.GetTranslator().GetString("LVMi"));
            }

            String sdesc = _ShortDescriptionInHTML;
            String ldesc = _LongDescriptionInHTML;
            String offdatapath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "Offline";
            if ((ocd != null) && (bUseOfflineData))
            {
                foreach (KeyValuePair<String, String> paire in ocd._ImageFilesFromDescription)
                {
                    sdesc = sdesc.Replace(paire.Key, "file:\\\\" + offdatapath + Path.DirectorySeparatorChar + paire.Value);
                    ldesc = ldesc.Replace(paire.Key, "file:\\\\" + offdatapath + Path.DirectorySeparatorChar + paire.Value);
                }
            }

            if ((ocd != null) && (ocd._Comment != ""))
            {
                String mynote = "<B>" + _daddy.GetTranslator().GetString("LblNote") + "</B>:<BR>" + ocd._Comment + "<BR><HR><BR>";
                mynote = mynote.Replace("\r\n", "\n");
                mynote = mynote.Replace("\n", "<BR>");
                myString = myString.Replace("<#MYNOTE#>", mynote);
            }
            else
                myString = myString.Replace("<#MYNOTE#>", "");

            myString = myString.Replace("<#SHORTDESC#>", sdesc);
            myString = myString.Replace("<#LONGDESC#>", IncludeWaypointsIcons(ldesc));

            String hint = _Hint;
            if (hint == null)
            	hint = "";
            hint = hint.Replace("\r\n", "<br>");
            hint = hint.Replace("\n", "<br>");
            myString = myString.Replace("<#HINT#>", hint);
            myString = myString.Replace("<#HINTTXT#>", _daddy.GetTranslator().GetString("HTMLHint"));

            myString = myString.Replace("<#IMGTXT#>", _daddy.GetTranslator().GetString("LblImage"));
            // Look for OfflineImageData
            if ((ocd != null) && (ocd._ImageFilesSpoilers.Count != 0))
            {
                String imglst = "";
                foreach (KeyValuePair<String, OfflineImageWeb> paire2 in ocd._ImageFilesSpoilers)
                {
                    if (!bUseOfflineData)
                    {
                        imglst += "<li><a href=" + paire2.Value._url + ">" + paire2.Value._name + "</a></li><br>\r\n";
                    }
                    else
                    {
                        imglst += "<li><a href=file:\\\\" + offdatapath + Path.DirectorySeparatorChar +
                            paire2.Value._localfile + ">" + paire2.Value._name + "</a></li><br>\r\n";
                    }
                }
                myString = myString.Replace("<#IMGLST#>", imglst);
            }
            else
            {
                myString = myString.Replace("<#IMGLST#>", _daddy.GetTranslator().GetString("LblNA"));
            }


            myString = myString.Replace("<#ATTRIBUTESTXT#>", _daddy.GetTranslator().GetString("HTMLAttributes"));
            String atts = "";
            if (_Attributes.Count != 0)
            {
                int iatt = 0;
                foreach (String att in _Attributes)
                {
                	String atttxt = _daddy.GetTranslator().GetString(_daddy.CreateAttributeTranslationKey(att.Replace("/", "")));
                    String att2 = att.Replace("/", "").ToLower();
                    String img = getImgSrcHTML(path, "Attribute", att2);
                    img = img.Replace("<img src=", "<img alt=\"" + atttxt + "\" src=");
                    atts += img + "&nbsp;";
                    iatt++;
                    if (iatt == 8)
                    {
                        iatt = 0;
                        atts += "<br>";
                    }
                }
            }
            
            myString = myString.Replace("<#ATTRIBUTESIMG#>", atts);

            myString = myString.Replace("<#TBGCTXT#>", _daddy.GetTranslator().GetString("HTMLTBGC"));
            String stbgc = "";
            if (_listTB.Count != 0)
            {
                foreach (KeyValuePair<String, String> tbgc in _listTB)
                {
                    // http://coord.info/TB377KQ
                    link = "http://coord.info/" + tbgc.Key;
                    stbgc += "<li>" + getImgSrcHTML(path, "", "TBORGC", ".png") + " " +
                        "<a href=" + link + ">" + tbgc.Key + "</a>: " +
                        tbgc.Value + "</li><br>";
                }
            }
            myString = myString.Replace("<#TBGC#>", stbgc);

            // stats for logs
            myString = myString.Replace("<#LOGSTATSFOUNDIMG#>", getImgSrcHTML(path, "Log", "Found it"));
            myString = myString.Replace("<#LOGSTATSNOTFOUNDIMG#>", getImgSrcHTML(path, "Log", "Didn't find it"));
            if ((ocd != null) && ( (ocd._iNbFounds != -1) || (ocd._iNbNotFounds != -1) ))
            {
                if (ocd._iNbFounds == -1)
                    myString = myString.Replace("<#LOGSTATSFOUND#>", "?");
                else
                    myString = myString.Replace("<#LOGSTATSFOUND#>", ocd._iNbFounds.ToString());
                if (ocd._iNbNotFounds == -1)
                    myString = myString.Replace("<#LOGSTATSNOTFOUND#>", "?");
                else
                    myString = myString.Replace("<#LOGSTATSNOTFOUND#>", ocd._iNbNotFounds.ToString());
            }
            else
            {
                myString = myString.Replace("<#LOGSTATSFOUND#>", "?");
                myString = myString.Replace("<#LOGSTATSNOTFOUND#>", "?");
            }

            // Logs
            String logs = "";
            if (_Logs.Count != 0)
            {
                foreach (CacheLog lgs in _Logs)
                {
                	logs += lgs.ToHTML();
                }
            }
            // Et ce maudit markdown si besoin
            //Markdown mk = new Markdown();
            //mk.AutoNewLines = true;
            //logs = mk.Transform(logs + "\r\n");
            
            myString = myString.Replace("<#LOGS#>", logs);

            String sori = "";
            foreach (String ori in _origin)
            {
                sori += ori + "<br>";
            }
            myString = myString.Replace("<#ORIGIN#>", sori);
            
            // Et maintenant on remplace les smileys
            var dicoSmileys = GCLogHMI.GetDicoSmileys(_daddy);
            foreach(KeyValuePair<String, String> pair in dicoSmileys)
            {
            	myString = myString.Replace(pair.Key, "<IMG SRC=\"" + pair.Value + "\" alt=\"" + pair.Key + "\">");
            }
            
            return myString;
        }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>String version of the object</returns>
        public override string ToString()
        {
            string s = "";
            s += "_DateExport: " + _DateExport.ToShortDateString() + " " + _DateExport.ToShortTimeString() + "\r\n";
            s += "_Code: " + _Code + "\r\n";
            s += "_Name: " + _Name + "\r\n";
            s += "_Url: " + _Url +"\r\n";
            s += "_Owner: " + _Owner +"\r\n";
            s += "_DateCreation: " + _DateCreation + "\r\n";
            s += "_ShortDescription: " + _ShortDescription + "\r\n";
            s += "_LongDescription: " + _LongDescription + "\r\n";
            s += "_D : " + _D +"\r\n";
            s += "_T: " + _T  +"\r\n";
            s += "_Latitude: " + _Latitude +"\r\n";
            s += "_Longitude: " +  _Longitude +"\r\n";
            s += "_Type: " + _Type + "\r\n";
            s += "_Available: " + _Available + "\r\n";
            s += "_Archived: " + _Archived + "\r\n";
            s += "_Container: " + _Container + "\r\n";
            s += "_Hint: " + _Hint + "\r\n";
            if (_Attributes.Count != 0)
            {
                s += "_Attributes:\r\n";
                foreach (String att in _Attributes)
                {
                    s += "    " + att + "\r\n";
                }
            }
            if (_Logs.Count != 0)
            {
                s += "_Logs:\r\n";
                foreach (CacheLog lgs in _Logs)
                {
                    s += "Log --> " + lgs + "\r\n";
                }
            }
            s += "_bFound: " + _bFound.ToString() + "\r\n";
            s += "_bFoundInMGM: " + _bFoundInMGM.ToString() + "\r\n";
            s += "_bOwned: " + _bOwned.ToString() + "\r\n";
            s += "_bManualSelection: " + _bManualSelection.ToString() + "\r\n";
            foreach (KeyValuePair<string, string> pair in _listTB)
            {
                s += "TB: " + pair.Key + " -> " + pair.Value + "\r\n";
            }
            foreach (KeyValuePair<string, Waypoint> pair in _waypoints)
            {
                s += "Waypoint: " + pair.Key + " -> " + pair.Value.ToString() + "\r\n";
            }
            foreach (KeyValuePair<string, Waypoint> pair in _waypointsFromMGM)
            {
                s += "WaypointFromMGM: " + pair.Key + " -> " + pair.Value.ToString() + "\r\n";
            }
            if (_Ocd != null)
                s += "_Ocd: " + _Ocd.ToString() + "\r\n";
            // La suite
            s += "_DistanceToHome: " + _DistanceToHome.ToString() + "\r\n";
            s += "_DistanceToHomeMi: " + _DistanceToHomeMi.ToString() + "\r\n";
            s += "_dA: " + _dA.ToString() + "\r\n";
            s += "_dD: " + _dD.ToString() + "\r\n";
            s += "_dT: " + _dT.ToString() + "\r\n";
            s += "_bAvailable: " + _bAvailable.ToString() + "\r\n";
            s += "_bArchived: " + _bArchived.ToString() + "\r\n";
            s += "_CacheId: " + _CacheId + "\r\n";
            s += "_OwnerId: " + _OwnerId + "\r\n";
            s += "_Country: " + _Country + "\r\n";
            s += "_State: " + _State + "\r\n";
            s += "_PlacedBy: " + _PlacedBy + "\r\n";
            foreach (String ori in _origin)
            {
                s += "Origin: " + ori +"\r\n";
            }
            s += "_Modifications:\r\n";
            foreach (String m in _Modifications)
            {
                s += "   " + m + "\r\n";
            }

            return s;
        }

        /// <summary>
        /// ToString without descriptions
        /// </summary>
        /// <returns>String version of the object</returns>
        public string ToShortString()
        {
            string s = "";
            s += "_DateExport: " + _DateExport.ToShortDateString() + " " + _DateExport.ToShortTimeString() + "\r\n";
            s += "_Code: " + _Code + "\r\n";
            s += "_Name: " + _Name + "\r\n";
            s += "_Url: " + _Url +"\r\n";
            s += "_Owner: " + _Owner +"\r\n";
            s += "_DateCreation: " + _DateCreation + "\r\n";
            s += "_D : " + _D +"\r\n";
            s += "_T: " + _T  +"\r\n";
            s += "_Latitude: " + _Latitude +"\r\n";
            s += "_Longitude: " +  _Longitude +"\r\n";
            s += "_Type: " + _Type + "\r\n";
            s += "_Available: " + _Available + "\r\n";
            s += "_Archived: " + _Archived + "\r\n";
            s += "_Container: " + _Container + "\r\n";
            s += "_Hint: " + _Hint + "\r\n";
            if (_Attributes.Count != 0)
            {
                s += "_Attributes:\r\n";
                foreach (String att in _Attributes)
                {
                    s += "    " + att + "\r\n";
                }
            }
            if (_Logs.Count != 0)
            {
                s += "_Logs:\r\n";
                foreach (CacheLog lgs in _Logs)
                {
                	s += "Log --> " + lgs.ToShortString() + "\r\n";
                }
            }
            s += "_bFound: " + _bFound.ToString() + "\r\n";
            s += "_bFoundInMGM: " + _bFoundInMGM.ToString() + "\r\n";
            s += "_bOwned: " + _bOwned.ToString() + "\r\n";
            s += "_bManualSelection: " + _bManualSelection.ToString() + "\r\n";
            foreach (KeyValuePair<string, string> pair in _listTB)
            {
                s += "TB: " + pair.Key + " -> " + pair.Value + "\r\n";
            }
            foreach (KeyValuePair<string, Waypoint> pair in _waypoints)
            {
                s += "Waypoint: " + pair.Key + " -> " + pair.Value.ToString() + "\r\n";
            }
            foreach (KeyValuePair<string, Waypoint> pair in _waypointsFromMGM)
            {
                s += "WaypointFromMGM: " + pair.Key + " -> " + pair.Value.ToString() + "\r\n";
            }
            if (_Ocd != null)
                s += "_Ocd: " + _Ocd.ToString() + "\r\n";
            // La suite
            s += "_DistanceToHome: " + _DistanceToHome.ToString() + "\r\n";
            s += "_DistanceToHomeMi: " + _DistanceToHomeMi.ToString() + "\r\n";
            s += "_dA: " + _dA.ToString() + "\r\n";
            s += "_dD: " + _dD.ToString() + "\r\n";
            s += "_dT: " + _dT.ToString() + "\r\n";
            s += "_bAvailable: " + _bAvailable.ToString() + "\r\n";
            s += "_bArchived: " + _bArchived.ToString() + "\r\n";
            s += "_CacheId: " + _CacheId + "\r\n";
            s += "_OwnerId: " + _OwnerId + "\r\n";
            s += "_Country: " + _Country + "\r\n";
            s += "_State: " + _State + "\r\n";
            s += "_PlacedBy: " + _PlacedBy + "\r\n";
            foreach (String ori in _origin)
            {
                s += "Origin: " + ori +"\r\n";
            }
            s += "_Modifications:\r\n";
            foreach (String m in _Modifications)
            {
                s += "   " + m + "\r\n";
            }

            return s;
        }
        
        /// <summary>
        /// Compute distance from home based on provided home location 
        /// </summary>
        /// <param name="homeLat">Home latitude</param>
        /// <param name="homeLon">Home longitude</param>
        public void UpdateDistanceToHome(double homeLat, double homeLon)
        {
            double k = DistanceToCoord(homeLat, homeLon);

            _DistanceToHome = k;
            _DistanceToHomeMi = k * 0.621371192;
        }

        /// <summary>
        /// Compute distance from location based on provided location in kilometer
        /// </summary>
        /// <param name="ptLat">Location latitude</param>
        /// <param name="ptLon">Location longitude</param>
        /// <returns>distance from location based on provided location in kilometer</returns>
        public double DistanceToCoord(double ptLat, double ptLon)
        {
            double lat = MyTools.ConvertToDouble(_Latitude);
            double lon = MyTools.ConvertToDouble(_Longitude);
            const double toRad = 3.1415926538 / 180;
            double e = toRad * lat;
            double f = toRad * lon;
            double g = toRad * ptLat;
            double h = toRad * ptLon;
            double i = (Math.Cos(e) * Math.Cos(g) * Math.Cos(f) * Math.Cos(h) + Math.Cos(e) * Math.Sin(f) * Math.Cos(g) * Math.Sin(h) + Math.Sin(e) * Math.Sin(g));
            double j = (Math.Acos(i));
            double k = (6371 * j);
            return k;
        }

        /// <summary>
        /// Return distance to home in kilometer
        /// </summary>
        /// <returns>distance to home in kilometer</returns>
        public Double DistanceToHome()
        {
            return _DistanceToHome;
        }

        /// <summary>
        /// Return distance to home in miles
        /// </summary>
        /// <returns>distance to home in miles</returns>
        public Double DistanceToHomeMi()
        {
            return _DistanceToHomeMi;
        }

        /// <summary>
        /// Check if cache should be completed, i.e. its description if empty
        /// </summary>
        /// <returns>true if cache description is empty</returns>
        public bool ShouldBeCompleted()
        {
            return (
                        ( (_LongDescription == "") || 
                          (_LongDescription == "none") || 
                          (_LongDescription == "Not available")
                        ) &&
                        ( (_ShortDescription == "") ||
                          (_ShortDescription == "none")
                        )
                    );
        }

        /// <summary>
        /// Check if cache is of event type (Event, Mega, Giga, CITO)
        /// </summary>
        /// <returns>True if cache is of event type</returns>
        public bool IsEventType()
        {
            if (
                (_Type == "Event Cache") ||
                (_Type == "Mega-Event Cache") ||
                (_Type == "Cache In Trash Out Event") ||
                (_Type == "Giga-Event Cache")
                )
                return true;
            else
                return false;
        }

        /// <summary>
        /// Check if cache is of event type (Event, Mega, Giga, CITO)
        /// </summary>
        /// <returns>True if cache is of event type</returns>
        public bool IsWebcamType() // BCR 20170822
        {
            if (
                (_Type == "Webcam Cache")
                )
                return true;
            else
                return false;
        }

        
        /// <summary>
        /// Clone a Geocache
        /// </summary>
        /// <returns>Cloned Geocache WITHOUT OCD NOR OPENCACHE !!!</returns>
        public Geocache Clone()
        {
            Geocache geo = new Geocache(_daddy);
            geo._Archived = _Archived;
            geo._Attributes = new List<string>();
            foreach (String s in _Attributes)
                geo._Attributes.Add(s);
            geo._Available = _Available;
            geo._bArchived = _bArchived;
            geo._bAvailable = _bAvailable;
            geo._bFound = _bFound;
            geo._bFoundInMGM = _bFoundInMGM;
            geo._bManualSelection = _bManualSelection;
            geo._bOwned = _bOwned;
            geo._CacheId = _CacheId;
            geo._Code = _Code;
            geo._Container = _Container;
            geo._Country = _Country;
            geo._D = _D;
            geo._dA = _dA;
            geo._DateCreation = _DateCreation;
            geo._DateExport = _DateExport;
            geo._dD = _dD;
            geo._DistanceToHome = _DistanceToHome;
            geo._DistanceToHomeMi = _DistanceToHomeMi;
            geo._dLatitude = _dLatitude;
            geo._dLongitude = _dLongitude;
            geo._dT = _dT;
            geo._dtDateCreation = _dtDateCreation;
            geo._dtDateLastLog = _dtDateLastLog;
            geo._Hint = _Hint;
            geo._Latitude = _Latitude;
            geo._listAttributesId = new List<string>();
            foreach (String s in _listAttributesId)
                geo._listAttributesId.Add(s);
            geo._listTB = new Dictionary<string, string>();
            foreach (KeyValuePair<String, String> pair in _listTB)
            {
                geo._listTB.Add(pair.Key, pair.Value);
            }
            geo._listTBId = new List<string>();
            foreach (String s in _listTBId)
                geo._listTBId.Add(s);
            geo._Logs = new List<CacheLog>();
            foreach (CacheLog log in _Logs)
                geo._Logs.Add(log.Clone());
            geo._LongDescHTML = _LongDescHTML;
            geo._LongDescription = _LongDescription;
            geo._LongDescriptionInHTML = _LongDescriptionInHTML;
            geo._Longitude = _Longitude;
            geo._Modifications = new List<string>();
            foreach (String s in _Modifications)
                geo._Modifications.Add(s);
            geo._Name = _Name;
            geo._Ocd = null;
            geo._origin = new List<string>();
            foreach (String s in _origin)
                geo._origin.Add(s);
            geo._Owner = _Owner;
            geo._OwnerId = _OwnerId;
            geo._PlacedBy = _PlacedBy;
            geo._ShortDescHTML = _ShortDescHTML;
            geo._ShortDescription = _ShortDescription;
            geo._ShortDescriptionInHTML = _ShortDescriptionInHTML;
            geo._State = _State;
            geo._T = _T;
            geo._txtAttributes = _txtAttributes;
            geo._Type = _Type;
            geo._Url = _Url;
            geo._waypoints = new Dictionary<string, Waypoint>();
            foreach (KeyValuePair<string, Waypoint> pair in _waypoints)
                geo._waypoints.Add(pair.Key, pair.Value.Clone());
            geo._waypointsFromMGM = new Dictionary<string, Waypoint>();
            foreach (KeyValuePair<string, Waypoint> pair in _waypointsFromMGM)
                geo._waypointsFromMGM.Add(pair.Key, pair.Value.Clone());
           
            return geo;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geo"></param>
        public void Update(Geocache geo)
        {
            _Archived = geo._Archived;
            _Attributes.Clear();
            foreach (String s in geo._Attributes)
                _Attributes.Add(s);
            _Available = geo._Available;
            _bArchived = geo._bArchived;
            _bAvailable = geo._bAvailable;
            _bFound = geo._bFound;
            _bFoundInMGM = geo._bFoundInMGM;
            _bManualSelection = geo._bManualSelection;
            _bOwned = geo._bOwned;
            _CacheId = geo._CacheId;
            _Code = geo._Code;
            _Container = geo._Container;
            _Country = geo._Country;
            _D = geo._D;
            _dA = geo._dA;
            _DateCreation = geo._DateCreation;
            _DateExport = geo._DateExport;
            _dD = geo._dD;
            _DistanceToHome = geo._DistanceToHome;
            _DistanceToHomeMi = geo._DistanceToHomeMi;
            _dLatitude = geo._dLatitude;
            _dLongitude = geo._dLongitude;
            _dT = geo._dT;
            _dtDateCreation = geo._dtDateCreation;
            _dtDateLastLog = geo._dtDateLastLog;
            _Hint = geo._Hint;
            _Latitude = geo._Latitude;
            
            _listAttributesId.Clear();
            foreach (String s in geo._listAttributesId)
                _listAttributesId.Add(s);
            
            _listTB.Clear();
            foreach (KeyValuePair<String, String> pair in geo._listTB)
            {
                _listTB.Add(pair.Key, pair.Value);
            }
            
            _listTBId.Clear();
            foreach (String s in geo._listTBId)
                _listTBId.Add(s);
            
            _Logs.Clear();
            foreach (CacheLog log in geo._Logs)
                _Logs.Add(log.Clone());
            
            _LongDescHTML = geo._LongDescHTML;
            _LongDescription = geo._LongDescription;
            _LongDescriptionInHTML = geo._LongDescriptionInHTML;
            _Longitude = geo._Longitude;
            
            _Modifications.Clear();
            foreach (String s in geo._Modifications)
                _Modifications.Add(s);
            
            _Name = geo._Name;
            _Ocd = null;
            
            _origin.Clear();
            foreach (String s in geo._origin)
                _origin.Add(s);
            
            _Owner = geo._Owner;
            _OwnerId = geo._OwnerId;
            _PlacedBy = geo._PlacedBy;
            _ShortDescHTML = geo._ShortDescHTML;
            _ShortDescription = geo._ShortDescription;
            _ShortDescriptionInHTML = geo._ShortDescriptionInHTML;
            _State = geo._State;
            _T = geo._T;
            _txtAttributes = geo._txtAttributes;
            _Type = geo._Type;
            _Url = geo._Url;
            
            _waypoints.Clear();
            foreach (KeyValuePair<string, Waypoint> pair in geo._waypoints)
                _waypoints.Add(pair.Key, pair.Value.Clone());
            
            _waypointsFromMGM.Clear();
            foreach (KeyValuePair<string, Waypoint> pair in geo._waypointsFromMGM)
                _waypointsFromMGM.Add(pair.Key, pair.Value.Clone());
        }
    }

    /// <summary>
    /// Mini instance of a Geocache with vital information
    /// </summary>
    public class MiniGeocache
    {
        /// <summary>
        /// Geocaching code
        /// </summary>
        public String _Code;

        /// <summary>
        /// Cache name
        /// </summary>
        public String _Name;

        /// <summary>
        /// Cache type
        /// </summary>
        public String _Type;

        /// <summary>
        /// Cache container size
        /// </summary>
        public String _Container;

        /// <summary>
        /// Cache difficulty
        /// </summary>
        public String _D;

        /// <summary>
        /// Cache terrain
        /// </summary>
        public String _T;

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>String version of the object</returns>
        public override string ToString()
        {
            String s = _Code + ";" + _Name.Replace(";", ",") + ";" + _Type + ";" + _Container + ";" + _D + ";" + _T;
            return s;
        }
    }
}

